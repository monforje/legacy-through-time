"""Статическая проверка макетов оболочки: python3 -I check.py

Цвета только из токенов kit.css, палитра без зелёно-красной, контраст AA,
кнопки с подписью, ссылки живые. Код выхода 0 при чистом прогоне, иначе 1."""
import colorsys
import re
import sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
CSS = HERE / "kit.css"
PAGES = sorted(HERE.glob("*.html"))
HEX = re.compile(r"#[0-9a-fA-F]{3,8}\b")
FUNC = re.compile(r"\b(?:rgba?|hsla?)\(")
EXEMPT = ("--ok", "--danger", "--art-")  # смысловые и цвета иллюстраций


def root_block(css: str) -> str:
    m = re.search(r":root\s*\{(.*?)\n\}", css, re.S)
    return m.group(1) if m else ""


def tokens(css: str) -> dict[str, str]:
    return {k: v.strip() for k, v in re.findall(r"(--[\w-]+)\s*:\s*([^;]+);", root_block(css))}


def rgb(value: str):
    m = HEX.fullmatch(value.strip())
    if not m:
        return None
    h = value.strip().lstrip("#")
    if len(h) in (3, 4):
        h = "".join(c * 2 for c in h)
    return tuple(int(h[i:i + 2], 16) / 255 for i in (0, 2, 4))


def luminance(c) -> float:
    lin = [x / 12.92 if x <= 0.03928 else ((x + 0.055) / 1.055) ** 2.4 for x in c]
    return 0.2126 * lin[0] + 0.7152 * lin[1] + 0.0722 * lin[2]


def ratio(a, b) -> float:
    la, lb = sorted((luminance(a), luminance(b)), reverse=True)
    return (la + 0.05) / (lb + 0.05)


def colors_outside_tokens(css_text: str, pages: dict[str, str]) -> list[str]:
    errors = []
    outside = css_text.replace(root_block(css_text), "")
    for n, line in enumerate(outside.splitlines(), 1):
        if HEX.search(line) or FUNC.search(line):
            errors.append(f"kit.css:{n}: цвет вне токенов: {line.strip()}")
    for name, text in pages.items():
        for n, line in enumerate(text.splitlines(), 1):
            if HEX.search(line) or FUNC.search(line):
                errors.append(f"{name}:{n}: цвет в разметке: {line.strip()[:80]}")
    return errors


def forbidden_hues(toks: dict[str, str]) -> list[str]:
    errors = []
    for name, value in toks.items():
        c = rgb(value)
        if c is None or name.startswith(EXEMPT):
            continue
        h, s, _ = colorsys.rgb_to_hsv(*c)
        deg = h * 360
        if s > 0.35 and (100 <= deg <= 170 or deg >= 340 or deg <= 15):
            errors.append(f"{name}: {value} зелёный или красный (оттенок {deg:.0f}°)")
    return errors


PAIRS = [("--text", "--bg"), ("--text", "--surface"), ("--text", "--surface-2"),
         ("--accent-ink", "--accent"), ("--text-muted", "--bg"), ("--text-muted", "--surface")]
NON_TEXT = [("--line", "--surface"), ("--line", "--bg")]  # WCAG 1.4.11: границы и треки не ниже 3:1


def contrast(toks: dict[str, str]) -> list[str]:
    errors = []
    for fg, bg in PAIRS:
        a, b = rgb(toks.get(fg, "")), rgb(toks.get(bg, ""))
        if a is None or b is None:
            errors.append(f"{fg}/{bg}: нет токена")
        elif ratio(a, b) < 4.5:
            errors.append(f"{fg}/{bg}: контраст {ratio(a, b):.2f} < 4.5")
    for fg, bg in NON_TEXT:
        a, b = rgb(toks.get(fg, "")), rgb(toks.get(bg, ""))
        if a is not None and b is not None and ratio(a, b) < 3:
            errors.append(f"{fg}/{bg}: контраст границы {ratio(a, b):.2f} < 3")
    return errors


def unlabeled_buttons(pages: dict[str, str]) -> list[str]:
    errors = []
    for name, text in pages.items():
        for m in re.finditer(r"<button\b([^>]*)>(.*?)</button>", text, re.S):
            inner = re.sub(r"<[^>]+>", "", m.group(2)).strip()
            if not inner and "aria-label" not in m.group(1):
                errors.append(f"{name}: кнопка без подписи: <button{m.group(1)[:50]}>")
    return errors


def dead_links(pages: dict[str, str]) -> list[str]:
    errors = []
    for name, text in pages.items():
        for href in re.findall(r'(?:href|src)="([^"#:?]+)"', text):
            if not (HERE / href).exists():
                errors.append(f"{name}: нет файла {href}")
    return errors


def main() -> int:
    if not CSS.exists():
        print("нет kit.css")
        return 1
    css_text = CSS.read_text(encoding="utf-8")
    pages = {p.name: p.read_text(encoding="utf-8") for p in PAGES}
    toks = tokens(css_text)
    errors = (colors_outside_tokens(css_text, pages) + forbidden_hues(toks) + contrast(toks)
              + unlabeled_buttons(pages) + dead_links(pages))
    print("\n".join(errors) if errors else "ok")
    return 1 if errors else 0


if __name__ == "__main__":
    sys.exit(main())
