"""Hybrid index: dense vectors (cosine) + BM25 over Russian stems, fused by RRF.

On disk (chat/data/index/):
    chunks.jsonl   one chunk per line, in row order
    vectors.npy    float32 [n, dim], unit length
    meta.json      embed model, dim, corpus hash, build time

Embeddings are cached in chat/data/embeddings.sqlite by (model, sha256 of text),
so a rebuild only pays for new or changed chunks.
"""

import hashlib
import json
import math
import re
import sqlite3
import time
from collections import Counter
from dataclasses import dataclass
from pathlib import Path

import numpy as np
import snowballstemmer

from .corpus import Chunk, corpus_hash
from .llm import LLM

RRF_K = 60
CANDIDATES = 40

_WORD = re.compile(r"[а-яёa-z0-9]+")
_ru = snowballstemmer.stemmer("russian")
_en = snowballstemmer.stemmer("english")


def tokenize(text: str) -> list[str]:
    words = _WORD.findall(text.lower().replace("ё", "е"))
    return [(_ru if re.search("[а-я]", w) else _en).stemWord(w) for w in words if len(w) > 1]


class BM25:
    def __init__(self, docs: list[list[str]], k1: float = 1.4, b: float = 0.75):
        self.k1, self.b = k1, b
        self.tf = [Counter(d) for d in docs]
        self.len = np.array([len(d) for d in docs], dtype=np.float32)
        self.avg = float(self.len.mean()) if len(docs) else 0.0
        df = Counter(t for d in self.tf for t in d)
        n = len(docs)
        self.idf = {t: math.log(1 + (n - f + 0.5) / (f + 0.5)) for t, f in df.items()}

    def scores(self, query: list[str]) -> np.ndarray:
        out = np.zeros(len(self.tf), dtype=np.float32)
        for term in set(query):
            idf = self.idf.get(term)
            if idf is None:
                continue
            for i, tf in enumerate(self.tf):
                f = tf.get(term)
                if f:
                    norm = self.k1 * (1 - self.b + self.b * self.len[i] / self.avg)
                    out[i] += idf * f * (self.k1 + 1) / (f + norm)
        return out


@dataclass
class Hit:
    chunk: Chunk
    score: float
    dense_rank: int | None
    sparse_rank: int | None

    def to_dict(self) -> dict:
        return {
            **self.chunk.to_dict(),
            "score": round(self.score, 5),
            "dense_rank": self.dense_rank,
            "sparse_rank": self.sparse_rank,
        }


