"""uv run python -m rag <command>

fetch-wiki      download the Wikipedia articles listed in corpus/wiki.yaml into corpus/wiki/
ingest          chunk corpus/, embed what changed, write data/index/
chunks          print the chunks without embedding (to check years and layers)
search "..."    hybrid search, as the hero of a story would see it
ask "..."       one line from the reader, the hero's reply in the terminal
"""

import argparse
import asyncio
import re
from pathlib import Path

import httpx
import yaml

from . import config
from .corpus import load_corpus
from .hero import Hero, load_heroes
from .index import Index
from .llm import LLM, ChatResult
from .server import HERO_LAYERS, reply_options

WIKI_API = "https://ru.wikipedia.org/w/api.php"


def hero_of(settings, story: str) -> Hero:
    heroes = load_heroes(settings.stories_dir, settings.rules_path.read_text(encoding="utf-8"))
    if story not in heroes:
        raise SystemExit(f"no chat for the story {story!r}; there are: {', '.join(heroes) or 'none'}")
    return heroes[story]


def main() -> None:
    p = argparse.ArgumentParser(prog="python -m rag")
    sub = p.add_subparsers(dest="cmd", required=True)
    sub.add_parser("fetch-wiki")
    sub.add_parser("ingest")
    sub.add_parser("chunks")
    s = sub.add_parser("search")
    s.add_argument("query")
    s.add_argument("--year", type=int, help="drop chunks about later events (default: the year of the hero)")
    s.add_argument("--story", default="golden-cage")
    s.add_argument("--all-layers", action="store_true")
    s.add_argument("-k", type=int, default=6)
    a = sub.add_parser("ask")
    a.add_argument("line")
    a.add_argument("--story", default="golden-cage")
    args = p.parse_args()

    settings = config.load()
    match args.cmd:
        case "fetch-wiki":
            fetch_wiki(settings.corpus_dir)
        case "chunks":
            for c in load_corpus(settings.corpus_dir):
                print(f"{c.id:28} {c.layer:8} {c.event_year!s:5} {len(c.text):4}  {c.heading[:50]}")
        case "ingest":
            asyncio.run(ingest(settings))
        case "search":
            asyncio.run(search(settings, args))
        case "ask":
            asyncio.run(ask(settings, args.line, args.story))


def fetch_wiki(corpus_dir: Path) -> None:
    """Articles as plain text, headings kept, `== X ==` turned into Markdown."""
    spec = yaml.safe_load((corpus_dir / "wiki.yaml").read_text(encoding="utf-8"))
    out_dir = corpus_dir / "wiki"
    out_dir.mkdir(exist_ok=True)
    skip = set(spec.get("skip_sections", []))
    # Wikimedia refuses requests without a User-Agent that says who is asking.
    ua = "legacy-through-time-rag/0.1 (https://github.com/monforje/legacy-through-time)"
    with httpx.Client(timeout=30, headers={"User-Agent": ua}) as http:
        for art in spec["articles"]:
            r = http.get(
                WIKI_API,
                params={
                    "action": "query",
                    "prop": "extracts|info",
                    "inprop": "url",
                    "explaintext": 1,
                    "exsectionformat": "wiki",
                    "redirects": 1,
                    "format": "json",
                    "titles": art["title"],
                },
            )
            r.raise_for_status()
            page = next(iter(r.json()["query"]["pages"].values()))
            if "extract" not in page:
                print(f"  ! not found: {art['title']}")
                continue
            body = _wiki_to_md(page["extract"], skip)
            front = {
                "source_id": art["id"],
                "title": page["title"],
                "layer": art.get("layer", "fact"),
                "rights": "cc",
                "cite": f"Википедия, «{page['title']}» (CC BY-SA 4.0), rev {page.get('lastrevid')}",
                "url": page.get("fullurl", ""),
            }
            if "event_year" in art:
                front["event_year"] = art["event_year"]
            text = "---\n" + yaml.safe_dump(front, allow_unicode=True, sort_keys=False) + "---\n\n" + body
            path = out_dir / f"{art['id']}.md"
            old = path.read_text(encoding="utf-8") if path.exists() else None
            if old is not None and _meta_comments(old) and not _meta_comments(text):
                print(f"  = kept (has hand-written meta comments): {path.name}")
                continue
            path.write_text(text, encoding="utf-8")
            print(f"  + {path.name}: {len(body)} chars")


def _wiki_to_md(extract: str, skip: set[str]) -> str:
    out: list[str] = []
    skipping_level = None
    for line in extract.splitlines():
        m = re.match(r"^(=+)\s*(.+?)\s*\1$", line)
        if m:
            level = len(m.group(1))
            if skipping_level is not None and level > skipping_level:
                continue
            skipping_level = level if m.group(2) in skip else None
            if skipping_level is None:
                out.append(f"\n{'#' * level} {m.group(2)}\n")
            continue
        if skipping_level is None:
            out.append(line)
    return re.sub(r"\n{3,}", "\n\n", "\n".join(out)).strip() + "\n"


def _meta_comments(text: str) -> bool:
    return "<!-- meta:" in text


async def ingest(settings) -> None:
    chunks = load_corpus(settings.corpus_dir)
    llm = LLM(settings)
    try:
        index = await Index.build(chunks, llm, settings.data_dir)
    finally:
        await llm.close()
    m = index.meta
    print(f"{m['chunks']} chunks, {m['embedded_now']} embedded now, dim {m['dim']}, model {m['embed_model']}")


async def search(settings, args) -> None:
    index = Index.load(settings.data_dir)
    hero = hero_of(settings, args.story)
    llm = LLM(settings)
    try:
        vec = (await llm.embed([args.query]))[0]
    finally:
        await llm.close()
    hits = index.search(
        vec,
        args.query,
        k=args.k,
        max_year=args.year or hero.year,
        layers=None if args.all_layers else HERO_LAYERS,
    )
    for h in hits:
        c = h.chunk
        print(
            f"{h.score:.4f} d={h.dense_rank} s={h.sparse_rank} [{c.layer}] {c.event_year} {c.id}  {c.heading}"
        )
        print("   " + c.text[:220].replace("\n", " ") + "…")


async def ask(settings, line: str, story: str) -> None:
    index = Index.load(settings.data_dir)
    hero = hero_of(settings, story)
    llm = LLM(settings)
    history = [{"role": "hero", "content": hero.opening[0]}, {"role": "reader", "content": line}]
    try:
        q = hero.query(history)
        hits = index.search((await llm.embed([q]))[0], q, max_year=hero.year, layers=HERO_LAYERS)
        for h in hits:
            print(f"  · [{h.chunk.layer}] {h.chunk.id} {h.chunk.heading}")
        result = ChatResult()
        text = ""
        async for delta in llm.stream_chat(hero.messages(history, {}, hits), result, **reply_options()):
            text += delta
            print(delta, end="", flush=True)
        print(
            f"\n  usage: {result.usage.get('prompt_tokens')}→{result.usage.get('completion_tokens')} "
            f"cost {result.usage.get('cost')}  flags: {hero.check_reply(text.strip())}"
        )
    finally:
        await llm.close()


if __name__ == "__main__":
    main()
