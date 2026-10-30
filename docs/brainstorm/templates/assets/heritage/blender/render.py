"""Renders the Blender kit of the Heritage UI: the sprites of the SVG kit (names, roles) drawn as crafted things -
tooled leather in gold rims, parchment with stitches and embroidery, cut stones, filigree - under a warm lamp.

    blender -b --factory-startup --python-exit-code 1 -P blender/render.py -- [name ...] [--list]

(`task assets:blender`). Writes png3d/<name>.png at @4x and png3d/slices.txt for the Unity importer. Without names it
renders every sprite of blender/sprites3d.py; the slices file always lists all of them.

Sizes and borders come from the SVG kit's registry unless the sprite overrides them. Every 9-slice sprite is tiled:
the game's KitImage repeats a whole number of its edge and centre tiles (border-image: round), so the patterns and
textures, periodic with the tile, never smear or break."""
import sys
import tempfile
import time
from pathlib import Path

HERE = Path(__file__).resolve().parent
ROOT = HERE.parent
sys.path[:0] = [str(ROOT), str(HERE)]

import sprites  # noqa: E402,F401  (registers the SVG kit: its sizes and borders)
from kit.registry import REGISTRY  # noqa: E402

import sprites3d  # noqa: E402
import chat3d  # noqa: E402,F401  (the heroine chat's sprites, registered into sprites3d.KIT)
import sikhote3d  # noqa: E402,F401  (the story kit of «По хребтам Сихотэ-Алиня»: sa-*, registered into sprites3d.KIT)
from studio import SCALE, Studio, finish  # noqa: E402

OUT = ROOT / 'png3d'


def args():
    argv = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
    return [a for a in argv if not a.startswith('--')], {a for a in argv if a.startswith('--')}


def slices_txt(specs):
    """`name left bottom right top sliced|tiled fill_center` in px of the PNG, as the SVG kit's slices.txt."""
    rows = []
    for spec in sorted(specs, key=lambda s: s.name + '.png'):
        b = spec.slice
        if b:
            border = ' '.join(str(v * SCALE) for v in (b.left, b.bottom, b.right, b.top))
            rows.append(f'{spec.name} {border} {"tiled" if spec.tiled else "sliced"} {int(b.fill)}')
    return '\n'.join(rows) + '\n'


def main():
    names, flags = args()
    specs = sprites3d.specs(REGISTRY)
    if '--list' in flags:
        for spec in specs.values():
            print(f'{spec.name:16} {spec.width}x{spec.height} {"tiled" if spec.tiled else "sliced" if spec.slice else "-"}')
        return
    unknown = [n for n in names if n not in specs]
    if unknown:
        raise SystemExit(f'not in the Blender kit: {", ".join(unknown)}')
    OUT.mkdir(exist_ok=True)
    raw = Path(tempfile.mkdtemp(prefix='heritage3d-'))
    for name in names or list(specs):
        spec = specs[name]
        t = time.time()
        studio = Studio(spec.width, spec.height, spec.shear, spec.period, spec.samples)
        studio.borders = spec.slice
        spec.build(studio, spec.width, spec.height)
        studio.render(raw / f'{name}.png')
        b = spec.slice
        finish(raw / f'{name}.png', OUT / f'{name}.png',
               slice_px=b and tuple(v * SCALE for v in (b.left, b.bottom, b.right, b.top)),
               sliced=bool(b) and not spec.tiled, mask=spec.mask, halo=spec.halo)
        print(f'[3d] {name:16} {time.time() - t:5.1f} s', flush=True)
    (OUT / 'slices.txt').write_text(slices_txt(specs.values()), encoding='utf-8')
    print(f'[3d] ok: {len(names or specs)} png -> {OUT}')


main()
