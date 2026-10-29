"""OpenAI-compatible client: embeddings and streamed chat (OpenRouter by default)."""

import json
from collections.abc import AsyncIterator
from dataclasses import dataclass, field

import httpx
import numpy as np

from .config import Settings

EMBED_BATCH = 64


@dataclass
class ChatResult:
    """Filled in while a stream runs; read it after the stream ends."""

    usage: dict = field(default_factory=dict)
    finish_reason: str | None = None


class LLM:
    def __init__(self, settings: Settings):
        self.settings = settings
        self.http = httpx.AsyncClient(
            base_url=settings.base_url,
            proxy=settings.proxy,
            timeout=httpx.Timeout(120, connect=15),
            headers={
                "Authorization": f"Bearer {settings.api_key}",
                "X-Title": "Legacy Through Time: heroine chat",
            },
        )

    async def close(self) -> None:
        await self.http.aclose()

    async def embed(self, texts: list[str]) -> np.ndarray:
        """Unit-length float32 vectors, one row per text."""
        rows: list[list[float]] = []
        for i in range(0, len(texts), EMBED_BATCH):
            batch = texts[i : i + EMBED_BATCH]
            r = await self.http.post("/embeddings", json={"model": self.settings.embed_model, "input": batch})
            body = _json_or_raise(r)
            data = sorted(body["data"], key=lambda d: d["index"])
            if len(data) != len(batch):
                raise RuntimeError(f"embeddings: asked {len(batch)}, got {len(data)}")
            rows.extend(d["embedding"] for d in data)
        vecs = np.asarray(rows, dtype=np.float32)
        norms = np.linalg.norm(vecs, axis=1, keepdims=True)
        return vecs / np.maximum(norms, 1e-12)

    async def stream_chat(self, messages: list[dict], result: ChatResult, **options) -> AsyncIterator[str]:
        """Yields text deltas. Reasoning tokens are not yielded."""
        payload = {
            "model": self.settings.chat_model,
            "messages": messages,
            "stream": True,
            "usage": {"include": True},
            **options,
        }
        async with self.http.stream("POST", "/chat/completions", json=payload) as r:
            if r.status_code != 200:
                await r.aread()
                _json_or_raise(r)
            async for line in r.aiter_lines():
                if not line.startswith("data: "):
                    continue
                data = line[6:]
                if data == "[DONE]":
                    break
                chunk = json.loads(data)
                if "error" in chunk:
                    raise RuntimeError(f"chat: {chunk['error']}")
                if chunk.get("usage"):
                    result.usage = chunk["usage"]
                for choice in chunk.get("choices", []):
                    if choice.get("finish_reason"):
                        result.finish_reason = choice["finish_reason"]
                    text = (choice.get("delta") or {}).get("content")
                    if text:
                        yield text


def _json_or_raise(r: httpx.Response) -> dict:
    try:
        body = r.json()
    except ValueError:
        raise RuntimeError(f"{r.request.url.path}: HTTP {r.status_code}: {r.text[:300]}")
    if r.status_code != 200 or "error" in body:
        raise RuntimeError(f"{r.request.url.path}: HTTP {r.status_code}: {body.get('error', body)}")
    return body
