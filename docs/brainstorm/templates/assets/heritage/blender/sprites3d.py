"""The sprites of the Blender kit, drawn as crafted things of the story's world rather than stickers: tooled
leather plaques in antique gold rims with granulation (зернь), parchment bound in dark leather with tambour
(chain) stitches and satin-stitched tulips, gold corner caps, cut rubies, filigree scrolls, a wax seal, stamped
glyphs. Names and roles are the SVG kit's (sprites/*.py); sizes and borders are set here where a pattern needs a
tile of its own.

Every 9-slice sprite is tiled: KitImage repeats a whole number of tiles of its edges and centre (CSS border-image
round), so a pattern is never cut or smeared while the element grows and shrinks. A pattern along a straight edge
therefore sits on a lattice anchored at the tile (`lattice`, `runs`, `chain_runs`) with a step that divides the
tile, and the textures repeat once per tile (studio.Nodes). Ornaments of the corners stay inside the corner cells,
and clear of what the game lays over them (icons, text, the preview picture): the limits are noted per sprite."""
import math
from dataclasses import dataclass
from functools import partial
from typing import Callable, Optional

from kit.geometry import wave
from kit.palette import CREAM, EMERALD, EMERALD_D, INK, MINT, PAPER, PAPER_L, ROSE, ROSE_PAPER, SCARLET, SPRING, WHITE
from kit.registry import Slice
from shapes import (arc, bezier, cartouche, chain, chamfer, circle, dashes, dist, fillet, hatch, length, mirror_x,
                    offset, quad, resample, rhombus, ring, rrect, sparkle, spiral, star, stroke, transform, tulip)

GOLD = '#d8ae64'           # antique gold: rims, wires, granulation
GOLD_L = '#ecd192'
EDGE = '#2b1d15'           # the dark leather of the edges and backs
RUBY = '#c22a2f'
EMERALD_GEM = '#13865a'
PEARL = '#f1e8d6'
LEATHER = '#17654a'        # emerald leather
LEATHER_D = '#0f4a37'
LEATHER_RED = '#b8403c'
LEATHER_IVORY = '#eee0c6'
LEATHER_ROSE = '#e3948e'
STITCH_RED = '#b8312f'
STITCH_GREEN = '#1d6a4c'


@dataclass
class Spec:
    name: str
    build: Callable
    width: int
    height: int
    slice: Optional[Slice]
    tiled: bool
    mask: bool
    halo: str
    shear: float
    samples: int

    @property
    def period(self):
        """The tile of a tiled sprite: textures repeat with exactly this period."""
        if not (self.tiled and self.slice):
            return None
        b = self.slice
        return self.width - b.left - b.right, self.height - b.top - b.bottom


KIT = {}


def sprite(name, size=None, slice=None, tiled=None, mask=False, halo=INK, shear=None, samples=160):
    """tiled: None - every 9-slice sprite is tiled (round); shear: px of drop per px of depth, none for masks."""
    def register(build):
        KIT[name] = dict(build=build, size=size, slice=slice, tiled=tiled, mask=mask, halo=halo,
                         shear=(0.0 if mask else 1.0) if shear is None else shear, samples=samples)
        return build
    return register


def add(name, build, **o):
    sprite(name, **o)(build)


def specs(registry):
    """The sprites with sizes and borders resolved against the SVG kit."""
    out = {}
    for name, o in KIT.items():
        if o['size']:
            w, h = o['size']
        else:
            svg = registry.make(name)
            w, h = svg.width, svg.height
        b = o['slice'] or (registry.entries[name].slice if name in registry.entries else None)
        tiled = bool(b) if o['tiled'] is None else o['tiled']
        out[name] = Spec(name, o['build'], w, h, b, tiled, o['mask'], WHITE if o['mask'] else o['halo'],
                         o['shear'], o['samples'])
    return out


# ================================================================ materials
def gold(s):
    return s.mat('gold', GOLD)


def edge(s):
    return s.mat('lacquer', EDGE)


def leather(s, color):
    return s.mat('leather', color)


def parchment(s, color):
    return s.mat('parchment', color)


def thread(s, color):
    return s.mat('thread', color, sheen=.8)


def gem(s, color=RUBY):
    return s.mat('gem', color)


# ================================================================ lattices: patterns that tile whole
def lattice(a0, a1, anchor, step):
    """Centres anchor + (k + .5) * step inside [a0, a1]: the places of a pattern along a straight run, aligned on
    the tile (whose length is a multiple of step), so that the round tiles of KitImage repeat it whole."""
    out = []
    k = math.ceil((a0 - anchor) / step - .5)
    while True:
        p = anchor + (k + .5) * step
        if p > a1 + 1e-6:
            return out
        if p >= a0 - 1e-6:
            out.append(p)
        k += 1


def runs(s, box, inset, step, margin, sides='tblr'):
    """Points along the straight sides of box = (x0, y0, x1, y1), `inset` px in, on the lattice of the tile;
    `margin`: how far from each corner of the box they stop."""
    x0, y0, x1, y1 = box
    b = s.borders
    pts = []
    for x in lattice(x0 + margin, x1 - margin, b.left, step):
        if 't' in sides:
            pts.append((x, y0 + inset))
        if 'b' in sides:
            pts.append((x, y1 - inset))
    for y in lattice(y0 + margin, y1 - margin, b.top, step):
        if 'l' in sides:
            pts.append((x0 + inset, y))
        if 'r' in sides:
            pts.append((x1 - inset, y))
    return pts


def chain_runs(s, box, inset, step, margin, width=1.3):
    """Loops of a tambour stitch along the four straight sides, one per lattice cell."""
    x0, y0, x1, y1 = box
    b = s.borders
    loops = []
    xs = lattice(x0 + margin, x1 - margin, b.left, step)
    ys = lattice(y0 + margin, y1 - margin, b.top, step)
    for y in (y0 + inset, y1 - inset):
        for x in xs:
            loops += chain([(x - step / 2, y), (x + step / 2, y)], step, width)
    for x in (x0 + inset, x1 - inset):
        for y in ys:
            loops += chain([(x, y - step / 2), (x, y + step / 2)], step, width)
    return loops


def straight(s, box, inset, margin):
    """The four straight lines of a tooled (or stitched) border, from margin to margin: no corners, so that the
    ends lie in the corner cells and the middle is the same in every tile."""
    x0, y0, x1, y1 = box
    return [[(x0 + margin, y0 + inset), (x1 - margin, y0 + inset)], [(x0 + margin, y1 - inset), (x1 - margin, y1 - inset)],
            [(x0 + inset, y0 + margin), (x0 + inset, y1 - margin)], [(x1 - inset, y0 + margin), (x1 - inset, y1 - margin)]]


def corners4(build, w, h, top=0, inset=4):
    """Runs build(place) for the four corners: the motif is drawn for the top-left one; the lower ones mirror about
    h - inset (the room of the depth below)."""
    build(lambda pts: [(x, y + top) for x, y in pts])
    build(lambda pts: [(w - x, y + top) for x, y in pts])
    build(lambda pts: [(x, h - inset - y) for x, y in pts])
    build(lambda pts: [(w - x, h - inset - y) for x, y in pts])


# ================================================================ common parts
def plaque(s, outline, face, depth=3.0, lift=1.0, bevel=1.4, rim=.95):
    """A leather plaque: the dark back (its wall below is the shadow of the SVG kit), the cushioned face, a gilt
    rim round the outline."""
    s.slab(outline, -depth, 0, edge(s), bevel=.6)
    s.slab(outline, -.6, lift, face, bevel=bevel)
    if rim:
        s.tube(fillet(outline, rim), rim, gold(s), z=lift, closed=True, flat=.75, caps=None)


def sheet(s, outline, color, depth=3.0, binding=1.6, lift=.5, piping=None):
    """Parchment on a dark board, its edge bound in dark leather (a cord centred on the outline)."""
    if depth:
        s.slab(outline, -depth, 0, edge(s), bevel=.6)
    s.slab(outline, -.5, lift, parchment(s, color), bevel=.4)
    if binding:
        if piping:
            piping(binding, edge(s), lift)
        else:
            s.tube(fillet(outline, binding), binding, edge(s), z=lift, closed=True, flat=.65, caps=None)


