"""Writes the sprites (SVG), the manifest of sizes and borders, and the flat list for the Unity importer."""
import json
import re
from pathlib import Path

# PNG scale of `task assets:png`: 4x leaves room for mip-maps when the screen is smaller than the texture.
SCALE = 4


def write_svgs(sprites, out: Path):
    out.mkdir(parents=True, exist_ok=True)
    for name, sprite in sprites.items():
        (out / f'{name}.svg').write_text(sprite.render(), encoding='utf-8')
    # a sprite that is no longer registered must not linger as a stale file
    for stale in out.glob('*.svg'):
        if stale.stem not in sprites:
            stale.unlink()


def manifest(sprites, registry):
    """name -> sizes and (when the sprite is 9-sliced) borders, in px of the SVG and of the PNG."""
    out = {}
    for name, sprite in sorted(sprites.items(), key=lambda kv: kv[0] + '.svg'):   # file-name order: a stable order for diffs
        item = {'size_1x': [sprite.width, sprite.height], 'size_png': [sprite.width * SCALE, sprite.height * SCALE]}
        s = registry.entries[name].slice
        if s:
            item['slice'] = {
                'top_bottom_left_right_1x': [s.top, s.bottom, s.left, s.right],
                # Unity wants Sprite.border as (left, bottom, right, top) in px of the texture
                'unity_border_ltrb_png': [s.left * SCALE, s.bottom * SCALE, s.right * SCALE, s.top * SCALE],
                'type': s.kind, 'fill_center': s.fill,
            }
        out[name] = item
    return out


def slices_txt(items):
    """One line per 9-sliced sprite, `name left bottom right top sliced|tiled fill_center`: the Unity importer
    reads it without a JSON parser."""
    rows = [f'{k} {" ".join(map(str, v["slice"]["unity_border_ltrb_png"]))} {v["slice"]["type"]} {int(v["slice"]["fill_center"])}'
            for k, v in items.items() if 'slice' in v]
    return '\n'.join(rows) + '\n'


def manifest_json(items):
    """One sprite per line: the manifest reads and diffs line by line."""
    lines = ',\n'.join(f' {json.dumps(k)}: {json.dumps(v, ensure_ascii=False)}' for k, v in items.items())
    return '{\n' + lines + '\n}\n'


def write_all(registry, out: Path):
    sprites = registry.make_all()
    write_svgs(sprites, out)
    items = manifest(sprites, registry)
    (out / 'slices.txt').write_text(slices_txt(items), encoding='utf-8')
    (out / 'manifest.json').write_text(manifest_json(items), encoding='utf-8')
    return sprites


def viewbox_of(svg_text):
    m = re.search(r'viewBox="0 0 (\d+) (\d+)"', svg_text)
    return int(m[1]), int(m[2])
