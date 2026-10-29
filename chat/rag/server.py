"""Test server for the heroine chat: the page, the stream, the debug data.

uv run uvicorn rag.server:app --port 8765
"""

import json
import os
import re
import time
from collections import defaultdict, deque
from contextlib import asynccontextmanager
from pathlib import Path

from fastapi import Depends, FastAPI, HTTPException, Request
from fastapi.responses import FileResponse, StreamingResponse
from pydantic import BaseModel, Field

from . import config
from .heroine import Heroine
from .index import Index
from .llm import LLM, ChatResult

HEROINE_LAYERS = ("fact", "setting")
WEB_DIR = Path(__file__).resolve().parent.parent / "web"


def reply_options() -> dict:
    """Short replies. GLM 5.3 Flash on OpenRouter cannot turn reasoning off, so it runs at
    the lowest effort (AI_REASONING=low|medium|high) and its text is not sent back.
    max_tokens covers reasoning plus the reply."""
    return {
        "max_tokens": 1500,
        "temperature": 0.8,
        "reasoning": {"effort": os.environ.get("AI_REASONING", "low").strip(), "exclude": True},
    }


class Message(BaseModel):
    role: str = Field(pattern="^(heroine|voice)$")
    content: str = Field(max_length=2000)


class ChatIn(BaseModel):
    history: list[Message] = Field(max_length=60)
    state: dict = {}
    memory: str = Field(default="", max_length=2000)
    layers: list[str] | None = None
    k: int = Field(default=6, ge=0, le=12)
    debug: bool = False


class SearchIn(BaseModel):
    q: str = Field(max_length=2000)
    year: int | None = None
    layers: list[str] | None = None
    k: int = Field(default=8, ge=1, le=30)


class SummaryIn(BaseModel):
    history: list[Message] = Field(max_length=60)
    memory: str = Field(default="", max_length=2000)


class ReportIn(BaseModel):
    message: str = Field(max_length=2000)
    reason: str = Field(default="", max_length=500)
    history: list[Message] = Field(default=[], max_length=60)
    state: dict = {}


ctx: dict = {}


def game_token(request: Request) -> None:
    """With CHAT_TOKEN set (the public server), the model calls need it in X-Game-Token (the game)
    or ?token= (the test page). It keeps out strangers who find the address, not someone who
    takes the APK apart; the per-address limit below is for them."""
    token = os.environ.get("CHAT_TOKEN", "")
    if token and token not in (request.headers.get("x-game-token"), request.query_params.get("token")):
        raise HTTPException(401, "no token")


_calls: dict[str, deque] = defaultdict(deque)


def rate_limit(request: Request) -> None:
    """At most CHAT_RATE model calls an hour from one address (0 = no limit)."""
    rate = int(os.environ.get("CHAT_RATE", "0"))
    if not rate:
        return
    who = request.headers.get("x-real-ip") or (request.client.host if request.client else "?")
    now = time.monotonic()
    calls = _calls[who]
    while calls and now - calls[0] > 3600:
        calls.popleft()
    if len(calls) >= rate:
        raise HTTPException(429, "too many messages")
    calls.append(now)


guarded = [Depends(game_token), Depends(rate_limit)]


@asynccontextmanager
async def lifespan(_: FastAPI):
    settings = config.load()
    ctx.update(
        settings=settings,
        index=Index.load(settings.data_dir),
        heroine=Heroine(settings.heroine_dir),
        llm=LLM(settings),
    )
    yield
    await ctx["llm"].close()


app = FastAPI(lifespan=lifespan)


@app.get("/")
async def page():
    return FileResponse(WEB_DIR / "index.html")


@app.get("/art/{name}")
async def art(name: str):
    if not re.fullmatch(r"[a-z0-9_]+", name):
        raise HTTPException(404)
    for folder in ctx["settings"].art_dirs:
        path = folder / f"{name}.jpeg"
        if path.exists():
            return FileResponse(path, headers={"Cache-Control": "max-age=86400"})
    raise HTTPException(404)


@app.get("/fonts/{name}")
async def font(name: str):
    if not re.fullmatch(r"[A-Za-z0-9-]+\.ttf", name):
        raise HTTPException(404)
    path = ctx["settings"].fonts_dir / name
    if not path.exists():
        raise HTTPException(404)
    return FileResponse(path, headers={"Cache-Control": "max-age=86400"})


@app.get("/kit/{name}")
async def kit(name: str):
    """The game's Blender UI kit (docs/.../heritage/png3d): the page is drawn with the same sprites as the game."""
    if not re.fullmatch(r"[a-z0-9-]+\.png|slices\.txt", name):
        raise HTTPException(404)
    path = ctx["settings"].kit_dir / name
    if not path.exists():
        raise HTTPException(404)
    return FileResponse(path, headers={"Cache-Control": "no-cache"})