def granules(s, points, r=.5, z=1.0):
    s.beads(points, r, gold(s), z=z, flat=.85)


def stone(s, outline, color=RUBY, z=.6, height=1.6, setting=.7):
    """A cut stone in a gold collet."""
    s.slab(offset(outline, setting), z - .4, z + height * .45, gold(s), bevel=.4)
    s.facet(outline, z + height * .2, z + height, gem(s, color), table=.42)


def satin(s, poly, color, angle, z=.55, spacing=.5):
    """Satin stitch: the shape filled with parallel threads."""
    s.tubes(hatch(poly, spacing, angle), .27, thread(s, color), z=z, flat=.6)


def glyph(s, pts, w):
    """A stamped stroke of a white glyph mask: a flat top, bevelled sides, round caps and joins."""
    s.slab(stroke(pts, w / 2), -.9, .9, s.mat('matte', WHITE), bevel=min(.65, w / 3), anchor=.9)


def glyph_ring(s, pts, w):
    outer, inner = ring(pts, w / 2)
    s.slab(outer, -.9, .9, s.mat('matte', WHITE), bevel=min(.65, w / 3), holes=[inner], anchor=.9)


def glyph_fill(s, outline, grow=0.0):
    s.slab(offset(outline, grow) if grow else outline, -.9, .9, s.mat('matte', WHITE), bevel=.65, anchor=.9)


def svg_arc(p0, p1, r, large, sweep):
    """The SVG arc command A r r 0 large sweep p1 from p0, as points (y down: sweep 1 is clockwise on screen)."""
    mx, my = (p0[0] + p1[0]) / 2, (p0[1] + p1[1]) / 2
    d = dist(p0, p1)
    r = max(r, d / 2)
    h = math.sqrt(max(r * r - d * d / 4, 0))
    nx, ny = -(p1[1] - p0[1]) / d, (p1[0] - p0[0]) / d
    for c in ((mx + nx * h, my + ny * h), (mx - nx * h, my - ny * h)):
        a0, a1 = math.atan2(p0[1] - c[1], p0[0] - c[0]), math.atan2(p1[1] - c[1], p1[0] - c[0])
        delta = (a1 - a0) % math.tau if sweep else -((a0 - a1) % math.tau)
        if (abs(delta) > math.pi) == bool(large):
            return arc(c[0], c[1], r, a0, a0 + delta)
    raise ValueError('no arc')


def scallop(a, b, r):
    """`A r r 0 0 1 b` from a: the small clockwise arc, a bump outward of a clockwise outline."""
    return svg_arc(a, b, r, 0, 1)


def leaf(x0, y0, x1, y1, bulge):
    """A leaf: two quadratic curves through the ends, bulging to both sides."""
    mx, my = (x0 + x1) / 2, (y0 + y1) / 2
    nx, ny = -(y1 - y0), x1 - x0
    ln = math.hypot(nx, ny)
    nx, ny = nx / ln * bulge, ny / ln * bulge
    return quad((x0, y0), (mx + nx, my + ny), (x1, y1))[:-1] + quad((x1, y1), (mx - nx, my - ny), (x0, y0))[:-1]


def corner_cap(s, place, size, z=.5):
    """A gold corner bracket over the corner of a card (an L of 3 px wide arms, `size` long), a rosette of
    granules where the arms meet."""
    k, t = size, 3.2
    pts = fillet([(1, 1), (k, 1), (k, t), (t, t), (t, k), (1, k)], .6)
    s.slab(place(pts), z - .3, z + .9, gold(s), bevel=.45)
    granules(s, place([(2.1, 2.1)]), r=.8, z=z + .9)
    granules(s, place([(k - 1.2, 2.1), (2.1, k - 1.2)]), r=.42, z=z + .9)


# ================================================================ buttons and keys
def button(s, w, h, face):
    """The option button: a leather cartouche (concave corners) in a gilt rim, a tooled gold line and a row of
    granules inside, a gold drop in every corner. Clear of the game's icons: they start 12 px from the ends."""
    body = cartouche(1.5, 1.5, w - 1.5, 45.5, 5)
    plaque(s, body, leather(s, face))
    s.tubes(straight(s, (1.5, 1.5, w - 1.5, 45.5), 3.6, 8), .3, gold(s), z=.95)
    granules(s, runs(s, (1.5, 1.5, w - 1.5, 45.5), 5.8, 2, 10))
    corners4(lambda place: granules(s, place([(6.2, 6.2)]), r=.95, z=1.0), w, 47, inset=0)


for _name, _face in (('btn', LEATHER), ('btn-pressed', LEATHER_RED), ('btn-chosen', LEATHER_IVORY)):
    add(_name, partial(button, face=_face), slice=Slice(16, 22, 22, 20))


def key(s, w, h, face):
    """The square arrow key 48x48: the button's cartouche."""
    box = (1.5, 1.5, 46.5, 44.5)
    plaque(s, cartouche(*box, 5), leather(s, face))
    s.tubes(straight(s, box, 3.6, 8), .3, gold(s), z=.95)
    granules(s, runs(s, box, 5.8, 2, 10))
    corners4(lambda place: granules(s, place([(6.2, 6.2)]), r=.95, z=1.0), 48, 46, inset=0)


add('key', partial(key, face=LEATHER))
add('key-pressed', partial(key, face=LEATHER_RED))


@sprite('field')
def field(s, w, h):
    """The value field between the arrows: parchment sunk in a gold frame, a ring of emerald beads."""
    outline = rrect(1.25, 1.25, 46.75, 46.75, 10)
    outer, inner = offset(outline, 0), offset(outline, -2.3)
    s.slab(outline, -1.5, -.8, edge(s), bevel=.3)
    s.slab(outer, -.4, 1.1, gold(s), bevel=.6, holes=[inner])
    s.slab(inner, -.7, .1, parchment(s, PAPER_L), bevel=.3)
    s.beads(runs(s, (1.25, 1.25, 46.75, 46.75), 4.8, 4, 9), .62, gem(s, EMERALD_GEM), z=0, flat=.7)
    corners4(lambda place: granules(s, place([(5.6, 5.6)]), r=.8, z=.2), 48, 48, inset=0)


@sprite('banner')
def banner(s, w, h):
    """The ribbon of a changed stat: emerald velvet with notched ends in a gilt rim, gold embroidery along it
    (a line and a chain of rhombuses). Its height never changes; the ends are clear of the stat icon (12..44)."""
    y0, y1 = 1.5, 50.5
    mid = (y0 + y1) / 2
    body = [(1.5, y0), (w - 1.5, y0), (w - 9, mid), (w - 1.5, y1), (1.5, y1), (9, mid)]
    plaque(s, body, s.mat('velvet', LEATHER), bevel=1.6)
    for y in (6.2, 44.8):
        s.tubes([[(16, y), (w - 16, y)]], .32, gold(s), z=.95)
    for y in (9.8, 41.2):
        for x in lattice(20, w - 20, s.borders.left, 6):
            s.slab(rhombus(x, y, 1.35), .6, 1.4, gold(s), bevel=.35)


@sprite('pill')
def pill(s, w, h):
    """The coin balance: dark leather in a gilt rim, granules along it."""
    body = rrect(1.5, 1.5, 94.5, 44.5, 21.5)
    plaque(s, body, leather(s, LEATHER_D))
    granules(s, runs(s, (1.5, 1.5, 94.5, 44.5), 4.6, 2, 24, sides='tb'))
    for cx, a0, a1 in ((23, .5 * math.pi, 1.5 * math.pi), (73, -.5 * math.pi, .5 * math.pi)):
        granules(s, [p for p in arc(cx, 23, 16.9, a0, a1, 26)[1:-1]])


