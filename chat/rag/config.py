"""Settings from chat/.env (or the process environment, which wins)."""

import os
from dataclasses import dataclass
from pathlib import Path

CHAT_DIR = Path(__file__).resolve().parent.parent
REPO_DIR = CHAT_DIR.parent


def _load_env(path: Path) -> None:
    if not path.exists():
        return
    for line in path.read_text(encoding="utf-8").splitlines():
        line = line.strip()
        if not line or line.startswith("#") or "=" not in line:
            continue
        key, value = line.split("=", 1)
        os.environ.setdefault(key.strip(), value.strip())


@dataclass(frozen=True)
class Settings:
    base_url: str
    api_key: str
    proxy: str | None
    chat_model: str
    embed_model: str
    corpus_dir: Path
    heroine_dir: Path
    data_dir: Path
    art_dirs: tuple[Path, ...]
    fonts_dir: Path
    kit_dir: Path


def load() -> Settings:
    _load_env(CHAT_DIR / ".env")
    if os.environ.get("AI_ENABLED", "true").lower() != "true":
        raise SystemExit("AI_ENABLED is not true")
    key = os.environ.get("AI_API_KEY", "")
    if not key:
        raise SystemExit("AI_API_KEY is empty: put it in chat/.env")
    return Settings(
        base_url=os.environ.get("AI_BASE_URL", "https://openrouter.ai/api/v1").rstrip("/"),
        api_key=key,
        proxy=os.environ.get("AI_HTTP_PROXY") or None,
        chat_model=os.environ.get("AI_CHAT_MODEL", "z-ai/glm-5.3-flash"),
        embed_model=os.environ.get("AI_EMBED_MODEL", "baai/bge-m3"),
        corpus_dir=CHAT_DIR / "corpus",
        heroine_dir=REPO_DIR / "content/stories/golden-cage/chat",
        data_dir=CHAT_DIR / "data",
        art_dirs=(REPO_DIR / "docs/art/legacy-scenes", REPO_DIR / "docs/art"),
        fonts_dir=REPO_DIR / "Assets/Resources/Fonts",
        kit_dir=REPO_DIR / "docs/brainstorm/templates/assets/heritage/png3d",
    )