class Index:
    def __init__(self, chunks: list[Chunk], vectors: np.ndarray, meta: dict):
        self.chunks = chunks
        self.vectors = vectors
        self.meta = meta
        self.bm25 = BM25([tokenize(c.embed_text()) for c in chunks])

    # ---- build / load ----

    @staticmethod
    async def build(chunks: list[Chunk], llm: LLM, data_dir: Path) -> "Index":
        model = llm.settings.embed_model
        texts = [c.embed_text() for c in chunks]
        cache = _EmbedCache(data_dir / "embeddings.sqlite")
        keys = [_key(model, t) for t in texts]
        found = cache.get_many(keys)
        missing = [i for i, k in enumerate(keys) if k not in found]
        if missing:
            fresh = await llm.embed([texts[i] for i in missing])
            cache.put_many({keys[i]: fresh[j] for j, i in enumerate(missing)})
            found.update({keys[i]: fresh[j] for j, i in enumerate(missing)})
        cache.close()

        vectors = (
            np.stack([found[k] for k in keys]).astype(np.float32)
            if keys
            else np.zeros((0, 0), dtype=np.float32)
        )
        meta = {
            "embed_model": model,
            "dim": int(vectors.shape[1]) if len(keys) else 0,
            "chunks": len(chunks),
            "embedded_now": len(missing),
            "corpus_hash": corpus_hash(chunks),
            "built_at": time.strftime("%Y-%m-%d %H:%M:%S"),
        }
        out = data_dir / "index"
        out.mkdir(parents=True, exist_ok=True)
        with (out / "chunks.jsonl").open("w", encoding="utf-8") as f:
            for c in chunks:
                f.write(json.dumps(c.to_dict(), ensure_ascii=False) + "\n")
        np.save(out / "vectors.npy", vectors)
        (out / "meta.json").write_text(json.dumps(meta, ensure_ascii=False, indent=2))
        return Index(chunks, vectors, meta)

    @staticmethod
    def load(data_dir: Path) -> "Index":
        src = data_dir / "index"
        if not (src / "meta.json").exists():
            raise SystemExit("no index yet: run `task chat:ingest` (or `uv run python -m rag ingest`)")
        with (src / "chunks.jsonl").open(encoding="utf-8") as f:
            chunks = [Chunk(**json.loads(line)) for line in f]
        vectors = np.load(src / "vectors.npy")
        meta = json.loads((src / "meta.json").read_text())
        return Index(chunks, vectors, meta)

    # ---- search ----

    def search(
        self,
        query_vec: np.ndarray,
        query_text: str,
        *,
        k: int = 6,
        max_year: int | None = None,
        layers: tuple[str, ...] | None = None,
        story: str | None = None,
        per_source: int = 3,
    ) -> list[Hit]:
        """Top k chunks that pass the filters. `max_year` drops chunks about later events,
        `story` those written for the hero of another story (a chunk without a story is for all)."""
        if not self.chunks:
            return []
        allowed = np.array(
            [
                (layers is None or c.layer in layers)
                and (story is None or not c.story or c.story == story)
                and (max_year is None or c.event_year is None or c.event_year <= max_year)
                for c in self.chunks
            ]
        )
        if not allowed.any():
            return []

        dense = self.vectors @ query_vec.astype(np.float32)
        sparse = self.bm25.scores(tokenize(query_text))
        dense_ranks = _ranks(dense, allowed)
        sparse_ranks = _ranks(sparse, allowed & (sparse > 0))

        fused: dict[int, float] = {}
        for ranks in (dense_ranks, sparse_ranks):
            for i, r in ranks.items():
                fused[i] = fused.get(i, 0.0) + 1.0 / (RRF_K + r)

        hits: list[Hit] = []
        taken: Counter = Counter()
        for i, score in sorted(fused.items(), key=lambda kv: -kv[1]):
            c = self.chunks[i]
            if taken[c.source_id] >= per_source:
                continue
            taken[c.source_id] += 1
            hits.append(Hit(c, score, dense_ranks.get(i), sparse_ranks.get(i)))
            if len(hits) == k:
                break
        return hits


def _ranks(scores: np.ndarray, mask: np.ndarray) -> dict[int, int]:
    """Row -> 1-based rank among the CANDIDATES best rows that pass the mask."""
    idx = np.flatnonzero(mask)
    best = idx[np.argsort(-scores[idx], kind="stable")][:CANDIDATES]
    return {int(i): r + 1 for r, i in enumerate(best)}


def _key(model: str, text: str) -> str:
    return hashlib.sha256(f"{model}\n{text}".encode()).hexdigest()


class _EmbedCache:
    def __init__(self, path: Path):
        path.parent.mkdir(parents=True, exist_ok=True)
        self.db = sqlite3.connect(path)
        self.db.execute("CREATE TABLE IF NOT EXISTS emb (key TEXT PRIMARY KEY, vec BLOB NOT NULL)")

    def get_many(self, keys: list[str]) -> dict[str, np.ndarray]:
        out: dict[str, np.ndarray] = {}
        for i in range(0, len(keys), 500):
            part = keys[i : i + 500]
            q = f"SELECT key, vec FROM emb WHERE key IN ({','.join('?' * len(part))})"
            for key, blob in self.db.execute(q, part):
                out[key] = np.frombuffer(blob, dtype=np.float32)
        return out

    def put_many(self, items: dict[str, np.ndarray]) -> None:
        self.db.executemany(
            "INSERT OR REPLACE INTO emb VALUES (?, ?)",
            [(k, v.astype(np.float32).tobytes()) for k, v in items.items()],
        )
        self.db.commit()

    def close(self) -> None:
        self.db.close()