# ================================================================ seals and medallions
def medallion(s, cx, cy, r, face, depth=2.0, beads_r=None):
    """A gold ring with granulation round a face (leather or enamel)."""
    s.slab(circle(cx, cy, r), -depth, 0, edge(s), bevel=.6)
    s.slab(circle(cx, cy, r), -.4, 1.2, gold(s), bevel=.8, holes=[circle(cx, cy, r - 2.6)])
    s.slab(circle(cx, cy, r - 2.4), -.6, .7, face, bevel=1.4)
    br = beads_r or r - 1.3
    n = int(math.tau * br / 1.7)
    granules(s, [(cx + br * math.cos(math.tau * k / n), cy + br * math.sin(math.tau * k / n)) for k in range(n)],
             r=.42, z=1.2)


@sprite('plus')
def plus(s, w, h):
    medallion(s, 16, 16, 14.5, leather(s, LEATHER))
    for line in ([(16, 9.8), (16, 22.2)], [(9.8, 16), (22.2, 16)]):
        s.slab(stroke(line, 1.6), .4, 1.9, gold(s), bevel=.6)
    stone(s, rhombus(16, 16, 1.6), z=1.4, height=1.1, setting=.4)


@sprite('round')
def round_seal(s, w, h):
    """The seal under the collapse arrow: dark leather in a gold ring of granules."""
    medallion(s, 20, 20, 18, leather(s, LEATHER_D))
    n = 28
    granules(s, [(20 + 13 * math.cos(math.tau * k / n), 20 + 13 * math.sin(math.tau * k / n)) for k in range(n)], r=.35, z=.8)


@sprite('gear')
def gear(s, w, h):
    """The settings seal: a gold medallion, a gold gear on dark leather, a ruby in the middle."""
    cx, cy = 22, 22
    medallion(s, cx, cy, 20, leather(s, LEATHER_D), depth=2.5)
    pts = []
    half, r_ring, r_tip = 2.6, 9.4, 11.2
    base = math.sqrt(r_ring ** 2 - half ** 2)
    for i in range(8):
        axis = -math.pi / 2 + i * math.tau / 8
        tooth = [(axis + math.atan2(t, r), math.hypot(t, r)) for t, r in ((-half, base), (-half, r_tip), (half, r_tip), (half, base))]
        pts += [(cx + r * math.cos(a), cy + r * math.sin(a)) for a, r in tooth]
        pts += arc(cx, cy, r_ring, tooth[-1][0], axis + math.tau / 8 - math.atan2(half, base))[1:-1]
    s.slab(fillet(pts, .6), -.2, 1.7, gold(s), bevel=.55, holes=[circle(cx, cy, 5.6)])
    s.slab(circle(cx, cy, 5.8), -.4, .8, leather(s, LEATHER_D), bevel=.6)
    stone(s, circle(cx, cy, 2.6, 12), z=.6, height=1.6, setting=.6)


def switch(s, w, h, on):
    """The sound switch: a leather track in a gilt rim, a pearl knob in a gold collet with a stone."""
    body = rrect(1.5, 1.5, 58.5, 29.5, 14)
    plaque(s, body, leather(s, LEATHER if on else '#4b3a31'), bevel=1.8)
    s.tube(fillet(rrect(5.2, 5.2, 54.8, 25.8, 10.3), .3), .3, gold(s), z=.95, closed=True, flat=.8, caps=None)
    x = 41 if on else 17
    s.slab(circle(x, 15.5, 11.4), -1.6, 1.3, edge(s), bevel=.6)
    s.slab(circle(x, 15.5, 10.4), -.6, 2.0, gold(s), bevel=.7)
    s.slab(circle(x, 15.5, 8.6), .4, 2.9, s.mat('pearl', PEARL), bevel=2.2)
    stone(s, rhombus(x, 15.5, 3.2), RUBY if on else '#8f8478', z=2.5, height=1.2, setting=.45)


add('switch-on', partial(switch, on=True))
add('switch-off', partial(switch, on=False))


# ================================================================ white glyph masks (the game tints them)
SPEAKER = [(4, 12), (9, 12), (16, 6), (16, 26), (9, 20), (4, 20)]


@sprite('chevron', mask=True)
def chevron(s, w, h):
    glyph(s, [(4, 3), (12.5, 10), (4, 17)], 3.6)


@sprite('chevron-down', mask=True)
def chevron_down(s, w, h):
    glyph(s, [(3, 3), (11, 11), (19, 3)], 4)


@sprite('sound', mask=True)
def sound(s, w, h):
    glyph_fill(s, SPEAKER, .8)
    glyph(s, quad((20.5, 11.5), (23.5, 16), (20.5, 20.5)), 2.6)
    glyph(s, quad((24.5, 7.5), (30, 16), (24.5, 24.5)), 2.6)


@sprite('sound-off', mask=True)
def sound_off(s, w, h):
    glyph_fill(s, SPEAKER, .8)
    glyph(s, [(20.5, 12), (28.5, 20)], 2.6)
    glyph(s, [(28.5, 12), (20.5, 20)], 2.6)


@sprite('home', mask=True)
def home(s, w, h):
    """A yurt: "back to the menu"."""
    glyph(s, quad((3.5, 16), (16, 3), (28.5, 16)), 2.6)
    glyph(s, [(28.5, 16), (28.5, 27.5), (3.5, 27.5), (3.5, 16)], 2.6)
    glyph(s, [(16, 6), (16, 3.5)], 2.6)
    glyph(s, [(3.5, 16), (28.5, 16)], 2.6)
    glyph_fill(s, [(12.5, 27.5), (12.5, 21)] + quad((12.5, 21), (16, 18), (19.5, 21))[1:-1] + [(19.5, 21), (19.5, 27.5)])


@sprite('restart', mask=True)
def restart(s, w, h):
    glyph(s, svg_arc((24.5, 10.5), (26, 17.5), 10, 1, 0), 2.8)
    glyph(s, [(26.5, 3.5), (26.5, 11.5), (18.5, 11.5)], 2.8)


@sprite('exit', mask=True)
def exit_door(s, w, h):
    glyph(s, [(15, 5), (6.5, 5), (6.5, 27), (15, 27)], 2.6)
    glyph(s, [(12, 16), (26, 16)], 2.8)
    glyph(s, [(22, 10.5), (27.5, 16), (22, 21.5)], 2.8)


@sprite('download', mask=True)
def download(s, w, h):
    glyph(s, [(16, 4), (16, 18)], 2.8)
    glyph(s, [(9.5, 13), (16, 19.5), (22.5, 13)], 2.8)
    glyph(s, [(5, 20), (5, 26.5), (27, 26.5), (27, 20)], 2.6)


# ================================================================ the menu: card, frame, star
@sprite('card')
def card(s, w, h):
    """The story card and the settings toast: parchment on a dark board, bound in leather, a scarlet tambour
    stitch and a thin gold line round it, gold book corners. Everything within 11 px of the edge: the preview
    picture starts 12 px in, the buttons 10 px from the sides and 22 px from the bottom."""
    box = (1.5, 1.5, 94.5, 90.5)
    sheet(s, rrect(*box, 3.5), PAPER, depth=4, binding=1.3)
    s.tubes(chain_runs(s, box, 5.6, 2, 12, width=1.25), .26, thread(s, STITCH_RED), z=.55, closed=True)
    s.tubes(straight(s, box, 8.6, 12), .25, gold(s), z=.55)
    corners4(lambda place: corner_cap(s, place, 11), w, h, inset=96 - 92)


@sprite('frame')
def frame(s, w, h):
    """The frame over a preview picture: a gilt moulding (a round bead, a groove, a row of granules), rosettes in
    the corners. Within 6 px of the edge; no centre."""
    s.tube(rrect(1.5, 1.5, 30.5, 30.5, 2.2), 1.45, gold(s), z=0, closed=True, flat=.8, caps=None)
    s.tubes(straight(s, (1.5, 1.5, 30.5, 30.5), 2.9, 4.5), .32, edge(s), z=-.1)
    granules(s, runs(s, (1.5, 1.5, 30.5, 30.5), 3.9, 2, 5.5), r=.42, z=.1)
    corners4(lambda place: granules(s, place([(4.3, 4.3)]), r=1.1, z=.2), 32, 32, inset=0)