@app.get("/api/night")
async def night():
    n = ctx["heroine"].night
    s = ctx["settings"]
    return {
        "id": n.id,
        "title": n.title,
        "year": n.year,
        "background": n.background,
        "limit": n.limit,
        "opening": n.opening,
        "opening_again": n.opening_again,
        "chips": n.chips,
        "wake": n.wake,
        "vars": n.vars,
        "layers": HEROINE_LAYERS,
        "index": ctx["index"].meta,
        "model": s.chat_model,
        "reasoning": reply_options()["reasoning"],
    }


@app.post("/api/state")
async def state_lines(state: dict):
    """What the heroine reads from these ink values; no model call."""
    return ctx["heroine"].state_lines(state)


@app.post("/api/search", dependencies=[Depends(game_token)])
async def search(body: SearchIn):
    vec = (await ctx["llm"].embed([body.q]))[0]
    hits = ctx["index"].search(
        vec,
        body.q,
        k=body.k,
        max_year=body.year,
        layers=tuple(body.layers) if body.layers else None,
    )
    return [h.to_dict() for h in hits]


@app.post("/api/chat", dependencies=guarded)
async def chat(body: ChatIn):
    heroine: Heroine = ctx["heroine"]
    llm: LLM = ctx["llm"]
    history = [m.model_dump() for m in body.history]
    if not history or history[-1]["role"] != "voice":
        raise HTTPException(400, "the last message must be the Voice's")
    state = heroine.clean_state(body.state)

    async def stream():
        started = time.monotonic()
        try:
            input_flags = heroine.check_input(history[-1]["content"])
            hits = []
            if body.k:
                q = heroine.query(history)
                vec = (await llm.embed([q]))[0]
                layers = tuple(body.layers) if body.layers else HEROINE_LAYERS
                hits = ctx["index"].search(vec, q, k=body.k, max_year=heroine.night.year, layers=layers)
            messages = heroine.messages(history, state, hits, body.memory)
            yield _sse(
                "sources",
                {
                    "query": heroine.query(history),
                    "hits": [h.to_dict() for h in hits],
                    "state_lines": heroine.state_lines(state),
                    "input_flags": input_flags,
                    "system": messages[0]["content"] if body.debug else None,
                    "retrieval_ms": round((time.monotonic() - started) * 1000),
                },
            )

            result = ChatResult()
            text = ""
            first = None
            async for delta in llm.stream_chat(messages, result, **reply_options()):
                first = first or time.monotonic()
                text += delta
                yield _sse("delta", {"text": delta})

            text = text.strip()
            flags = heroine.check_reply(text)
            done = {
                "flags": flags,
                "usage": result.usage,
                "finish_reason": result.finish_reason,
                "first_token_ms": round(((first or time.monotonic()) - started) * 1000),
                "total_ms": round((time.monotonic() - started) * 1000),
            }
            _log(
                "chats.jsonl",
                {
                    "history": history,
                    "state": state,
                    "memory": body.memory,
                    "reply": text,
                    "hits": [h.chunk.id for h in hits],
                    **done,
                },
            )
            yield _sse("done", done)
        except Exception as e:  # noqa: BLE001 — the page shows it; the stream must end cleanly
            yield _sse("error", {"message": str(e)})

    return StreamingResponse(
        stream(),
        media_type="text/event-stream",
        headers={"Cache-Control": "no-cache", "X-Accel-Buffering": "no"},
    )


@app.post("/api/summary", dependencies=guarded)
async def summary(body: SummaryIn):
    """What the heroine keeps of a dream: the game sends it with the next dream (memory)."""
    history = [m.model_dump() for m in body.history]
    if not any(m["role"] == "voice" for m in history):
        return {"memory": body.memory}
    result = ChatResult()
    text = ""
    async for delta in ctx["llm"].stream_chat(
        ctx["heroine"].summary_messages(history, body.memory),
        result,
        **{**reply_options(), "temperature": 0.3},
    ):
        text += delta
    text = _once(text.strip())[:1500]
    _log(
        "summaries.jsonl", {"history": history, "memory": body.memory, "summary": text, "usage": result.usage}
    )
    return {"memory": text or body.memory}


@app.post("/api/report", dependencies=[Depends(game_token)])
async def report(body: ReportIn):
    _log("reports.jsonl", body.model_dump())
    return {"ok": True}


def _once(text: str) -> str:
    """The model now and then writes its whole answer twice in a row; one copy is kept."""
    half = len(text) // 2
    for cut in (half, half + 1):
        if text[:cut].strip() and text[:cut].strip() == text[cut:].strip():
            return text[:cut].strip()
    return text


def _sse(event: str, data: dict) -> str:
    return f"event: {event}\ndata: {json.dumps(data, ensure_ascii=False)}\n\n"


def _log(name: str, record: dict) -> None:
    """A log that cannot be written must not take the reply away from the reader."""
    path = ctx["settings"].data_dir / name
    try:
        path.parent.mkdir(parents=True, exist_ok=True)
        with path.open("a", encoding="utf-8") as f:
            f.write(
                json.dumps({"at": time.strftime("%Y-%m-%d %H:%M:%S"), **record}, ensure_ascii=False) + "\n"
            )
    except OSError as e:
        print(f"log {name}: {e}", flush=True)
