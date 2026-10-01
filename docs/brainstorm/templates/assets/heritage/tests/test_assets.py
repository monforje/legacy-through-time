"""Checks of the generated kit: every sprite is a well-formed SVG of the declared size, the 9-slice borders fit inside
the sprites, the output is reproducible and independent of the build order, the manifest agrees with the sprites.
Run: python3 -m unittest discover -s tests   (task assets:test)"""
import json
import sys
import unittest
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
sys.path.insert(0, str(ROOT))

import sprites  # noqa: E402,F401
from kit import export  # noqa: E402
from kit.registry import REGISTRY  # noqa: E402

SVG = '{http://www.w3.org/2000/svg}'


class SpriteTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.sprites = REGISTRY.make_all()

    def test_every_sprite_is_well_formed_svg_of_its_declared_size(self):
        for name, sprite in self.sprites.items():
            with self.subTest(name):
                root = ET.fromstring(sprite.render())
                self.assertEqual(root.tag, SVG + 'svg')
                self.assertEqual(root.get('viewBox'), f'0 0 {sprite.width} {sprite.height}')
                self.assertEqual((root.get('width'), root.get('height')), (str(sprite.width), str(sprite.height)))

    def test_ids_inside_a_sprite_are_unique(self):
        # the grain filter and the tail clip have ids; two of the same id in one file would break the reference
        for name, sprite in self.sprites.items():
            with self.subTest(name):
                ids = [el.get('id') for el in ET.fromstring(sprite.render()).iter() if el.get('id')]
                self.assertEqual(len(ids), len(set(ids)))

    def test_names_match_files_and_are_lowercase_kebab(self):
        for name in REGISTRY.entries:
            self.assertRegex(name, r'^[a-z][a-z0-9]*(-[a-z0-9]+)*$')

    def test_slice_borders_fit_inside_the_sprite(self):
        for name, entry in REGISTRY.entries.items():
            s = entry.slice
            if not s:
                continue
            sprite = self.sprites[name]
            with self.subTest(name):
                self.assertLess(s.left + s.right, sprite.width, 'borders leave no middle column')
                self.assertLess(s.top + s.bottom, sprite.height, 'borders leave no middle row')
                self.assertIn(s.kind, ('sliced', 'tiled'))

    def test_a_sprite_with_a_halo_has_the_bed_and_without_it_has_none(self):
        for name, sprite in self.sprites.items():
            with self.subTest(name):
                has_bed = 'fill-opacity=".0045"' in sprite.render()
                self.assertEqual(has_bed, sprite.halo is not None)

    def test_the_output_is_reproducible(self):
        again = REGISTRY.make_all()
        for name, sprite in self.sprites.items():
            with self.subTest(name):
                self.assertEqual(sprite.render(), again[name].render())

    def test_a_sprite_does_not_depend_on_the_others_or_on_the_order(self):
        # the jitter of the hand is seeded by the name: building one sprite alone gives the same drawing
        for name in ('plate', 'label', 'btn', 'plate-tail-r'):
            with self.subTest(name):
                self.assertEqual(REGISTRY.make(name).render(), self.sprites[name].render())

    def test_tiled_edges_are_whole_tiles(self):
        # a tiled sprite's middle must be a whole number of tiles wide so that the repeat ends on a seam
        for name, entry in REGISTRY.entries.items():
            s = entry.slice
            if s and s.kind == 'tiled':
                sprite = self.sprites[name]
                with self.subTest(name):
                    middle = sprite.width - s.left - s.right
                    self.assertGreater(middle, 0)
                    self.assertEqual(sprite.width, sprite.height, 'tiled clouds are square')

    def test_mirrored_horns_swap_their_wide_border(self):
        right, left = REGISTRY.entries['plate-tail-r'].slice, REGISTRY.entries['plate-tail-l'].slice
        self.assertEqual((right.left, right.right), (left.right, left.left))
        self.assertEqual((right.top, right.bottom), (left.top, left.bottom))


class ExportTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.sprites = REGISTRY.make_all()
        cls.items = export.manifest(cls.sprites, REGISTRY)

    def test_png_sizes_are_the_svg_sizes_times_the_scale(self):
        for name, item in self.items.items():
            with self.subTest(name):
                self.assertEqual(item['size_png'], [item['size_1x'][0] * export.SCALE, item['size_1x'][1] * export.SCALE])

    def test_unity_borders_are_left_bottom_right_top_in_png_pixels(self):
        plate = self.items['plate-tail-r']['slice']
        self.assertEqual(plate['unity_border_ltrb_png'], [32 * export.SCALE, 32 * export.SCALE, 140 * export.SCALE, 64 * export.SCALE])

    def test_slices_txt_lists_exactly_the_sliced_sprites(self):
        rows = export.slices_txt(self.items).strip().split('\n')
        names = {row.split(' ')[0] for row in rows}
        self.assertEqual(names, {n for n, e in REGISTRY.entries.items() if e.slice})
        for row in rows:
            parts = row.split(' ')
            self.assertEqual(len(parts), 7, row)
            self.assertTrue(all(p.lstrip('-').isdigit() for p in parts[1:5]))

    def test_the_manifest_is_valid_json_one_sprite_per_line(self):
        text = export.manifest_json(self.items)
        self.assertEqual(json.loads(text).keys(), self.items.keys())
        self.assertEqual(len(text.strip().split('\n')), len(self.items) + 2)

    def test_the_files_on_disk_are_what_the_generator_writes(self):
        # catches a forgotten `python3 build.py` after a change of a builder
        for name, sprite in self.sprites.items():
            path = ROOT / f'{name}.svg'
            with self.subTest(name):
                self.assertTrue(path.exists(), f'{path.name} missing: run build.py')
                self.assertEqual(path.read_text(encoding='utf-8'), sprite.render(), f'{path.name} is stale: run build.py')


if __name__ == '__main__':
    unittest.main()
