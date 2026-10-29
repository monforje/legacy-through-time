"""Corpus documents and chunking.

A document is a Markdown file in chat/corpus/ with YAML front matter:

    ---
    source_id: wiki-suyumbike
    title: Сююмбике
    layer: fact               # canon | fact | setting | fiction | legend | interp
    rights: cc                # pd | cc | summary | licensed
    cite: "Википедия, «Сююмбике»"
    url: https://...
    event_year: 1533          # optional default for the whole document
    ---

A section (from one heading to the next) can override these with a comment line
right under its heading:

    <!-- meta: layer=legend event_year=1552 -->

`event_year` of a chunk is the explicit override if there is one; otherwise the
latest year of 1400–2099 written in the chunk; otherwise the year carried over
from the previous chunk of the same section (the text reads in time order);
otherwise the document default. Chunks with no year at all pass any year filter.

A chunk is cut where the text moves on to a later year, so that "married in 1532"
and "reported in 1535" do not share a chunk and the earlier fact stays visible
to a heroine who lives in 1533.
"""

import hashlib
import re
from dataclasses import asdict, dataclass
from pathlib import Path

import yaml

LAYERS = ("canon", "fact", "setting", "fiction", "legend", "interp")
CHUNK_CHARS = 900
MIN_CHARS = 200

_HEADING = re.compile(r"^(#{1,6})\s+(.+?)\s*$")
_META = re.compile(r"^<!--\s*meta:(.*?)-->\s*$")
_YEAR = re.compile(r"(?<!\d)(1[4-9]\d\d|20\d\d)(?!\d)")


@dataclass
class Chunk:
    id: str
    source_id: str
    title: str
    heading: str
    text: str
    layer: str
    rights: str
    cite: str
    url: str
    event_year: int | None

    def embed_text(self) -> str:
        """What goes to the embedding model: the place in the document, then the text."""
        where = f"{self.title} › {self.heading}" if self.heading else self.title
        return f"{where}\n\n{self.text}"

    def to_dict(self) -> dict:
        return asdict(self)


def load_corpus(corpus_dir: Path) -> list[Chunk]:
    chunks: list[Chunk] = []
    for path in sorted(corpus_dir.rglob("*.md")):
        if path.name.lower() == "readme.md":
            continue
        chunks.extend(chunk_document(path))
    ids = [c.id for c in chunks]
    if len(ids) != len(set(ids)):
        raise ValueError("duplicate chunk ids: two documents share a source_id")
    return chunks


def chunk_document(path: Path) -> list[Chunk]:
    raw = path.read_text(encoding="utf-8")
    meta, body = _split_front_matter(raw, path)
    for key in ("source_id", "title", "layer", "rights", "cite"):
        if not meta.get(key):
            raise ValueError(f"{path}: front matter needs `{key}`")
    _check_layer(meta["layer"], path)

    chunks: list[Chunk] = []
    for heading, overrides, paragraphs in _sections(body):
        layer = overrides.get("layer", meta["layer"])
        _check_layer(layer, path)
        fixed_year = _int_or_none(overrides.get("event_year"))
        carried = _int_or_none(meta.get("event_year"))
        for text in _pack(paragraphs):
            if fixed_year is not None:
                year = fixed_year
            else:
                years = _years(text)
                year = max(years) if years else carried
                carried = year
            n = len(chunks)
            chunks.append(
                Chunk(
                    id=f"{meta['source_id']}#{n:03d}",
                    source_id=meta["source_id"],
                    title=meta["title"],
                    heading=heading,
                    text=text,
                    layer=layer,
                    rights=meta["rights"],
                    cite=meta["cite"],
                    url=meta.get("url", ""),
                    event_year=year,
                )
            )
    return chunks


def corpus_hash(chunks: list[Chunk]) -> str:
    h = hashlib.sha256()
    for c in chunks:
        h.update(repr(sorted(c.to_dict().items())).encode())
    return h.hexdigest()[:16]


def _split_front_matter(raw: str, path: Path) -> tuple[dict, str]:
    if not raw.startswith("---\n"):
        raise ValueError(f"{path}: no front matter")
    end = raw.find("\n---\n", 4)
    if end < 0:
        raise ValueError(f"{path}: front matter is not closed")
    return yaml.safe_load(raw[4:end]) or {}, raw[end + 5 :]


def _sections(body: str):
    """Yields (heading path, overrides, paragraphs) for every section with text."""
    stack: list[tuple[int, str]] = []  # (heading level, title), outermost first
    overrides: dict = {}
    paragraphs: list[str] = []
    buf: list[str] = []

    def flush_paragraph():
        text = " ".join(s.strip() for s in buf).strip()
        if text:
            paragraphs.append(text)
        buf.clear()

    for line in body.splitlines():
        if m := _HEADING.match(line):
            flush_paragraph()
            if paragraphs:
                yield " › ".join(t for _, t in stack), overrides, paragraphs
            level = len(m.group(1))
            while stack and stack[-1][0] >= level:
                stack.pop()
            stack.append((level, m.group(2)))
            overrides, paragraphs = {}, []
        elif m := _META.match(line.strip()):
            overrides = dict(kv.split("=", 1) for kv in m.group(1).split())
        elif not line.strip():
            flush_paragraph()
        else:
            buf.append(line)
    flush_paragraph()
    if paragraphs:
        yield " › ".join(t for _, t in stack), overrides, paragraphs


def _pack(paragraphs: list[str]) -> list[str]:
    """Packs whole paragraphs into chunks of about CHUNK_CHARS; splits long ones by sentence.
    Never joins text across a move to a later year (see the module docstring)."""
    pieces: list[str] = []
    for p in paragraphs:
        if len(p) <= CHUNK_CHARS and not _moves_on(p):
            pieces.append(p)
            continue
        sentence_buf = ""
        for s in re.split(r"(?<=[.!?…])\s+", p):
            too_long = len(sentence_buf) + len(s) + 1 > CHUNK_CHARS
            if sentence_buf and (too_long or _later(s, sentence_buf)):
                pieces.append(sentence_buf)
                sentence_buf = s
            else:
                sentence_buf = f"{sentence_buf} {s}".strip()
        if sentence_buf:
            pieces.append(sentence_buf)

    chunks: list[str] = []
    for piece in pieces:
        fits = chunks and len(chunks[-1]) + len(piece) + 2 <= CHUNK_CHARS
        if fits and not _later(piece, chunks[-1]):
            chunks[-1] = f"{chunks[-1]}\n\n{piece}"
        else:
            chunks.append(piece)
    # A short tail joins the chunk before it rather than standing alone, if no year is crossed.
    if len(chunks) > 1 and len(chunks[-1]) < MIN_CHARS and not _later(chunks[-1], chunks[-2]):
        tail = chunks.pop()
        chunks[-1] = f"{chunks[-1]}\n\n{tail}"
    return chunks


def _years(text: str) -> list[int]:
    return [int(y) for y in _YEAR.findall(text)]


def _later(new: str, before: str) -> bool:
    """True when `new` names a year later than every year in `before` (which has one)."""
    new_years, old_years = _years(new), _years(before)
    return bool(new_years and old_years and max(new_years) > max(old_years))


def _moves_on(paragraph: str) -> bool:
    """A paragraph that names more than one distinct year is split by sentence."""
    return len(set(_years(paragraph))) > 1


def _check_layer(layer: str, path: Path) -> None:
    if layer not in LAYERS:
        raise ValueError(f"{path}: unknown layer `{layer}`, expected one of {LAYERS}")


def _int_or_none(value) -> int | None:
    return None if value in (None, "", "none") else int(value)