@sprite('star', shear=.5)
def star_sprite(s, w, h):
    """The mark of a finished story: a cut ruby star in a gold setting."""
    s.slab(sparkle(12, 12, 10.6), -1.0, .5, gold(s), bevel=.35)
    s.facet(sparkle(12, 12, 8.4), .3, 2.2, gem(s), table=.3)


# ================================================================ plates
def tulip_corner(s, place, red, green):
    """A satin-stitched tulip on its stem with two leaves, in the corner cell (13..25 px from the edges)."""
    a = -math.pi / 4                                    # pointing at the corner
    flower = transform(tulip(0, 0, 1), 19.5, 19.5, 6.4, a)
    satin(s, place(flower), red, a + math.pi / 2 + .5)
    s.tubes([place([(19.5, 19.5), (23, 23)])], .32, thread(s, green), z=.5)
    for side in (1, -1):
        lf = leaf(21, 21, 21 + 4.2 * (1 if side > 0 else -.15), 21 + 4.2 * (-.15 if side > 0 else 1), 1.2 * side)
        satin(s, place(lf), green, math.pi / 4)


def plate(s, w, h, fill, red, green):
    """96x96, slice 32 (tile 32): parchment bound in dark leather, a double tambour stitch, satin-stitched tulips
    in the corners (the text keeps 30 px from the edges, the ornaments end 25 px in)."""
    box = (2, 2, 94, 90)
    sheet(s, chamfer(*box, 10), fill, binding=1.75)
    s.tubes(chain_runs(s, box, 6.2, 2, 13, width=1.35), .28, thread(s, red), z=.55, closed=True)
    s.tubes(straight(s, box, 9.6, 13), .3, thread(s, green), z=.5)
    corners4(lambda place: (tulip_corner(s, place, red, green),
                            granules(s, place([(9.2, 9.2)]), r=.8, z=.5)), w, h)


for _name, _fill, _red, _green in (('plate', PAPER, STITCH_RED, STITCH_GREEN), ('plate-mint', MINT, STITCH_GREEN, STITCH_RED),
                                   ('plate-rose', ROSE_PAPER, STITCH_RED, STITCH_GREEN)):
    add(_name, partial(plate, fill=_fill, red=_red, green=_green))


def plate_tail(s, w, h, mirror):
    """The plate with its speech horn in one outline, 32 px of headroom for the horn. 204x128 (tile 32 x 32);
    slice 64 on top, 32 on the left, 140 on the right (the horn lives there), 32 at the bottom. plate-tail-r has
    the horn in its right part; `mirror` (plate-tail-l) in the left one."""
    top = 32
    box = (2, 2 + top, w - 2, 90 + top)
    y0 = box[1]
    ox = w - 132
    xa, xb = ox + 20, ox + 64                        # the base of the tail on the top edge
    apex = (ox + 3, 3.5)
    pts = chamfer(*box, 10)
    # A tail that grows out of the plate: both sides leave the top edge along it (no corner at the base), the near
    # side is a long concave sweep, the far one an S that swells and then narrows to a rounded tip.
    near = bezier((xa - 9, y0), (xa + 3, y0), (ox + 13, y0 - 9), apex)
    far = bezier(apex, (ox + 13, y0 - 26), (ox + 30, y0 - 6), (xb - 4, y0 - 1.2))[1:] + bezier(
        (xb - 4, y0 - 1.2), (xb + 1, y0 - .2), (xb + 6, y0), (xb + 12, y0))[1:]
    spine = bezier(((xa + xb) / 2 + 2, y0 + 1), ((xa + xb) / 2 - 8, y0 - 6), (ox + 17, y0 - 13), (ox + 10.5, y0 - 21.5))
    tail = near + far
    outline = [pts[0]] + tail + pts[1:]
    if mirror:
        outline, spine = mirror_x(outline, w), mirror_x(spine, w)
    tip = mirror_x([apex], w)[0] if mirror else apex
    k = outline.index(tip)

    def piping(r, mat, z):
        """Open at the tip of the tail and closed there by its round cap: no miter spike."""
        line = outline[k:] + outline[:k + 1]
        s.tube(fillet(line, r, closed=False), r, mat, z=z, flat=.65, caps='round')

    sheet(s, outline, PAPER, binding=1.75, piping=piping)
    # the embroidery follows the tail: a tambour stitch along its spine, a gold bead at its tip
    s.tubes(chain(spine, 2, 1.2), .26, thread(s, STITCH_RED), z=.55, closed=True)
    granules(s, [spine[-1]], r=.9, z=.6)
    granules(s, [tip], r=1.5, z=.9)
    s.tubes(chain_runs(s, box, 6.2, 2, 13, width=1.35), .28, thread(s, STITCH_RED), z=.55, closed=True)
    s.tubes(straight(s, box, 9.6, 13), .3, thread(s, STITCH_GREEN), z=.5)
    corners4(lambda place: (tulip_corner(s, place, STITCH_RED, STITCH_GREEN),
                            granules(s, place([(9.2, 9.2)]), r=.8, z=.5)), w, h, top=top)


add('plate-tail-r', partial(plate_tail, mirror=False), size=(204, 128), slice=Slice(64, 32, 140, 32))
add('plate-tail-l', partial(plate_tail, mirror=True), size=(204, 128), slice=Slice(64, 140, 32, 32))


# ================================================================ clouds (tiled edges, whole tiles in the layout)
def stitches(s, line, color, r, z=.4, dip=.7):
    """Dashes of thread that dive into the paper at both ends: the dashed outlines of the clouds."""
    paths, heights = [], []
    for d in line:
        pts = resample(d, .25)
        if len(pts) < 2:
            continue
        total = length(pts)
        zs, acc = [], 0.0
        for k, p in enumerate(pts):
            if k:
                acc += math.dist(pts[k - 1], p)
            t = acc / total if total else 0
            zs.append(z - dip * (1 - math.sin(math.pi * t) ** .3))
        paths.append(pts)
        heights.append(zs)
    s.tubes(paths, r, thread(s, color), zs=heights, flat=.6, anchor=z + r * .6)


def gold_star(s, cx, cy, r, z=.3):
    """A small star of polished gold with a ruby (the sparkles of the SVG kit)."""
    s.slab(sparkle(cx, cy, r, .2), z - .3, z + .8, gold(s), bevel=.3)
    if r > 6:
        s.facet(rhombus(cx, cy, r * .22), z + .6, z + 1.5, gem(s), table=.35)


def cloud_edge(s, outline, inset=3.4, binding=1.2):
    """The edge of a cloud: a thin dark leather binding on the outline and, inside it, a gold line that follows
    every bump (the outline shrunk by `inset`)."""
    s.tube(fillet(outline, binding), binding, edge(s), z=.7, closed=True, flat=.65, caps=None)
    s.tube(offset(outline, -inset), .36, gold(s), z=.75, closed=True, flat=.8, caps=None)


