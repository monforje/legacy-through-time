"""The sprites of the Blender kit. One builder per sprite (or family), registered with what differs from the SVG
kit: a tiled paper sprite, a glyph mask the game tints, a different size, a flatter view.

The material language (DESIGN.md, made physical): the ink outline is a lacquered piping centred on the outline
of the SVG kit, the hard sticker shadow is the wall of the ink back; buttons and seals are cloisonné enamel with
gilt wire where the SVG kit has a cream hairline; plates and clouds are paper with grain, their scarlet lines are
twisted silk threads, their dashed outlines are stitches; ornaments are enamel studs. Coordinates and sizes are
the SVG kit's (sprites/*.py), so a sprite of one kit replaces the other's one to one."""
import math
from dataclasses import dataclass
from functools import partial
from typing import Callable, Optional

from kit.geometry import wave
from kit.palette import (BRONZE, BRONZE_D, BRONZE_L, CREAM, EMERALD, EMERALD_D, INK, MINT, PAPER, PAPER_L, ROSE,
                         ROSE_PAPER, SCARLET, SPRING, WHITE)
from kit.registry import Slice
from shapes import (arc, bezier, chamfer, circle, dashes, dist, fillet, hexbar, length, mirror_x, offset, quad,
                    resample, rhombus, rrect, sparkle, star)

GOLD = '#ecce8e'           # the gilt wire: the cream hairlines of the SVG kit
OFF_TRACK = '#5b4a43'
INK_LACQUER = '#120c0a'    # darker than INK: the lacquer's own sheen lifts it back to it


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
        """The tile of a tiled sprite: the paper grain repeats with exactly this period."""
        if not (self.tiled and self.slice):
            return None
        b = self.slice
        return self.width - b.left - b.right, self.height - b.top - b.bottom


KIT = {}


def sprite(name, size=None, slice=None, tiled=False, mask=False, halo=INK, shear=None, samples=160):
    """shear: px of drop per px of depth (the walls under the parts); flat glyph masks get none."""
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
        out[name] = Spec(name, o['build'], w, h, b, o['tiled'], o['mask'], WHITE if o['mask'] else o['halo'],
                         o['shear'], o['samples'])
    return out


# ================================================================ common parts
def ink(s):
    return s.mat('lacquer', INK_LACQUER)


def gold(s):
    return s.mat('metal', GOLD, rough=.32)


def enamel(s, color, **o):
    return s.mat('enamel', color, **o)


def sticker(s, outline, face, rim=2.5, depth=3.0, lift=.6, bevel=2.6, z0=0.0, rim_mat=None, piping=None):
    """The ink back (its wall below is the sticker shadow), the face, and the ink piping centred on the outline
    (the stroke of the SVG kit). `piping`: the path of the piping when it is not the filleted outline."""
    k = ink(s)
    if depth > 0:
        s.slab(outline, z0 - depth, z0, k, bevel=min(.6, depth / 2.5))
    s.slab(outline, z0 - .5, z0 + lift, face, bevel=bevel)
    if rim:
        r = rim / 2
        if piping:
            piping(r, rim_mat or k, z0 + lift)
        else:
            s.tube(fillet(outline, r), r, rim_mat or k, z=z0 + lift, closed=True, flat=.7, caps=None)


def paper_sheet(s, outline, color, rim=2.5, depth=3.0, **o):
    sticker(s, outline, s.mat('paper', color), rim=rim, depth=depth, lift=.5, bevel=.35, **o)


def wire(s, line, mat, r=.6, z=.6, closed=True):
    """A thin wire or thread laid on a face."""
    s.tube(fillet(line, r) if closed else line, r, mat, z=z, closed=closed, flat=.8,
           caps=None if closed else 'round')


def thread(s, line, color, r, z=.5, twist=True):
    """A twisted silk thread on paper (a ridge every 2 px: tiles of 32, 40 and 44 px hold a whole number)."""
    s.tube(fillet(line, r), r, s.mat('thread', color), z=z, closed=True, flat=.6, caps=None,
           twist=(2, 3) if twist else None)


def stud(s, outline, color, z=.6, rim=1.2, height=1.0, rim_mat=None, bevel=.5):
    """A small enamel stud in an ink (or gilt) setting: the rhombuses and stars of the ornaments."""
    sticker(s, outline, enamel(s, color, rough=.3), rim=rim, depth=.4, lift=height, bevel=bevel, z0=z, rim_mat=rim_mat)


def bead(s, x, y, r, mat, z=.6):
    s.ball(x, y, z, r, mat, .6)


def stitches(s, line, mat, r, z=.4, dip=.7):
    """Dashes of thread that dive into the paper at both ends (the dashed ink outlines of the clouds)."""
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
        s.tube(pts, r, mat, zs=zs, flat=.6, caps='flat', anchor=z + r * .6)


