#!/usr/bin/env python3
"""Draws the sprites of the "Наследие. Сквозь время" UI kit as SVG (references: assets/ai_slop) and writes, next to
them, manifest.json (sizes, 9-slice borders) and slices.txt (the flat list for the Unity importer).

    python3 build.py           # write everything here
    python3 build.py --list    # print the registered sprites and their borders

Layout:  kit/      palette, geometry, the SVG document, the registry, the export
         sprites/  one module per group of sprites; importing it registers them (borders are registered with the sprite)
Tests:   python3 -m unittest discover -s tests   (task assets:test)

Plain Python 3, no dependencies. The hand-drawn jitter is seeded by the name of the sprite, so the output is
the same on every run and a sprite does not change when another one does.
"""
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent))

import sprites  # noqa: E402,F401  (registers every sprite)
from kit.export import write_all  # noqa: E402
from kit.registry import REGISTRY  # noqa: E402

OUT = Path(__file__).parent


def main(argv):
    if '--list' in argv:
        for name, entry in REGISTRY.entries.items():
            print(f'{name:16} {entry.slice or "-"}')
        return 0
    written = write_all(REGISTRY, OUT)
    print('ok:', len(written), 'svg')
    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