@sprite('thought', shear=.3)
def thought(s, w, h):
    """The "drop" cloud: a domed parchment cushion with a small wave (one period per 40 px tile), bound in dark
    leather, a row of gold granules inside (on the tile's lattice), gold stars in the corners."""
    cell, radius, inset, amp = 40, 34, 5, 1.5
    lo, hi = inset, 120 - inset

    def wy(x, y):
        return y + amp * math.sin(math.tau * (x - cell) / 40)

    top = wave(cell, 80, lo, amp)
    bottom = [(x, wy(x, hi)) for x, _ in wave(cell, 80, hi, amp)][::-1]
    left = [(wy(y, lo), y) for _, y in wave(cell, 80, lo, amp, vertical=True)][::-1]
    right = [(wy(y, hi), y) for _, y in wave(cell, 80, hi, amp, vertical=True)]
    tl = arc(lo + radius, lo + radius, radius, math.pi, 1.5 * math.pi)
    tr = arc(hi - radius, lo + radius, radius, 1.5 * math.pi, 2 * math.pi)
    br = arc(hi - radius, hi - radius, radius, 0, .5 * math.pi)
    bl = arc(lo + radius, hi - radius, radius, .5 * math.pi, math.pi)
    outline = tl + top + tr + right + br + bottom + bl + left
    s.slab(outline, -.6, .8, parchment(s, PAPER), bevel=2.6)
    s.tube(fillet(outline, 1.2), 1.2, edge(s), z=.7, closed=True, flat=.65, caps=None)
    # granules 4.6 px inside the wavy edge: at lattice x (top, bottom) and y (sides); along the corner arcs
    d, step = 4.6, 2.5
    pts = []
    for x in lattice(36, 84, 40, step):
        pts += [(x, wy(x, lo) + d), (x, wy(x, hi) - d)]
    for y in lattice(36, 84, 40, step):
        pts += [(wy(y, lo) + d, y), (wy(y, hi) - d, y)]
    for cx, cy, a0 in ((lo + radius, lo + radius, math.pi), (hi - radius, lo + radius, 1.5 * math.pi),
                       (hi - radius, hi - radius, 0), (lo + radius, hi - radius, .5 * math.pi)):
        r = radius - d
        n = int(r * math.pi / 2 / step)
        pts += [(cx + r * math.cos(a0 + .5 * math.pi * (k + .5) / n), cy + r * math.sin(a0 + .5 * math.pi * (k + .5) / n)) for k in range(n)]
    granules(s, pts, r=.5, z=.75)
    for cx, cy, r in ((106, 14, 12.5), (14, 96, 9), (24, 110, 6)):
        gold_star(s, cx, cy, r, z=.8)


@sprite('thought-cloud', shear=.3)
def thought_cloud(s, w, h):
    """The puffy cloud: parchment scallops (2 per 40 px tile), each a domed puff, bound in dark leather, a gold
    line inside that follows every scallop, gold granules in the hollows between them, gold stars (the small ones
    4.2: with the setting a ray of 5 would reach into the edge tile and repeat along the side)."""
    b, e = 8, 112
    chain_pts = [(b, 40), (b, 20), (20, b), (40, b), (60, b), (80, b), (100, b), (e, 20), (e, 40), (e, 60), (e, 80),
                 (e, 100), (100, e), (80, e), (60, e), (40, e), (20, e), (b, 100), (b, 80), (b, 60), (b, 40)]
    radii = [12 if i in (1, 6, 11, 16) else 11 for i in range(len(chain_pts) - 1)]
    arcs = [scallop(chain_pts[i], chain_pts[i + 1], radii[i]) for i in range(len(chain_pts) - 1)]
    outline = [p for a in arcs for p in a[:-1]]
    s.slab(outline, -.6, .8, parchment(s, PAPER), bevel=2.4)
    cloud_edge(s, outline)
    s.beads([a[0] for a in arcs], 1.2, edge(s), z=.7, flat=.65)          # the cusps: round joins of the binding
    hollows = []
    for x, y in chain_pts[:-1]:
        nx = 1 if x == b else -1 if x == e else 0
        ny = 1 if y == b else -1 if y == e else 0
        if nx and ny:
            continue                                                    # the corner diagonals have no hollow
        hollows.append((x + 5.2 * nx, y + 5.2 * ny))
    granules(s, hollows, r=.75, z=.8)
    for cx, cy, r in ((102, 24, 8.5), (86, 34, 4.2), (18, 96, 8.5), (34, 86, 4.2)):
        gold_star(s, cx, cy, r, z=.8)


@sprite('info', shear=.3)
def info(s, w, h):
    """The info cloud: parchment scallops (2 per 32 px tile) bound in dark leather, satin-stitched leaves and a
    ruby in the corners (they end at 31: a part touching the corner cell's edge would repeat along the side)."""
    base, cell, size, small, big = 6, 32, 96, 10, 20
    e = size - base
    links = [((cell, base), None), ((48, base), small), ((64, base), small), ((e, cell), big), ((e, 48), small),
             ((e, 64), small), ((64, e), big), ((48, e), small), ((32, e), small), ((base, 64), big),
             ((base, 48), small), ((base, 32), small), ((cell, base), big)]
    arcs = [scallop(links[i][0], links[i + 1][0], links[i + 1][1]) for i in range(len(links) - 1)]
    outline = [p for a in arcs for p in a[:-1]]
    s.slab(outline, -.6, .8, parchment(s, PAPER), bevel=2.2)
    cloud_edge(s, outline, inset=3.2, binding=1.25)
    s.beads([a[0] for a in arcs], 1.25, edge(s), z=.7, flat=.65)        # the cusps: round joins of the binding

    def motif(place):
        satin(s, place(leaf(14, 20, 15, 30.5, 2.6)), STITCH_GREEN, 0, z=.95)
        satin(s, place(leaf(20, 14, 30.5, 15, -2.6)), STITCH_GREEN, math.pi / 2, z=.95)
        stone(s, place(rhombus(17, 17, 2.4)), z=.9, height=1.3, setting=.5)
    corners4(motif, size, size, inset=0)


# ================================================================ tags and their ornaments
def tag(s, w, h, face):
    """The name tag on the top edge of a plate: a leather cartouche in a gilt rim, granules along it (the game's
    rhombuses sit 12 px from the ends, the beads 6 px)."""
    box = (2, 2, 78, 31.5)
    plaque(s, cartouche(*box, 4), leather(s, face), bevel=1.8)
    granules(s, runs(s, box, 4.2, 2, 8), r=.38)
    corners4(lambda place: granules(s, place([(5.4, 5.4)]), r=.75, z=1.0), w, 33.5, inset=0)


add('label', partial(tag, face=LEATHER), slice=Slice(15, 14, 14, 17))
add('label-rose', partial(tag, face=LEATHER_ROSE), slice=Slice(15, 14, 14, 17))


@sprite('label-orn', shear=.4)
def tag_ornament(s, w, h):
    stone(s, rhombus(5, 5, 3.2), z=-.3, height=1.4, setting=.6)


@sprite('gem', shear=.5)
def gem_sprite(s, w, h):
    """The gem at the end of the "Факт" tag: a cut ruby in a gold collet ringed with granules."""
    s.slab(rhombus(14, 14, 12.4), -1.5, .3, edge(s), bevel=.5)
    s.slab(rhombus(14, 14, 12), -.4, 1.0, gold(s), bevel=.6)
    s.facet(rhombus(14, 14, 8.8), .7, 3.0, gem(s), table=.38)
    granules(s, [(14, 3.2), (24.8, 14), (14, 24.8), (3.2, 14)], r=.7, z=1.0)


@sprite('orn', shear=.5)
def orn(s, w, h):
    """The jewel at the left end of an option: a cut ruby in gold between two filigree scrolls, pearls above and
    below."""
    for sign in (1, -1):
        cx = 12 + sign * 7.2
        s.tubes([spiral(cx, 12, .4, 3.2, 1.3, a0=0 if sign > 0 else math.pi, cw=sign > 0)], .32, gold(s), z=.3)
    stone(s, rhombus(12, 12, 4.4), z=.2, height=1.9, setting=.8)
    s.beads([(12, 3.4), (12, 20.6)], 1.05, s.mat('pearl', PEARL), z=.4, flat=.8)


@sprite('bubble', shear=.5)
def bubble(s, w, h):
    """A thought bubble: a domed parchment puff in a thin binding, a ring of gold granules."""
    s.slab(circle(12, 12, 10), -.8, 1.0, parchment(s, PAPER), bevel=2.6)
    s.tube(circle(12, 12, 10), .95, edge(s), z=.8, closed=True, flat=.65, caps=None)
    n = 18
    granules(s, [(12 + 7.2 * math.cos(math.tau * k / n), 12 + 7.2 * math.sin(math.tau * k / n)) for k in range(n)], r=.42, z=1.0)