def glyph_stroke(s, pts, w, closed=False, joins=True):
    """A stroke of a white glyph mask: a round cord with round caps and round joins."""
    m = s.mat('matte', WHITE)
    r = w / 2
    s.tube(fillet(pts, r * .25, closed) if len(pts) > 2 else pts, r, m, z=0, closed=closed, flat=.75,
           caps=None if closed else 'round', anchor=r * .75)
    if joins:
        for p in (pts if closed else pts[1:-1]):
            s.ball(p[0], p[1], 0, r, m, .75, anchor=r * .75)


def glyph_fill(s, outline, grow=0.0):
    """A filled part of a glyph mask (a stroked shape when grow > 0: the stroke is grown into the slab)."""
    s.slab(offset(outline, grow) if grow else outline, -.9, .9, s.mat('matte', WHITE), bevel=.75, anchor=.9)


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
    """The leaf of the SVG kit: two quadratic curves through the ends, bulging to both sides."""
    mx, my = (x0 + x1) / 2, (y0 + y1) / 2
    nx, ny = -(y1 - y0), x1 - x0
    ln = math.hypot(nx, ny)
    nx, ny = nx / ln * bulge, ny / ln * bulge
    return quad((x0, y0), (mx + nx, my + ny), (x1, y1))[:-1] + quad((x1, y1), (mx - nx, my - ny), (x0, y0))[:-1]


def corners4(build, w, h, top=0, inset=4):
    """Runs build(transform) for the four corners: the motif is drawn for the top-left one (the SVG kit's
    `corners`: the lower ones mirror about h - inset, the room of the shadow)."""
    build(lambda pts: [(x, y + top) for x, y in pts])
    build(lambda pts: [(w - x, y + top) for x, y in pts])
    build(lambda pts: [(x, h - inset - y) for x, y in pts])
    build(lambda pts: [(w - x, h - inset - y) for x, y in pts])


# ================================================================ buttons and keys
def button(s, w, h, fill, line, line_kind='metal'):
    """120x50: an enamel bar with hexagonal ends in an ink rim, a gilt (or scarlet) wire 4.5 px inside."""
    sticker(s, hexbar(1.5, 1.5, w - 1.5, 45.5, 9), enamel(s, fill))
    wire(s, hexbar(6, 6, w - 6, 41, 6.6), gold(s) if line_kind == 'metal' else enamel(s, line, rough=.3))


add('btn', partial(button, fill=EMERALD, line=GOLD))
add('btn-pressed', partial(button, fill=SCARLET, line=GOLD))
add('btn-chosen', partial(button, fill=PAPER_L, line=SCARLET, line_kind='enamel'))


def key(s, w, h, fill):
    """The square arrow key 48x48."""
    sticker(s, chamfer(1.5, 1.5, 46.5, 44.5, 6), enamel(s, fill))
    wire(s, chamfer(5.5, 5.5, 42.5, 40.5, 4), gold(s), r=.5)


add('key', partial(key, fill=EMERALD))
add('key-pressed', partial(key, fill=SCARLET))


