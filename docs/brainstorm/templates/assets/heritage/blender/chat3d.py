"""The sprites of the heroine chat (the night talk with Sujumbike, chat/), in the Blender kit's materials and palette.
The chat reuses the kit where it can: the heroine speaks on `plate` with the `label` tag, the end of the night is a
`card`. New here is only what the kit has no piece for:

    chat-veil      the Voice's line: a veil of smoky-mint gauze you see the night through, a dashed rose stitch
                   (the dream, as the dashed outlines of the thought clouds), small gold stars in two corners
    chat-chip      a suggested line: ivory leather pill in a gilt rim (the cream state of the kit's buttons)
    chat-field     the input: parchment sunk in a gold frame, a row of granules along it
    chat-send      the send button: an emerald leather seal in a gilt rim, a gold arrow in relief
    chat-bead(-out) the lines left in the night: pearl beads on a thread, spent ones dark
    flag           a glyph mask (the game tints it): the pennant of "report this line"

Registered into sprites3d.KIT, rendered by render.py like every other sprite of the kit."""
import math

from kit.palette import INK, PAPER_L
from kit.registry import Slice
from shapes import circle, dashes, rrect, sparkle, stroke
from sprites3d import (LEATHER, LEATHER_IVORY, PEARL, edge, glyph, glyph_fill, gold, granules, leather, lattice,
                       parchment, plaque, runs, sprite, thread)

SMOKE_MINT = '#d9e1c8'      # DESIGN.md "Дымчатая мята"
ROSE_THREAD = '#c8706c'     # the rose of the kit (#f4a3a1) as a thread that still reads on mint


@sprite('chat-veil', size=(96, 96), slice=Slice.uniform(32), shear=.3, halo=INK)
def chat_veil(s, w, h):
    """96x96, tile 32. Text keeps 22 px from the edges; the stars end 26 px in."""
    box = (2, 2, 94, 92)
    outline = rrect(*box, 12)
    s.slab(outline, -.25, .25, s.mat('gauze', SMOKE_MINT, alpha=.48), bevel=.25)
    # a running stitch 5 px in: dashes 2.2 on, 1.8 off - a 4 px step that divides the 32 px tile, anchored at it
    x0, y0, x1, y1 = box
    paths = []
    for x in lattice(x0 + 14, x1 - 14, 32, 4):
        paths += [[(x - 1.1, y0 + 5), (x + 1.1, y0 + 5)], [(x - 1.1, y1 - 5), (x + 1.1, y1 - 5)]]
    for y in lattice(y0 + 14, y1 - 14, 32, 4):
        paths += [[(x0 + 5, y - 1.1), (x0 + 5, y + 1.1)], [(x1 - 5, y - 1.1), (x1 - 5, y + 1.1)]]
    # the corners: the same dashes along the arc of the rounded corner (radius 12 - 5)
    for cx, cy, a0 in ((x0 + 12, y0 + 12, math.pi), (x1 - 12, y0 + 12, 1.5 * math.pi),
                       (x1 - 12, y1 - 12, 0), (x0 + 12, y1 - 12, .5 * math.pi)):
        arc_pts = [(cx + 7 * math.cos(a0 + .5 * math.pi * k / 24), cy + 7 * math.sin(a0 + .5 * math.pi * k / 24))
                   for k in range(25)]
        paths += dashes(arc_pts, 2.2, 1.8)
    s.tubes(paths, .3, thread(s, ROSE_THREAD), z=.35)
    for cx, cy, r in ((14.5, 14.5, 4.6), (w - 14.5, h - 18.5, 3.6)):
        s.slab(sparkle(cx, cy, r, .22), .1, .9, gold(s), bevel=.3)


@sprite('chat-chip', size=(96, 40), slice=Slice(14, 20, 20, 16))
def chat_chip(s, w, h):
    """A suggested line: ivory leather pill, gilt rim, granules along the straight runs (tile 56 x 10)."""
    box = (1.5, 1.5, 94.5, 36.5)
    plaque(s, rrect(*box, 12.5), leather(s, LEATHER_IVORY), depth=2.4, bevel=1.4, rim=.8)
    granules(s, runs(s, box, 3.8, 2, 16, sides='tb'), r=.36, z=1.0)


@sprite('chat-field', size=(96, 48), slice=Slice(20, 24, 24, 22))
def chat_field(s, w, h):
    """The input: parchment sunk in a gold frame, granules along the long runs (tile 48 x 6; it grows with the
    text, so the corners stay inside the 20/22 px border cells)."""
    box = (1.5, 1.5, 94.5, 44.5)
    outer = rrect(*box, 15)
    inner = rrect(box[0] + 2.4, box[1] + 2.4, box[2] - 2.4, box[3] - 2.4, 12.6)
    s.slab(outer, -1.6, -.8, edge(s), bevel=.3)
    s.slab(outer, -.4, 1.1, gold(s), bevel=.6, holes=[inner])
    s.slab(inner, -.8, .0, parchment(s, PAPER_L), bevel=.3)
    granules(s, runs(s, box, 4.6, 2, 18, sides='tb'), r=.4, z=.2)


@sprite('chat-send', size=(48, 48), shear=.5)
def chat_send(s, w, h):
    """An emerald leather seal in a gilt rim, a ring of granules, a gold arrow in relief."""
    s.slab(circle(24, 24, 21.5), -2.5, 0, edge(s), bevel=.6)
    s.slab(circle(24, 24, 21.5), -.6, 1.2, leather(s, LEATHER), bevel=2.2)
    s.tube(circle(24, 24, 20.6), .9, gold(s), z=1.2, closed=True, flat=.75, caps=None)
    n = 28
    granules(s, [(24 + 17.6 * math.cos(math.tau * k / n), 24 + 17.6 * math.sin(math.tau * k / n)) for k in range(n)],
             r=.45, z=1.3)
    arrow = [(15, 24), (29.5, 24)]           # stops short of the tip: two slabs meeting there leave a dark seam
    head = [(26, 17.5), (32.5, 24), (26, 30.5)]
    for line in (arrow, head):
        s.slab(stroke(line, 1.9), .9, 2.6, gold(s), bevel=.7)


@sprite('chat-bead', size=(16, 16), shear=.4)
def chat_bead(s, w, h):
    s.beads([(8, 8)], 5.6, s.mat('pearl', PEARL), z=0, flat=.85)


@sprite('chat-bead-out', size=(16, 16), shear=.4)
def chat_bead_out(s, w, h):
    s.beads([(8, 8)], 4.4, s.mat('lacquer', '#4a3a33', rough=.35), z=0, flat=.8)


@sprite('flag', size=(32, 32), mask=True)
def flag(s, w, h):
    glyph(s, [(8, 5), (8, 28)], 2.6)
    glyph_fill(s, [(9, 5.5), (25.5, 9.5), (19.5, 13.5), (25.5, 17.5), (9, 19.5)], .3)