@sprite('badge')
def badge(s, w, h):
    """The "!" seal of the info cloud: a scarlet wax seal with a stamped rim and a gold "!"."""
    blob = [(22 + (19.2 + .9 * math.cos(9 * a)) * math.cos(a), 22 + (19.2 + .9 * math.cos(9 * a)) * math.sin(a))
            for a in (math.tau * k / 180 for k in range(180))]
    s.slab(blob, -2, 0, s.mat('wax', '#8e2420'), bevel=.8)
    s.slab(blob, -.6, 1.4, s.mat('wax', SCARLET), bevel=2.4)
    s.slab(circle(22, 22, 15.2), .9, 1.5, s.mat('wax', '#c94a46'), bevel=.4, holes=[circle(22, 22, 13.6)])
    s.slab(stroke([(22, 12.5), (22, 23.5)], 1.7), 1.0, 2.6, gold(s), bevel=.6)
    s.slab(circle(22, 29.5, 2.1), 1.0, 2.6, gold(s), bevel=.7)


@sprite('knot', shear=.3)
def knot(s, w, h):
    """The knot over the corner of a plate: two twisted scarlet cords interlaced (over, then under, at every
    crossing: real heights), a small one inside, pearls in the middles, two gold sparkles."""
    a, b, c = rhombus(28, 32, 19), rhombus(42, 26, 15), rhombus(28, 32, 9.5)

    def crossings(p, q):
        out = []
        for e1 in zip(p, p[1:] + p[:1]):
            for e2 in zip(q, q[1:] + q[:1]):
                x = cross(e1, e2)
                if x:
                    out.append(x)
        return out

    ab = crossings(a, b)
    over_a, under_a = ab[0::2], ab[1::2]
    cb = crossings(c, b)

    def heights(pts, ups, downs):
        return [sum(1.5 * math.exp(-(dist(p, q) / 4.5) ** 2) for q in ups)
                - sum(1.5 * math.exp(-(dist(p, q) / 4.5) ** 2) for q in downs) for p in pts]

    cord = s.mat('thread', '#b8322f', sheen=.9)
    for pts, ups, downs in ((a, over_a, under_a), (b, under_a + cb, over_a), (c, cb, [])):
        line = resample(pts + pts[:1], .25)
        zs = heights(line, ups, downs)
        s.tubes([line], 1.9, edge(s), zs=[[z - 1.2 for z in zs]], flat=.55, anchor=1.6)
        s.tube(line, 1.35, cord, zs=zs, flat=.8, caps=None, twist=(1.6, 3), anchor=1.6)
    s.beads([(28, 32), (42, 26)], 2.0, s.mat('pearl', PEARL), z=1.6, flat=.75)
    for cx, cy, r in ((58, 7, 4.6), (7, 53, 3.4)):
        s.slab(sparkle(cx, cy, r, .2), -.4, .7, gold(s), bevel=.3)


def cross(e1, e2):
    (x1, y1), (x2, y2) = e1
    (x3, y3), (x4, y4) = e2
    d = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4)
    if abs(d) < 1e-9:
        return None
    t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / d
    u = -((x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3)) / d
    if 0 < t < 1 and 0 < u < 1:
        return x1 + t * (x2 - x1), y1 + t * (y2 - y1)
    return None


# ================================================================ heads-up: coin, timer, stat icon
def coin_at(s, cx, cy, r, z=0.0, detail=True):
    """A coin of the kit: a thick polished gold rim, a ruby enamel field, a gold tulip standing on it. Bold enough
    to read at 20 px: the eye sees a gold ring round a red disc with a gold flower."""
    shine = s.mat('gold', GOLD_L, rough=.12)
    s.slab(circle(cx, cy, r), z - 1.2, z, edge(s), bevel=.5)
    s.slab(circle(cx, cy, r), z - .3, z + 1.6, shine, bevel=r * .09, holes=[circle(cx, cy, r * .7)])
    s.slab(circle(cx, cy, r * .72), z - .4, z + .8, s.mat('enamel', '#b3262b', rough=.18, coat=1), bevel=r * .05)
    if detail:
        n = max(12, int(math.tau * r * .86 / 2.1))
        granules(s, [(cx + r * .86 * math.cos(math.tau * k / n), cy + r * .86 * math.sin(math.tau * k / n)) for k in range(n)],
                 r=r * .035, z=z + 1.6)
    flower = tulip(cx, cy + r * .42, r * .82)
    s.slab(flower, z + .5, z + 2.2, shine, bevel=r * .05)
    s.slab(stroke([(cx, cy + r * .4), (cx, cy + r * .56)], r * .06), z + .5, z + 1.8, shine, bevel=r * .03)


@sprite('coin')
def coin(s, w, h):
    """The coin of the balance and of the paid options."""
    coin_at(s, 24, 24, 21.5)


def pile(s, places):
    """Coins lying on each other: (x, y, z) from the lowest."""
    for x, y, z in places:
        coin_at(s, x, y, 10.5, z=z, detail=False)


add('coins-s', lambda s, w, h: pile(s, [(22, 28, 0), (42, 30, 0), (32, 22, 2.6)]), size=(64, 48))
add('coins-m', lambda s, w, h: pile(s, [(18, 32, 0), (18, 28, 2.6), (18, 24, 5.2), (40, 34, 0), (40, 30, 2.6),
                                        (31, 22, 7.6)]), size=(64, 52))
add('coins-l', lambda s, w, h: pile(s, [(14, 40, 0), (32, 42, 0), (50, 40, 0), (14, 36, 2.6), (32, 38, 2.6), (50, 36, 2.6),
                                        (22, 30, 5.2), (42, 30, 5.2), (32, 24, 7.8), (32, 20, 10.4)]), size=(64, 60))


@sprite('timer', shear=.5)
def timer_ring(s, w, h):
    """The ring of the timer over the pie the game draws: a gold ring with engraved hours, a scarlet enamel ring
    inside it, a ruby flame on top."""
    s.slab(circle(30, 30, 28), -1.2, 0, edge(s), bevel=.5, holes=[circle(30, 30, 24.2)])
    s.slab(circle(30, 30, 27.8), -.3, 1.2, gold(s), bevel=.8, holes=[circle(30, 30, 24.4)])
    s.slab(circle(30, 30, 24.6), -.3, .7, s.mat('enamel', SCARLET), bevel=.4, holes=[circle(30, 30, 22.4)])
    ticks = []
    for i in range(12):
        a = i * math.tau / 12
        ticks.append([(30 + 25.2 * math.cos(a), 30 + 25.2 * math.sin(a)), (30 + 27.2 * math.cos(a), 30 + 27.2 * math.sin(a))])
    s.tubes(ticks, .42, edge(s), z=1.05)
    flame = (bezier((30, .8), (33.5, 4), (34, 7), (32.8, 9.3)) + bezier((32.8, 9.3), (32, 10.8), (30.8, 11.4), (30, 11.4))[1:]
             + bezier((30, 11.4), (28.4, 11.4), (26.8, 10), (27, 7.8))[1:] + bezier((27, 7.8), (27.2, 6.4), (28.2, 5.6), (28.6, 4.4))[1:]
             + bezier((28.6, 4.4), (29.6, 5.6), (29.8, 6.6), (29.6, 7.6))[1:] + bezier((29.6, 7.6), (31, 6.6), (31.4, 4), (30, .8))[1:-1])
    s.slab(flame, .3, 1.5, gold(s), bevel=.35)
    s.facet(offset(flame, -.8), 1.0, 2.6, gem(s), table=.4)


@sprite('stat-steppe', shear=.5)
def stat_steppe(s, w, h):
    """The icon of the stat "Память степи": dark enamel in a gold frame, feather grass of gold wire, a ruby sun."""
    s.slab(rrect(1.25, 1.25, 32.75, 32.75, 6), -1, 1.0, gold(s), bevel=.6, holes=[rrect(3.4, 3.4, 30.6, 30.6, 4.2)])
    s.slab(rrect(3.2, 3.2, 30.8, 30.8, 4.4), -.6, .4, s.mat('enamel', EMERALD_D, rough=.35), bevel=.4)
    s.facet(circle(25.5, 9, 3.1, 16), .3, 2.0, gem(s), table=.45)
    blades = [quad((x0, 28), (qx, qy), (x1, y1)) for x0, qx, qy, x1, y1 in
              ((16, 14, 18, 8, 10), (17, 17, 16, 15, 6.5), (18, 20, 15, 22, 7), (19, 22, 19, 27, 12), (15, 12, 22, 7, 20))]
    s.tubes(blades + [[(6, 28.5), (28, 28.5)]], .75, gold(s), z=.6, flat=.75)
    s.beads([b[-1] for b in blades], .75, gold(s), z=.6, flat=.8)