@sprite('field', tiled=True)
def field(s, w, h):
    """The value field between the arrows: paper in an ink rim, a ring of green beads every 4 px (5 per tile)."""
    paper_sheet(s, rrect(1.25, 1.25, 46.75, 46.75, 12), PAPER_L, depth=0)
    m = enamel(s, SPRING, rough=.3)
    for d in dashes(rrect(5, 5, 43, 43, 8.5), 1, 3, closed=True):
        x, y = d[len(d) // 2]
        bead(s, x, y, .75, m, z=.5)


@sprite('banner')
def banner(s, w, h):
    """The ribbon of a changed stat: satin with notched ends, gilt threads with scarlet studs."""
    y0, y1 = 1.5, 50.5
    mid = (y0 + y1) / 2
    sticker(s, [(1.5, y0), (w - 1.5, y0), (w - 9, mid), (w - 1.5, y1), (1.5, y1), (9, mid)], s.mat('satin', EMERALD))
    for y in (7, 45):
        wire(s, [(20, y), (w - 20, y)], gold(s), r=.55, closed=False)
        for x in (16, w - 16):
            stud(s, rhombus(x, y, 2.6), SCARLET, rim=1.6, rim_mat=gold(s))


@sprite('pill')
def pill(s, w, h):
    """The coin balance: dark enamel, a gilt wire."""
    sticker(s, rrect(1.5, 1.5, 94.5, 44.5, 21.5), enamel(s, EMERALD_D))
    wire(s, rrect(5.5, 5.5, 90.5, 40.5, 17.5), gold(s), r=.5)


@sprite('plus')
def plus(s, w, h):
    sticker(s, circle(16, 16, 14.5), enamel(s, EMERALD), rim=2, depth=2)
    wire(s, circle(16, 16, 11.5), gold(s), r=.45)
    m = enamel(s, CREAM, rough=.3)
    for line in ([(16, 9.5), (16, 22.5)], [(9.5, 16), (22.5, 16)]):
        s.tube(line, 1.6, m, z=.9, flat=.6)


@sprite('round')
def round_seal(s, w, h):
    """The dark round seal under the collapse arrow, a ring of gilt dashes."""
    sticker(s, circle(20, 20, 18), enamel(s, EMERALD_D), depth=2)
    for d in dashes(circle(20, 20, 14.5), 2, 2.6, closed=True):
        wire(s, d, gold(s), r=.5, closed=False)


@sprite('gear')
def gear(s, w, h):
    """The settings seal: dark enamel in an ink rim, a ring of gilt dashes, an ivory gear with a ruby."""
    cx, cy = 22, 22
    sticker(s, circle(cx, cy, 20), enamel(s, EMERALD_D), depth=2.5)
    for d in dashes(circle(cx, cy, 16.2), 2, 2.6, closed=True):
        wire(s, d, gold(s), r=.5, closed=False)
    # a ring 5.8..9.4 with eight teeth 5.2 wide out to 11.2 (the SVG kit's)
    pts = []
    half, r_ring, r_tip = 2.6, 9.4, 11.2
    base = math.sqrt(r_ring ** 2 - half ** 2)
    for i in range(8):
        axis = -math.pi / 2 + i * math.tau / 8
        tooth = [(axis + math.atan2(t, r), math.hypot(t, r)) for t, r in ((-half, base), (-half, r_tip), (half, r_tip), (half, base))]
        pts += [(cx + r * math.cos(a), cy + r * math.sin(a)) for a, r in tooth]
        pts += arc(cx, cy, r_ring, tooth[-1][0], axis + math.tau / 8 - math.atan2(half, base))[1:-1]
    s.slab(fillet(pts, .8), -.2, 1.6, enamel(s, CREAM, rough=.3), bevel=.6, holes=[circle(cx, cy, 5.8)])
    s.ball(cx, cy, .9, 2.1, enamel(s, SCARLET, coat=1), .5)


# ================================================================ switches
def switch(s, w, h, on):
    """The sound switch, 60x34: an enamel track and an ivory knob with a rhombus."""
    sticker(s, rrect(1.5, 1.5, 58.5, 29.5, 14), enamel(s, EMERALD if on else OFF_TRACK))
    wire(s, rrect(5, 5, 55, 26, 10.5), enamel(s, SPRING) if on else gold(s), r=.45)
    x = 41 if on else 17
    s.slab(circle(x, 15.5, 11.6), -1.6, 1.4, ink(s), bevel=.6)
    s.slab(circle(x, 15.5, 9.6), -1, 2.0, enamel(s, PAPER_L, rough=.25), bevel=1.6)
    s.slab(rhombus(x, 15.5, 3.4), 1.6, 2.4, enamel(s, SCARLET if on else '#b9a99a'), bevel=.35)


add('switch-on', partial(switch, on=True))
add('switch-off', partial(switch, on=False))


# ================================================================ white glyph masks (the game tints them)
SPEAKER = [(4, 12), (9, 12), (16, 6), (16, 26), (9, 20), (4, 20)]


@sprite('chevron', mask=True)
def chevron(s, w, h):
    glyph_stroke(s, [(4, 3), (12.5, 10), (4, 17)], 3.6)


@sprite('chevron-down', mask=True)
def chevron_down(s, w, h):
    glyph_stroke(s, [(3, 3), (11, 11), (19, 3)], 4)


@sprite('sound', mask=True)
def sound(s, w, h):
    glyph_fill(s, SPEAKER, .8)
    glyph_stroke(s, quad((20.5, 11.5), (23.5, 16), (20.5, 20.5)), 2.6, joins=False)
    glyph_stroke(s, quad((24.5, 7.5), (30, 16), (24.5, 24.5)), 2.6, joins=False)


@sprite('sound-off', mask=True)
def sound_off(s, w, h):
    glyph_fill(s, SPEAKER, .8)
    glyph_stroke(s, [(20.5, 12), (28.5, 20)], 2.6)
    glyph_stroke(s, [(28.5, 12), (20.5, 20)], 2.6)


@sprite('home', mask=True)
def home(s, w, h):
    """A yurt: "back to the menu"."""
    glyph_stroke(s, quad((3.5, 16), (16, 3), (28.5, 16))[:-1] + [(28.5, 16), (28.5, 27.5), (3.5, 27.5)], 2.6, closed=True, joins=False)
    for p in ((28.5, 16), (28.5, 27.5), (3.5, 27.5), (3.5, 16)):
        s.ball(p[0], p[1], 0, 1.3, s.mat('matte', WHITE), .75, anchor=1.3 * .75)
    glyph_stroke(s, [(16, 6), (16, 3.5)], 2.6)
    glyph_stroke(s, [(3.5, 16), (28.5, 16)], 2.6)
    glyph_fill(s, [(12.5, 27.5), (12.5, 21)] + quad((12.5, 21), (16, 18), (19.5, 21))[1:-1] + [(19.5, 21), (19.5, 27.5)])


@sprite('restart', mask=True)
def restart(s, w, h):
    """A circular arrow: "from the beginning"."""
    glyph_stroke(s, svg_arc((24.5, 10.5), (26, 17.5), 10, 1, 0), 2.8, joins=False)
    glyph_stroke(s, [(26.5, 3.5), (26.5, 11.5), (18.5, 11.5)], 2.8)


@sprite('exit', mask=True)
def exit_door(s, w, h):
    """A door with an arrow leaving it: "quit the game"."""
    glyph_stroke(s, [(15, 5), (6.5, 5), (6.5, 27), (15, 27)], 2.6)
    glyph_stroke(s, [(12, 16), (27, 16)], 2.8)
    glyph_stroke(s, [(22, 10.5), (27.5, 16), (22, 21.5)], 2.8)


@sprite('download', mask=True)
def download(s, w, h):
    """An arrow down into a tray: "update the game"."""
    glyph_stroke(s, [(16, 4), (16, 19)], 2.8)
    glyph_stroke(s, [(9.5, 13), (16, 19.5), (22.5, 13)], 2.8)
    glyph_stroke(s, [(5, 20), (5, 26.5), (27, 26.5), (27, 20)], 2.6)


# ================================================================ the menu: card, frame, star
@sprite('card', tiled=True)
def card(s, w, h):
    """The story card and the settings toast: paper on an ink board, a twisted scarlet thread, studs in the corners."""
    paper_sheet(s, chamfer(1.5, 1.5, 94.5, 90.5, 8), PAPER, depth=4)
    thread(s, chamfer(7, 7, 89, 85, 5), SCARLET, .7)
    for x, y in ((13, 13), (83, 13), (13, 79), (83, 79)):
        stud(s, rhombus(x, y, 3), SCARLET, z=.5)


@sprite('frame', shear=0)
def frame(s, w, h):
    """The frame over a preview picture: an ink moulding with a gilt fillet inside."""
    s.tube(rrect(1.5, 1.5, 30.5, 30.5, 3), 1.5, ink(s), z=0, closed=True, flat=.8, caps=None)
    s.tube(rrect(4, 4, 28, 28, 1.5), .5, gold(s), z=0, closed=True, flat=.8, caps=None)


@sprite('star', shear=.5)
def star_sprite(s, w, h):
    """The sparkle of a finished story on its card: a scarlet enamel star in an ink rim."""
    sticker(s, sparkle(12, 12, 10.5), enamel(s, SCARLET), rim=1.6, depth=1.2, bevel=.35)


# ================================================================ plates
def plate(s, w, h, fill, line):
    """96x96, slice 32 (tiled: the grain and the threads repeat every 32 px). Paper on an ink board, a double
    thread of `line`, scarlet hooks in the corners."""
    paper_sheet(s, chamfer(2, 2, 94, 90, 10), fill, rim=3.5)
    thread(s, chamfer(6.5, 6.5, 89.5, 85.5, 7), line, 1.0)
    thread(s, chamfer(10, 10, 86, 82, 5), line, .45, twist=False)
    corners4(partial(corner_hook, s), w, h)


def corner_hook(s, place):
    """The SVG kit's hook (a cell 16..33, scaled .72 and tucked into the corner) as scarlet thread and studs."""
    def t(pts):
        return place([(1.5 + x * .72, 1.5 + y * .72) for x, y in pts])
    m = s.mat('thread', SCARLET)
    s.tube(t([(16, 28), (16, 20.5)] + quad((16, 20.5), (16, 16.5), (20, 16.5))[1:] + [(28, 16.5)]), .65, m, z=.6, flat=.6)
    s.tube(t([(21, 28), (21, 24.5)] + quad((21, 24.5), (21, 21.5), (24, 21.5))[1:] + [(29, 21.5)]), .4, m, z=.6, flat=.6)
    for x, y in ((16, 31.5), (31.5, 16.5)):
        sq = 1.7
        s.slab(t([(x - sq, y - sq), (x + sq, y - sq), (x + sq, y + sq), (x - sq, y + sq)]), .3, 1.2, enamel(s, SCARLET), bevel=.35)
    s.slab(t(rhombus(20.5, 20.5, 2.3)), .3, 1.3, enamel(s, SCARLET), bevel=.4)


for _name, _fill, _line in (('plate', PAPER, SCARLET), ('plate-mint', MINT, EMERALD), ('plate-rose', ROSE_PAPER, SCARLET)):
    add(_name, partial(plate, fill=_fill, line=_line), tiled=True)


def plate_tail(s, w, h, mirror):
    """The plate with its speech horn in one outline, 32 px of headroom for the horn. 204x128 (the SVG kit's is
    180 wide: its 8 px centre would repeat the grain every 8 px); slice 64 on top, 32 on the left, 140 on the
    right (the horn lives there), 32 at the bottom; `mirror` the other way round."""
    top = 32
    x0, x1, y0, y1, cut = 2, w - 2, 2 + top, 90 + top, 10
    ox = w - 132
    left, right = ox + 26, ox + 58
    apex = (ox + 3, 2)
    pts = chamfer(x0, y0, x1, y1, cut)
    horn = bezier((left, y0), (ox + 24, 26), (ox + 8, 19), apex) + bezier(apex, (ox + 17, 20), (ox + 34, y0), (right + 4, y0))[1:]
    outline = [pts[0]] + horn + pts[1:]
    if mirror:
        outline = mirror_x(outline, w)
    k = outline.index(mirror_x([apex], w)[0] if mirror else apex)

    def piping(r, mat, z):
        """Open at the apex of the horn and closed there by a ball: a round join, no miter spike."""
        line = outline[k:] + outline[:k + 1]
        s.tube(fillet(line, r, closed=False), r, mat, z=z, flat=.7, caps='round')

    paper_sheet(s, outline, PAPER, rim=3.5, piping=piping)
    thread(s, chamfer(6.5, 38.5, w - 6.5, 117.5, 7), SCARLET, 1.0)
    thread(s, chamfer(10, 42, w - 10, 114, 5), SCARLET, .45, twist=False)
    corners4(partial(corner_hook, s), w, h, top=top)


add('plate-tail-r', partial(plate_tail, mirror=False), size=(204, 128), tiled=True)
add('plate-tail-l', partial(plate_tail, mirror=True), size=(204, 128), tiled=True)


# ================================================================ clouds (tiled edges)
@sprite('thought', tiled=True, shear=.3)
def thought(s, w, h):
    """The dashed "drop" cloud: a paper outline with a small wave (one period per 40 px tile), stitched with ink
    thread (4 stitches per tile), scarlet stars in the corners."""
    cell, radius, inset, amp = 40, 34, 5, 1.5
    lo, hi = inset, 120 - inset
    top = wave(cell, 80, lo, amp)
    bottom = [(x, hi + amp * math.sin(math.tau * (x - cell) / 40)) for x, _ in wave(cell, 80, hi, amp)][::-1]
    left = [(lo + amp * math.sin(math.tau * (y - cell) / 40), y) for _, y in wave(cell, 80, lo, amp, vertical=True)][::-1]
    right = [(hi + amp * math.sin(math.tau * (y - cell) / 40), y) for _, y in wave(cell, 80, hi, amp, vertical=True)]
    tl = arc(lo + radius, lo + radius, radius, math.pi, 1.5 * math.pi)
    tr = arc(hi - radius, lo + radius, radius, 1.5 * math.pi, 2 * math.pi)
    br = arc(hi - radius, hi - radius, radius, 0, .5 * math.pi)
    bl = arc(lo + radius, hi - radius, radius, .5 * math.pi, math.pi)
    s.slab(tl + top + tr + right + br + bottom + bl + left, -.5, .3, s.mat('paper', PAPER), bevel=.3)
    m = s.mat('thread', INK_LACQUER, sheen=.4)
    for pts in (top, bottom[::-1], left[::-1], right):
        stitches(s, dashes(resample(pts, .3), 6, 4, total=40), m, 1.6, z=.3)
    for pts in (tl, tr, br, bl):
        stitches(s, dashes(pts, 6, 4, total=50), m, 1.6, z=.3)
    for cx, cy, r in ((106, 14, 13), (14, 96, 9.5), (24, 110, 6.5)):
        stud(s, sparkle(cx, cy, r, .18), SCARLET, z=.3, rim=1.5, bevel=.15)


@sprite('thought-cloud', tiled=True, shear=.3)
def thought_cloud(s, w, h):
    """The puffy cloud: paper scallops (arcs of 20 px, 2 per 40 px tile) stitched with ink thread, stars inside."""
    b, e = 8, 112
    chain = [(b, 40), (b, 20), (20, b), (40, b), (60, b), (80, b), (100, b), (e, 20), (e, 40), (e, 60), (e, 80),
             (e, 100), (100, e), (80, e), (60, e), (40, e), (20, e), (b, 100), (b, 80), (b, 60), (b, 40)]
    radii = [12 if i in (1, 6, 11, 16) else 11 for i in range(len(chain) - 1)]
    arcs = [scallop(chain[i], chain[i + 1], radii[i]) for i in range(len(chain) - 1)]
    s.slab([p for a in arcs for p in a[:-1]], -.5, .3, s.mat('paper', PAPER), bevel=.3)
    m = s.mat('thread', INK_LACQUER, sheen=.4)
    for a in arcs:
        stitches(s, dashes(a, 6.5, 1.5, total=32), m, 1.5, z=.3)
    # (the small ones are 4.2, not 5: with the rim and the wall a ray of 5 reaches into the edge tile and repeats)
    for cx, cy, r, rim in ((102, 24, 9, 2), (86, 34, 4.2, 1.6), (18, 96, 9, 2), (34, 86, 4.2, 1.6)):
        stud(s, sparkle(cx, cy, r, .18), SCARLET, z=.3, rim=rim, bevel=.15)


@sprite('info', tiled=True, shear=.3)
def info(s, w, h):
    """The info cloud: paper scallops (2 per 32 px tile) in an ink piping, emerald leaves and a rhombus in the
    corners."""
    base, cell, size, small, big = 6, 32, 96, 10, 20
    e = size - base
    chain = [((cell, base), None), ((48, base), small), ((64, base), small), ((e, cell), big), ((e, 48), small),
             ((e, 64), small), ((64, e), big), ((48, e), small), ((32, e), small), ((base, 64), big),
             ((base, 48), small), ((base, 32), small), ((cell, base), big)]
    arcs = [scallop(chain[i][0], chain[i + 1][0], chain[i + 1][1]) for i in range(len(chain) - 1)]
    outline = [p for a in arcs for p in a[:-1]]
    paper_sheet(s, outline, PAPER, depth=0)
    for a in arcs:                                      # the cusps between the scallops: round joins
        s.ball(a[0][0], a[0][1], .5, 1.25, ink(s), .7)
    leaf_mat, rh = enamel(s, EMERALD, rough=.35), enamel(s, SCARLET)

    def motif(place):
        # the tips end at 31, not 32: a part that touches the edge of the corner cell repeats along the side
        s.slab(place(leaf(14, 20, 15, 31, 3)), .3, 1.0, leaf_mat, bevel=.4)
        s.slab(place(leaf(20, 14, 31, 15, -3)), .3, 1.0, leaf_mat, bevel=.4)
        s.slab(place(rhombus(17, 17, 3.2)), .3, 1.3, rh, bevel=.45)
    corners4(motif, size, size, inset=0)


# ================================================================ tags and their ornaments
def tag(s, w, h, fill):
    """The name tag on the top edge of a plate: an enamel plaque with chamfered corners in an ink rim."""
    sticker(s, chamfer(2.5, 2, 77.5, 31.5, 4), enamel(s, fill), rim=3)


add('label', partial(tag, fill=EMERALD))
add('label-rose', partial(tag, fill=ROSE))


@sprite('label-orn', shear=.4)
def tag_ornament(s, w, h):
    sticker(s, rhombus(5, 5, 3.8), enamel(s, SCARLET), rim=1.2, depth=.5, rim_mat=gold(s), bevel=.5)


@sprite('gem', shear=.5)
def gem(s, w, h):
    """The gem at the end of the "Факт" tag: a scarlet stone in an ink setting with an ink inlay."""
    sticker(s, rhombus(14, 14, 11.5), enamel(s, SCARLET, coat=1, rough=.25), rim=2.2, depth=1.5, bevel=2)
    s.slab(rhombus(14, 14, 6.2), .5, 1.4, ink(s), bevel=.5)
    s.slab(rhombus(14, 14, 3.4), 1.0, 2.0, enamel(s, SCARLET, coat=1, rough=.2), bevel=.6)


@sprite('orn', shear=.5)
def orn(s, w, h):
    """The solar rhombus with leaves at the left end of a button."""
    for x0, x1 in ((3, 8.5), (15.5, 21)):
        s.slab(leaf(x0, 12, x1, 12, 2.4), -.6, .6, enamel(s, SPRING, rough=.35), bevel=.45)
    sticker(s, rhombus(12, 12, 5.6), enamel(s, SCARLET), rim=1.2, depth=.6, rim_mat=gold(s), bevel=.6)
    s.slab(rhombus(12, 12, 2), .8, 1.6, enamel(s, CREAM, rough=.3), bevel=.4)
    for y in (3.2, 20.8):
        bead(s, 12, y, 1.1, gold(s), z=0)


@sprite('bubble', shear=.5)
def bubble(s, w, h):
    """A thought bubble: a paper disc in an ink rim."""
    paper_sheet(s, circle(12, 12, 10), PAPER, depth=1.2)


@sprite('badge')
def badge(s, w, h):
    """The "!" seal of the info cloud: a scarlet wax stamp of 12 rays with an ivory "!"."""
    sticker(s, star(22, 22, 12, 20, 17), enamel(s, SCARLET, rough=.45, coat=.3), depth=2, bevel=1.4)
    sticker(s, [(20, 11.5), (24, 11.5), (23, 25), (21, 25)], enamel(s, CREAM, rough=.3), rim=1, depth=0, lift=.6,
            bevel=.5, z0=.6)
    sticker(s, circle(22, 30.5, 2.6), enamel(s, CREAM, rough=.3), rim=1, depth=0, lift=.6, bevel=.6, z0=.6)


@sprite('knot', shear=.3)
def knot(s, w, h):
    """The big knot over the corner of a plate: two rhombus cords interlaced (over, then under, at every crossing:
    the heights are real), a small one inside, ivory gems in the middles, two sparkles."""
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
    over_a = ab[0::2]                          # the SVG kit: every second crossing A over B
    under_a = ab[1::2]
    cb = crossings(c, b)                       # the small rhombus is over the big one of B

    def heights(pts, ups, downs):
        out = []
        for p in pts:
            z = 0.0
            for q in ups:
                z += 1.4 * math.exp(-(dist(p, q) / 4.5) ** 2)
            for q in downs:
                z -= 1.4 * math.exp(-(dist(p, q) / 4.5) ** 2)
            out.append(z)
        return out

    cord, edge = enamel(s, SCARLET, rough=.35), ink(s)
    for pts, ups, downs in ((a, over_a, under_a), (b, under_a + cb, over_a), (c, cb, [])):
        line = resample(pts + pts[:1], .3)
        zs = heights(line, ups, downs)
        s.tube(line, 2.6, edge, zs=[z - .9 for z in zs], flat=.6, caps=None, anchor=1.6)
        s.tube(line, 1.2, cord, zs=zs, flat=.8, caps=None, anchor=1.6)
    for cx, cy, r, rim in ((28, 32, 3.6, 1.4), (42, 26, 2.6, 1.2)):
        stud(s, rhombus(cx, cy, r), PAPER, z=1.2, rim=rim)
    for cx, cy, r in ((58, 7, 4.5), (7, 53, 3.2)):
        s.slab(sparkle(cx, cy, r), -.4, .7, enamel(s, SCARLET), bevel=.4)


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
@sprite('coin')
def coin(s, w, h):
    """The coin: bronze in an ink rim, a struck ring of dots and a tulip in relief."""
    face, deep, light = s.mat('bronze', BRONZE), s.mat('bronze', BRONZE_D), s.mat('bronze', BRONZE_L)
    sticker(s, circle(24, 24, 21.5), face, depth=1.5, bevel=1.4)
    wire(s, circle(24, 24, 15), deep, r=.7)
    for i in range(20):
        a = i * math.tau / 20
        bead(s, 24 + 17.6 * math.cos(a), 24 + 17.6 * math.sin(a), .95, deep)
    tulip = (bezier((24, 13), (27, 16), (28.5, 19), (28, 23)) + bezier((28, 23), (30, 21), (32, 18.5), (33, 16))[1:]
             + bezier((33, 16), (34, 22), (31, 28.5), (24, 30))[1:] + bezier((24, 30), (17, 28.5), (14, 22), (15, 16))[1:]
             + bezier((15, 16), (16, 18.5), (18, 21), (20, 23))[1:] + bezier((20, 23), (19.5, 19), (21, 16), (24, 13))[1:-1])
    s.slab(tulip, .2, 1.4, deep, bevel=.6)
    s.tube([(24, 30), (24, 36)], .9, deep, z=.6, flat=.6)
    s.tube(bezier((24, 34), (21, 34), (19, 32.5), (18, 31)), .9, deep, z=.6, flat=.6)
    s.tube(bezier((24, 34), (27, 34), (29, 32.5), (30, 31)), .9, deep, z=.6, flat=.6)
    s.tube(bezier((24, 15.5), (25.5, 17.5), (26, 20), (25.6, 23)), .6, light, z=1.4, flat=.6)
    s.tube(svg_arc((9.5, 18), (18, 9.5), 15.5, 0, 1), 1.0, light, z=.6, flat=.6)


@sprite('timer', shear=.5)
def timer_ring(s, w, h):
    """The ring of the timer over the pie the game draws: an ink ring, a scarlet enamel ring, ink ticks, a flame."""
    s.tube(circle(30, 30, 26), 2, ink(s), z=0, closed=True, flat=.7, caps=None)
    s.tube(circle(30, 30, 23.4), 1.2, enamel(s, SCARLET), z=0, closed=True, flat=.7, caps=None)
    for i in range(12):
        a = i * math.tau / 12
        s.tube([(30 + 22.2 * math.cos(a), 30 + 22.2 * math.sin(a)), (30 + 24.6 * math.cos(a), 30 + 24.6 * math.sin(a))],
               .65, ink(s), z=.6, flat=.7)
    flame = (bezier((30, .8), (33.5, 4), (34, 7), (32.8, 9.3)) + bezier((32.8, 9.3), (32, 10.8), (30.8, 11.4), (30, 11.4))[1:]
             + bezier((30, 11.4), (28.4, 11.4), (26.8, 10), (27, 7.8))[1:] + bezier((27, 7.8), (27.2, 6.4), (28.2, 5.6), (28.6, 4.4))[1:]
             + bezier((28.6, 4.4), (29.6, 5.6), (29.8, 6.6), (29.6, 7.6))[1:] + bezier((29.6, 7.6), (31, 6.6), (31.4, 4), (30, .8))[1:-1])
    sticker(s, flame, enamel(s, SCARLET), rim=1.3, depth=.8, bevel=.6, z0=.4)


@sprite('stat-steppe', shear=.5)
def stat_steppe(s, w, h):
    """The icon of the stat "Память степи": a dark enamel tile in a gilt rim, feather grass of gilt wire, a sun."""
    sticker(s, rrect(1.25, 1.25, 32.75, 32.75, 6), enamel(s, EMERALD_D), rim=2, depth=1, rim_mat=gold(s))
    s.ball(25.5, 9, .6, 3.2, enamel(s, SCARLET), .45)
    for x0, qx, qy, x1, y1 in ((16, 14, 18, 8, 10), (17, 17, 16, 15, 6.5), (18, 20, 15, 22, 7), (19, 22, 19, 27, 12),
                               (15, 12, 22, 7, 20)):
        s.tube(quad((x0, 28), (qx, qy), (x1, y1)), 1, gold(s), z=.6, flat=.7)
    s.tube([(6, 28.5), (28, 28.5)], 1, gold(s), z=.6, flat=.7)


# ================================================================ the title
@sprite('title-brackets', halo=CREAM, shear=.6)
def brackets(s, w, h):
    """The brackets of the title, the upper and the lower in one file: a gilt cord curling at both ends, a scarlet
    thread under it, scarlet studs and gilt beads. Slice 30 / 50, no centre."""
    curl = ([(50, 14), (20, 14)] + quad((20, 14), (10, 14), (9, 21))[1:] + quad((9, 21), (8.5, 27), (14, 27))[1:]
            + quad((14, 27), (18.5, 27), (18, 22.5))[1:] + quad((18, 22.5), (17.5, 19.5), (14.5, 20.5))[1:])
    cord = curl[::-1][:-1] + [(w - x, y) for x, y in curl][1:]   # from the left curl through to the right one
    g, red = gold(s), s.mat('thread', SCARLET)
    for flip in (False, True):
        def v(pts):
            return [(x, h - y) for x, y in pts] if flip else pts
        s.tube(v(fillet(cord, .3, closed=False)), 1.5, g, z=.6, flat=.7)
        s.tube(v([(30, 20), (w - 30, 20)]), .75, red, z=.2, flat=.7)
        for x in (30, w - 30):
            stud(s, v(rhombus(x, 14, 4)), SCARLET, z=.6, rim=1.2, rim_mat=g)
        for x in (40, w - 40):
            (bx, by), = v([(x, 14)])
            bead(s, bx, by, 1.6, g, z=.6)


@sprite('title-orn', shear=.5)
def title_ornament(s, w, h):
    """The centre of a bracket: the solar rhombus with gilt rays."""
    for i in range(8):
        a = i * math.tau / 8 + math.tau / 16
        s.tube([(32 + 9 * math.cos(a), 16 + 9 * math.sin(a)), (32 + 14 * math.cos(a), 16 + 14 * math.sin(a))], .8,
               gold(s), z=0, flat=.7)
    sticker(s, rhombus(32, 16, 9), enamel(s, SCARLET), rim=2, depth=1.2, bevel=.8)
    s.slab(rhombus(32, 16, 5), .6, 1.5, enamel(s, CREAM, rough=.3), bevel=.5)
    s.slab(rhombus(32, 16, 2.2), 1.2, 2.0, enamel(s, SCARLET), bevel=.4)
    for x in (14, 50):
        s.slab(rhombus(x, 16, 2.4), -.4, .8, gold(s), bevel=.5)