# ================================================================ the title
@sprite('title-brackets', halo=CREAM, shear=.6)
def brackets(s, w, h):
    """The brackets of the title: a twisted gold cord ending in filigree scrolls, a scarlet silk thread under it,
    a row of granules, rubies and gold beads. Slice 30 / 50 (tile 60 x 20), no centre."""
    g = gold(s)
    for flip in (False, True):
        def v(pts):
            return [(x, h - y) for x, y in pts] if flip else pts
        cord = [(20, 14), (w - 20, 14)]
        s.tube(v(cord), 1.3, g, z=.6, flat=.75, caps='round', twist=(2, 3))
        for sign in (1, -1):
            cx = 20 if sign > 0 else w - 20
            sc = spiral(cx - sign * 0, 20.2, 6.2, .6, 1.6, a0=-math.pi / 2, cw=sign < 0)
            s.tube(v([(cx, 14)] + sc[1:]), .9, g, z=.6, flat=.75, caps='round')
        s.tube(v([(30, 20.5), (w - 30, 20.5)]), .6, thread(s, SCARLET), z=.2, flat=.7, caps='round')
        granules(s, v([(x, 17.6) for x in lattice(42, w - 42, s.borders.left, 3)]), r=.42, z=.4)
        for x in (32, w - 32):
            stone(s, v(rhombus(x, 14, 2.8)), z=1.0, height=1.5, setting=.6)


@sprite('title-orn', shear=.5)
def title_ornament(s, w, h):
    """The centre of a bracket: a gold sunburst round a cut ruby, gold rhombuses and pearls at its sides."""
    rays = []
    for i in range(16):
        a = i * math.tau / 16 + math.tau / 32
        r1 = 14.5 if i % 2 else 12
        rays.append(stroke([(32 + 8.5 * math.cos(a), 16 + 8.5 * math.sin(a)), (32 + r1 * math.cos(a), 16 + r1 * math.sin(a))], .55))
    for r in rays:
        s.slab(r, -.3, .8, gold(s), bevel=.3)
    s.slab(rhombus(32, 16, 9.4), -1, .6, gold(s), bevel=.6)
    s.facet(rhombus(32, 16, 7.2), .4, 2.6, gem(s), table=.4)
    for x in (14, 50):
        s.slab(rhombus(x, 16, 2.6), -.4, .9, gold(s), bevel=.5)
    s.beads([(8.5, 16), (55.5, 16)], 1.2, s.mat('pearl', PEARL), z=.2, flat=.8)


# ================================================================ the menu: story cards, carousel, tabs, shop
@sprite('story-frame', size=(120, 160), slice=Slice(40, 40, 40, 40, fill=False))
def story_frame(s, w, h):
    """The frame of a story card in the carousel, laid over its cover picture: a parchment band bound in dark
    leather, an emerald thread and a scarlet tambour stitch along it, a gold fillet on its inner edge; in the
    corners gold filigree scrolls round a ruby and emerald leaves, reaching over the picture. Tile 40 x 80."""
    outer, band = (1.5, 1.5, w - 1.5, h - 1.5), 12
    inner = (outer[0] + band, outer[1] + band, outer[2] - band, outer[3] - band)
    s.slab(rrect(*outer, 4), -2.5, 0, edge(s), bevel=.5, holes=[rrect(*inner, 1)])
    s.slab(rrect(*outer, 4), -.4, .6, parchment(s, PAPER), bevel=.5, holes=[rrect(*inner, 1)])
    s.tube(fillet(rrect(*outer, 4), 1.3), 1.3, edge(s), z=.5, closed=True, flat=.65, caps=None)
    s.tube(rrect(*inner, 1), .55, gold(s), z=.5, closed=True, flat=.8, caps=None)
    s.tubes(straight(s, outer, 4.4, 14), .3, thread(s, STITCH_GREEN), z=.6)
    s.tubes(chain_runs(s, outer, 7.6, 2, 18, width=1.3), .27, thread(s, STITCH_RED), z=.65, closed=True)

    def corner(place):
        for a0, cw in ((0, True), (math.pi / 2, False)):
            sc = spiral(13 + 9 * math.cos(a0), 13 + 9 * math.sin(a0), .5, 4.2, 1.25, a0=a0 + math.pi, cw=cw)
            s.tubes([place(sc)], .5, gold(s), z=.8)
        satin(s, place(leaf(17, 17, 30, 23, 2.6)), STITCH_GREEN, .9, z=.8)
        satin(s, place(leaf(17, 17, 23, 30, -2.6)), STITCH_GREEN, .7, z=.8)
        stone(s, place(rhombus(11.5, 11.5, 3.6)), z=.8, height=1.7, setting=.8)
    corners4(corner, w, h, inset=0)


@sprite('medallion', size=(40, 40), shear=.5)
def medallion_sprite(s, w, h):
    """The sun rhombus on the top and bottom edges of a story card: ivory and scarlet enamel in gold, a ruby."""
    s.slab(rhombus(20, 20, 18.5), -1.4, 0, edge(s), bevel=.5)
    s.slab(rhombus(20, 20, 18), -.3, 1.0, gold(s), bevel=.6)
    s.slab(rhombus(20, 20, 14.6), .3, 1.4, s.mat('enamel', PAPER_L, rough=.3), bevel=.5)
    s.slab(rhombus(20, 20, 10.6), .9, 1.9, s.mat('enamel', SCARLET, rough=.28), bevel=.5)
    s.slab(rhombus(20, 20, 6.8), 1.4, 2.3, s.mat('enamel', PAPER_L, rough=.3), bevel=.4)
    s.facet(rhombus(20, 20, 4.2), 1.8, 3.6, gem(s), table=.4)
    granules(s, [(20, 3.6), (36.4, 20), (20, 36.4), (3.6, 20)], r=.9, z=1.0)


@sprite('title-plaque', size=(200, 64), slice=Slice(16, 50, 50, 18))
def title_plaque(s, w, h):
    """The plaque of a story's title on its card: emerald leather in a gilt rim with granules, scarlet silk ribbons
    flying from its ends (they live in the side borders; the plaque never changes its height). Tile 100 x 30."""
    box = (20, 12, w - 20, 48)
    for flip in (False, True):
        def m(pts):
            return mirror_x(pts, w) if flip else pts
        # a broad flat silk ribbon tied behind the end of the plaque, its two tails falling apart, cut in a swallowtail
        for path, colour in ((bezier((26, 32), (12, 34), (10, 48), (4, 58)), '#b8302d'),
                             (bezier((26, 26), (12, 22), (8, 12), (3, 4)), '#a3292a')):
            band = stroke(path, 3.3)
            s.slab(m(band), -1.4, -.4, thread(s, colour), bevel=.6)
            end = path[-1]
            s.slab(m([(end[0] - 3.6, end[1]), (end[0] + 3.6, end[1]), (end[0], end[1] + (-3 if end[1] < 30 else 3))]),
                   -1.45, -.3, edge(s), bevel=.2)
    plaque(s, cartouche(*box, 5), leather(s, LEATHER), bevel=1.6)
    s.tubes(straight(s, box, 3.4, 8), .3, gold(s), z=.95)
    granules(s, runs(s, box, 5.6, 2, 10, sides='tb'))
    for x in (box[0] + 6, box[2] - 6):
        stone(s, rhombus(x, 30, 2.6), z=.9, height=1.3, setting=.6)


@sprite('lock', size=(32, 32), mask=True)
def lock(s, w, h):
    glyph(s, arc(16, 14, 6.4, math.pi, 2 * math.pi, 30) + [(22.4, 15)], 3)
    # the keyhole, one outline: round at the top (over the left, the top, the right), the slot below it
    hole = arc(16, 19.6, 2.2, .5 * math.pi + .6, 2.5 * math.pi - .6, 24) + [(17.3, 25), (14.7, 25)]
    s.slab(rrect(7, 13.5, 25, 28.5, 2.6), -.9, .9, s.mat('matte', WHITE), bevel=.65, anchor=.9, holes=[hole])


@sprite('check', size=(32, 32), mask=True)
def check(s, w, h):
    glyph(s, [(7, 16.5), (13.5, 23), (25.5, 9)], 3.8)


@sprite('play', size=(32, 32), mask=True)
def play(s, w, h):
    glyph_fill(s, [(11, 7.5), (25, 16), (11, 24.5)], 1.2)


@sprite('arrow', size=(28, 44), shear=.5)
def arrow(s, w, h):
    """The arrow of the carousel (pointing right; the game mirrors it): a gold chevron with a ruby at its point."""
    line = [(7, 6), (20, 22), (7, 38)]
    s.slab(stroke(line, 3.2), -1.6, 0, edge(s), bevel=.5)
    s.slab(stroke(line, 2.7), -.3, 1.5, gold(s), bevel=.9)
    stone(s, rhombus(18.5, 22, 2.6), z=1.2, height=1.4, setting=.6)


@sprite('dot', size=(14, 14), shear=.4)
def dot(s, w, h):
    s.slab(rhombus(7, 7, 5), -.4, .8, gold(s), bevel=.4, holes=[rhombus(7, 7, 2.9)])


@sprite('dot-on', size=(14, 14), shear=.4)
def dot_on(s, w, h):
    stone(s, rhombus(7, 7, 4.3), z=-.2, height=1.8, setting=.8)


@sprite('tabbar', size=(120, 72), slice=Slice(20, 30, 30, 20))
def tabbar(s, w, h):
    """The bar of the tabs at the bottom of the menu: dark leather under a gilt rim with a row of granules and a
    thin gold line (tile 60 x 32); it runs past both sides of the screen."""
    body = [(-4, 6), (w + 4, 6), (w + 4, h + 4), (-4, h + 4)]
    s.slab(body, -2, .6, leather(s, '#2b221d'), bevel=1.2)
    s.tubes([[(-4, 6.4), (w + 4, 6.4)]], 1.1, gold(s), z=.5)
    granules(s, [(x, 10.6) for x in lattice(-2, w + 2, 30, 2)], r=.48, z=.65)
    s.tubes([[(-4, 13.6), (w + 4, 13.6)]], .3, gold(s), z=.6)


@sprite('tab-stories', size=(40, 40), mask=True)
def tab_stories(s, w, h):
    """Stories by the fire: a campfire, three flames over crossed logs."""
    for cx, cy, size in ((20, 26, 17), (13.5, 28, 10), (26.5, 28, 10)):
        glyph_fill(s, [(cx, cy - size)] + bezier((cx, cy - size), (cx + size * .5, cy - size * .45), (cx + size * .32, cy), (cx, cy))[1:]
                   + bezier((cx, cy), (cx - size * .32, cy), (cx - size * .5, cy - size * .45), (cx, cy - size))[1:-1])
    glyph(s, [(7, 35), (33, 29)], 3.2)
    glyph(s, [(7, 29), (33, 35)], 3.2)


@sprite('tab-library', size=(40, 40), mask=True)
def tab_library(s, w, h):
    """The library: an open book."""
    for sign in (1, -1):
        def m(pts):
            return [(20 + sign * (x - 20), y) for x, y in pts]
        page = [(19, 11)] + quad((19, 11), (12, 7.5), (4.5, 9.5))[1:] + [(4.5, 31)] + quad((4.5, 31), (12, 29), (19, 33))[1:-1]
        glyph_fill(s, m(page))
    glyph(s, [(20, 11), (20, 34)], 1.6)


@sprite('tab-shop', size=(40, 40), mask=True)
def tab_shop(s, w, h):
    """The shop: a chest."""
    glyph_fill(s, [(6, 18), (34, 18), (34, 33), (6, 33)], .4)
    glyph_fill(s, [(6, 15.5)] + quad((6, 15.5), (20, 4), (34, 15.5))[1:])
    s.slab(rrect(16.5, 15, 23.5, 23, 1.2), .5, 1.6, s.mat('matte', WHITE), bevel=.4, anchor=1.6)


@sprite('chest', size=(64, 60))
def chest(s, w, h):
    """The daily gift: a wooden chest with gold bands and corners, a lock plate with a ruby."""
    wood = s.mat('wood', '#7a4024')
    s.slab(rrect(6, 24, 58, 52, 2), -6, 0, wood, bevel=.8)
    s.slab(rrect(5, 8, 59, 27, 6), -2, 2.4, wood, bevel=2.2)
    for x in (16, 48):
        s.slab([(x - 2.4, 7.4), (x + 2.4, 7.4), (x + 2.4, 52.6), (x - 2.4, 52.6)], .2, 2.8, gold(s), bevel=.5)
    for x, y in ((6.5, 25.5), (57.5, 25.5), (6.5, 50.5), (57.5, 50.5)):
        granules(s, [(x, y)], r=1.5, z=2.6)
    s.slab(rrect(26, 20, 38, 34, 2.2), 1.6, 3.4, gold(s), bevel=.6)
    stone(s, rhombus(32, 25.5, 2.6), z=3.0, height=1.4, setting=.5)
    s.slab(rrect(30.6, 28.5, 33.4, 32.5, .6), 3.2, 3.6, edge(s), bevel=.2)


@sprite('lock-frame', size=(64, 64), shear=.5)
def lock_frame(s, w, h):
    """The seal of a locked story on its dark card: a gold rhombus ringed with granules round dark leather (the game
    lays the lock glyph on it)."""
    s.slab(rhombus(32, 32, 30), -1.6, 0, edge(s), bevel=.6)
    s.slab(rhombus(32, 32, 29.5), -.4, 1.2, gold(s), bevel=.8, holes=[rhombus(32, 32, 24.5)])
    s.slab(rhombus(32, 32, 24.8), -.6, .5, leather(s, '#241d19'), bevel=1.4)
    pts = []
    for (ax, ay), (bx, by) in zip(rhombus(32, 32, 27), rhombus(32, 32, 27)[1:] + rhombus(32, 32, 27)[:1]):
        n = 12
        pts += [(ax + (bx - ax) * k / n, ay + (by - ay) * k / n) for k in range(n)]
    granules(s, pts, r=.6, z=1.2)
    for x, y in rhombus(32, 32, 27.5):
        stone(s, rhombus(x, y, 2.2), z=1.0, height=1.2, setting=.5)


@sprite('page', size=(160, 160), slice=Slice(40, 40, 40, 40))
def page(s, w, h):
    """The page of the shop and the library: a calm ground of dark emerald leather with a faint tooled lattice of
    rhombuses (tile 80), a gilt rim with granules round it and gold corner pieces; the content lies on it, not on the
    painting of the fire."""
    box = (2, 2, w - 2, h - 2)
    s.slab(rrect(*box, 8), -2.5, 0, edge(s), bevel=.6)
    s.slab(rrect(*box, 8), -.6, .8, leather(s, '#16382c'), bevel=1.4)
    # the lattice: small blind-tooled rhombuses every 20 px, on the tile (anchored at 40, as the borders)
    marks = [(x, y) for x in lattice(10, w - 10, 40, 20) for y in lattice(10, h - 10, 40, 20)]
    for x, y in marks:
        s.slab(rhombus(x, y, 2.2), .3, .95, leather(s, '#0f2a20'), bevel=.4)
    s.tube(fillet(rrect(*box, 8), 1.1), 1.1, gold(s), z=.8, closed=True, flat=.75, caps=None)
    s.tubes(straight(s, box, 5.2, 14), .3, gold(s), z=.85)
    granules(s, runs(s, box, 8, 2, 16), r=.42, z=.85)
    corners4(lambda place: corner_cap(s, place, 15, z=.8), w, h, inset=0)
