"""The story kit of «По хребтам Сихотэ-Алиня»: everything of the story screens drawn as the things of a surveyor's field
notebook instead of the steppe embroidery of the main kit. Not only another decoration on the same shapes: the
outlines are different too.

    plate (fact)       map sheet with cut corners, double neatline with ticks, registration crosses
    plate-mint         tracing paper with rounded corners and contour arcs
    plate-rose         kraft sheet with a folded corner (dog-ear), a wax seal
    plate-tail-r/l     the map sheet with an angular pennant for the horn (a north arrow: one half in shade), a brass pin
    thought            tracing paper with a wobbly coastline edge, contour lines inside
    thought-cloud      tracing paper with a mountain-ridge edge (a saw of peaks), contour line inside
    label, label-rose  a tag with pointed ends (a luggage tag), ruler ticks
    btn, -pressed, -chosen  a ticket with stepped corners, a brass line with ticks, rivets
    banner             a surveyor's tape with an arrow end, brass ticks
    info               a perforated sheet (bites along the edge, like a torn-off stub), crosses in the corners
    badge, gem, stat-steppe, knot, orn, bubble, label-orn, title-brackets, title-orn: brass, a compass, ridges

Every sprite has the size and the 9-slice borders of the sprite of the main kit it replaces (the layout of the game
does not change); its name is `sa-` + that name. The game takes `sa-<name>` while a chapter of the story is read and
`<name>` everywhere else (Art.StoryKit). Registered into sprites3d.KIT, rendered by render.py like every sprite."""
import math
from functools import partial

from kit.geometry import wave
from kit.palette import CREAM as TITLE_CREAM
from kit.registry import Slice
from shapes import arc, chamfer, circle, fillet, mirror_x, offset, rhombus, rrect, sparkle, stroke
from sprites3d import (add, corners4, edge, gold, gold_star, granules, lattice, leather, plaque, sheet, straight,
                       thread)

PREFIX = 'sa-'

NAVY = '#24476b'           # Prussian-blue leather of the buttons and tags
NAVY_D = '#16304b'
WAX = '#a8322c'            # vermilion: sealing wax, the pressed button
PAPER = '#f3ead3'          # map paper
KRAFT = '#dcc7a1'          # kraft paper of the hints
KRAFT_D = '#c7ad80'
TRACE = '#dde6e6'          # tracing paper of the thoughts
INK = '#1c2a3a'            # the ink of the neatlines
INK_RED = '#9a3a2e'
INK_BLUE = '#27506b'
PEARL = '#f1e8d6'


def sa(name, build, **o):
    add(PREFIX + name, build, **o)


def rot(pts, cx, cy, a):
    c, s_ = math.cos(a), math.sin(a)
    return [(cx + (x - cx) * c - (y - cy) * s_, cy + (x - cx) * s_ + (y - cy) * c) for x, y in pts]


# ================================================================ plates: map paper in a neatline
def registration(s, place, ink, z=.55):
    """A registration cross in a ring, as in the corner of a map sheet (it lives in the corner cell, 14..24 px)."""
    s.tube(place(circle(19, 19, 3.4)), .26, thread(s, ink), z=z, closed=True, flat=.7, caps=None)
    s.tubes([place([(19 - 6.2, 19), (19 + 6.2, 19)]), place([(19, 19 - 6.2), (19, 19 + 6.2)])], .22, thread(s, ink), z=z)


def contours(s, place, ink, z=.5):
    """Two quarter contour lines of a hill in the corner cell."""
    for r in (6.5, 10.5):
        s.tube(place(arc(25, 25, r, math.pi, 1.5 * math.pi)), .2, thread(s, ink), z=z, flat=.6)


def edge_ticks(s, box, ink, step=4, margin=13, a=5.2, b=7.6, z=.5):
    """The ticks between the two lines of the neatline, on the lattice of the tile (the frame of a map)."""
    x0, y0, x1, y1 = box
    bd = s.borders
    paths = []
    for x in lattice(x0 + margin, x1 - margin, bd.left, step):
        paths += [[(x, y0 + a), (x, y0 + b)], [(x, y1 - a), (x, y1 - b)]]
    for y in lattice(y0 + margin, y1 - margin, bd.top, step):
        paths += [[(x0 + a, y), (x0 + b, y)], [(x1 - a, y), (x1 - b, y)]]
    s.tubes(paths, .17, thread(s, ink), z=z)


def neatline(s, box, ink):
    s.tubes(straight(s, box, 5.2, 13), .34, thread(s, ink), z=.55)
    s.tubes(straight(s, box, 7.6, 13), .2, thread(s, ink), z=.5)
    edge_ticks(s, box, ink)


def dog_ear(box, cut=16, small=3):
    """A sheet with its top-right corner folded: the outline loses the corner, the others are cut a little."""
    x0, y0, x1, y1 = box
    return [(x0, y0 + small), (x0 + small, y0), (x1 - cut, y0), (x1, y0 + cut), (x1, y1 - small), (x1 - small, y1),
            (x0 + small, y1), (x0, y1 - small)]


def plate(s, w, h, fill, ink, kind):
    """96x96, tile 32. 'paper': a sheet with cut corners; 'trace': tracing paper with rounded corners and contour
    arcs; 'seal': kraft paper with a folded corner and a wax seal. The text keeps 30 px from the edges, the
    marks end 25 px in."""
    box = (2, 2, 94, 90)
    outline = {'paper': chamfer(*box, 11), 'trace': rrect(*box, 17), 'seal': dog_ear(box)}[kind]
    sheet(s, outline, fill, binding=1.5)
    neatline(s, box, ink)
    if kind == 'trace':
        corners4(lambda place: contours(s, place, ink), w, h)
    else:
        corners4(lambda place: registration(s, place, ink), w, h)
    if kind == 'seal':
        flap = [(78, 2), (94, 18), (78, 18)]
        s.slab(flap, .35, 1.0, s.mat('parchment', KRAFT_D), bevel=.3)
        s.tubes([[(78, 2), (94, 18)]], .25, thread(s, ink), z=1.0)
        s.slab(circle(18.5, 18.5, 5.6), -.3, 1.5, s.mat('wax', WAX), bevel=1.2)
        s.slab(sparkle(18.5, 18.5, 3.3, .3), 1.2, 1.9, s.mat('wax', '#7d241f'), bevel=.3)


for _name, _fill, _ink, _kind in (('plate', PAPER, INK, 'paper'), ('plate-mint', TRACE, INK_BLUE, 'trace'),
                                  ('plate-rose', KRAFT, INK_RED, 'seal')):
    sa(_name, partial(plate, fill=_fill, ink=_ink, kind=_kind), size=(96, 96), slice=Slice.uniform(32))


def plate_tail(s, w, h, mirror):
    """The speech plate: a map sheet with cut corners and an angular pennant for the horn (the north arrow of a map:
    a triangle, its far half in shade), 32 px of headroom for it, a brass pin on it. The same 204x128, the same
    borders as the main kit's plate-tail (the horn lives in the wide border)."""
    top = 32
    box = (2, 2 + top, w - 2, 90 + top)
    y0 = box[1]
    ox = w - 132
    xa, xb = ox + 22, ox + 74                        # the base of the pennant on the top edge
    apex = (ox + 46, 4)
    pts = chamfer(*box, 11)
    outline = [pts[0]] + [(xa, y0), apex, (xb, y0)] + pts[1:]
    shade = [apex, (xb, y0), (apex[0], y0)]
    pin = (apex[0], 12)
    if mirror:
        outline, shade, pin = mirror_x(outline, w), mirror_x(shade, w), mirror_x([pin], w)[0]
    tip = mirror_x([apex], w)[0] if mirror else apex
    k = outline.index(tip)

    def piping(r, mat, z):
        line = outline[k:] + outline[:k + 1]
        s.tube(fillet(line, r, closed=False), r, mat, z=z, flat=.65, caps='round')

    sheet(s, outline, PAPER, binding=1.5, piping=piping)
    s.slab(shade, .35, .9, s.mat('parchment', KRAFT), bevel=.3)
    s.tubes([[tip, (tip[0], y0 - .5)]], .25, thread(s, INK), z=.95)
    granules(s, [pin], r=1.8, z=1.0)                 # the brass pin that holds the pennant
    neatline(s, box, INK)
    corners4(lambda place: registration(s, place, INK), w, h, top=top)


sa('plate-tail-r', partial(plate_tail, mirror=False), size=(204, 128), slice=Slice(64, 32, 140, 32))
sa('plate-tail-l', partial(plate_tail, mirror=True), size=(204, 128), slice=Slice(64, 140, 32, 32))


# ================================================================ thoughts: tracing paper
def thought(s, w, h):
    """120x120, tile 40: tracing paper with a coastline edge: a wave of 3.4 px per 40 px tile, a thin ink binding, two
    contour lines inside it that follow the wave, compass stars in the corners."""
    cell, radius, inset, amp = 40, 30, 5, 3.4
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
    s.slab(outline, -.6, .8, s.mat('parchment', TRACE, grain=.6), bevel=2.6)
    s.tube(fillet(outline, 1.0), 1.0, thread(s, INK), z=.7, closed=True, flat=.65, caps=None)
    s.tube(offset(outline, -4.6), .3, thread(s, INK_BLUE), z=.75, closed=True, flat=.7, caps=None)
    s.tube(offset(outline, -8.0), .2, thread(s, INK_BLUE), z=.72, closed=True, flat=.7, caps=None)
    for cx, cy, r in ((106, 14, 12.5), (14, 96, 9), (24, 110, 6)):
        gold_star(s, cx, cy, r, z=.8)


def ridge_outline():
    """A saw of peaks round a square: peaks (outward) at x = 10 mod 20 (two per 40 px tile), valleys between."""
    def peak(v):
        return (v // 10) % 2 == 1
    top = [(x, 6 if peak(x) else 14) for x in range(20, 101, 10)]
    right = [(114 if peak(y) else 106, y) for y in range(20, 101, 10)]
    bottom = [(x, 114 if peak(x) else 106) for x in range(100, 19, -10)]
    left = [(6 if peak(y) else 14, y) for y in range(100, 19, -10)]
    return top + right + bottom + left


def thought_cloud(s, w, h):
    """120x120, tile 40: tracing paper with a mountain-ridge edge, a thin ink binding and one contour line inside;
    brass pins in the valleys, compass stars in two corners."""
    outline = ridge_outline()
    s.slab(outline, -.6, .8, s.mat('parchment', TRACE, grain=.6), bevel=1.6)
    s.tube(fillet(outline, 1.1), 1.1, thread(s, INK), z=.7, closed=True, flat=.65, caps=None)
    s.tube(offset(outline, -4.2), .3, thread(s, INK_BLUE), z=.75, closed=True, flat=.7, caps=None)
    valleys = [(x, 22) for x in (50, 70)] + [(x, 98) for x in (50, 70)] + [(22, y) for y in (50, 70)] + [(98, y) for y in (50, 70)]
    granules(s, valleys, r=.8, z=.8)
    for cx, cy, r in ((100, 24, 8.5), (86, 34, 4.2), (20, 96, 8.5), (34, 86, 4.2)):
        gold_star(s, cx, cy, r, z=.8)


sa('thought', thought, size=(120, 120), slice=Slice.uniform(40), shear=.3)
sa('thought-cloud', thought_cloud, size=(120, 120), slice=Slice.uniform(40), shear=.3)


# ================================================================ tags and buttons: blue leather, brass, ruler ticks
def ticks(s, box, inset, step, margin, length, sides='tb', z=.95, r=.2):
    """Brass ruler ticks along the long sides of a plaque on the lattice of the tile, `length` px deep."""
    x0, y0, x1, y1 = box
    bd = s.borders
    paths = []
    for x in lattice(x0 + margin, x1 - margin, bd.left, step):
        if 't' in sides:
            paths.append([(x, y0 + inset), (x, y0 + inset + length)])
        if 'b' in sides:
            paths.append([(x, y1 - inset), (x, y1 - inset - length)])
    s.tubes(paths, r, gold(s), z=z)


def tag(s, w, h, face):
    """The name tag: 80x36, a luggage tag with pointed ends in a brass rim, ruler ticks along the long sides (the
    game's marks sit 12 px from the ends)."""
    box = (2, 2, 78, 31.5)
    x0, y0, x1, y1 = box
    mid = (y0 + y1) / 2
    body = [(x0 + 9, y0), (x1 - 9, y0), (x1, mid), (x1 - 9, y1), (x0 + 9, y1), (x0, mid)]
    plaque(s, body, leather(s, face), bevel=1.8)
    ticks(s, box, 3.6, 4, 12, 1.8, z=1.0)


sa('label', partial(tag, face=NAVY), size=(80, 36), slice=Slice(15, 14, 14, 17))
sa('label-rose', partial(tag, face=KRAFT), size=(80, 36), slice=Slice(15, 14, 14, 17))


def stair(x0, y0, x1, y1, k):
    """A rectangle whose corners are two steps of k px (the corner of a map grid), clockwise."""
    corner = [(0, 2 * k), (k, 2 * k), (k, k), (2 * k, k), (2 * k, 0)]
    return ([(x0 + a, y0 + b) for a, b in corner]
            + [(x1 - a, y0 + b) for a, b in reversed(corner)]
            + [(x1 - a, y1 - b) for a, b in corner]
            + [(x0 + a, y1 - b) for a, b in reversed(corner)])


def button(s, w, h, face):
    """The option button: 120x50, tile 76 x 6. A ticket with stepped corners in a brass rim, a tooled brass line with
    ruler ticks inside it, a brass rivet at every corner step. Clear of the game's icons (12 px from the ends)."""
    box = (1.5, 1.5, w - 1.5, 45.5)
    plaque(s, stair(*box, 4), leather(s, face))
    s.tubes(straight(s, box, 3.6, 11), .3, gold(s), z=.95)
    ticks(s, box, 5.6, 4, 12, 1.6)
    corners4(lambda place: granules(s, place([(11.4, 5.6)]), r=1.0, z=1.0), w, 47, inset=0)


for _name, _face in (('btn', NAVY), ('btn-pressed', WAX), ('btn-chosen', PAPER)):
    sa(_name, partial(button, face=_face), size=(120, 50), slice=Slice(16, 22, 22, 20))


# ================================================================ the stat ribbon: a surveyor's tape
def banner(s, w, h):
    """The ribbon of a changed stat: 120x56, a navy tape with an arrow end in a brass rim, brass lines along it with
    ruler ticks (long every 12, short every 3). Its height never changes; the left end is clear of the stat icon."""
    y0, y1 = 1.5, 50.5
    mid = (y0 + y1) / 2
    body = [(1.5, y0), (w - 12, y0), (w - 1.5, mid), (w - 12, y1), (1.5, y1)]
    plaque(s, body, s.mat('velvet', NAVY), bevel=1.6)
    for y in (6.2, 44.8):
        s.tubes([[(16, y), (w - 20, y)]], .32, gold(s), z=.95)
    bd = s.borders
    short, long_ = [], []
    for x in lattice(20, w - 20, bd.left, 3):
        k = round((x - bd.left - 1.5) / 3)
        (long_ if k % 4 == 0 else short).append(x)
    for xs, depth in ((short, 2.0), (long_, 4.0)):
        s.tubes([[(x, 6.2 + 1), (x, 6.2 + 1 + depth)] for x in xs] + [[(x, 44.8 - 1), (x, 44.8 - 1 - depth)] for x in xs],
                .22, gold(s), z=.95)


sa('banner', banner, size=(120, 56), slice=Slice(18, 30, 30, 22))


# ================================================================ the info cloud: a perforated sheet
def perforated(box, centres, r=3.2, cut=5):
    """A sheet with a row of round bites along every side (the stub torn off a ticket), corners cut by `cut`."""
    x0, y0, x1, y1 = box
    pts = [(x0 + cut, y0)]
    for cx in centres:
        pts += arc(cx, y0, r, math.pi, 0)
    pts += [(x1 - cut, y0), (x1, y0 + cut)]
    for cy in centres:
        pts += arc(x1, cy, r, 1.5 * math.pi, .5 * math.pi)
    pts += [(x1, y1 - cut), (x1 - cut, y1)]
    for cx in reversed(centres):
        pts += arc(cx, y1, r, 0, -math.pi)
    pts += [(x0 + cut, y1), (x0, y1 - cut)]
    for cy in reversed(centres):
        pts += arc(x0, cy, r, .5 * math.pi, -.5 * math.pi)
    pts += [(x0, y0 + cut)]
    return pts


def info(s, w, h):
    """The info sheet: 96x96, tile 32, a bite every 16 px (two per tile), a cream map sheet, registration crosses
    in the corners (the badge sits over the top-left one)."""
    box = (3, 3, 93, 93)
    outline = perforated(box, [24, 40, 56, 72])
    sheet(s, outline, PAPER, binding=1.2, depth=2.4)
    s.tubes(straight(s, box, 8.5, 13), .25, thread(s, INK), z=.55)
    corners4(lambda place: registration(s, place, INK), w, h, inset=0)


sa('info', info, size=(96, 96), slice=Slice.uniform(32))


# ================================================================ the small things: a compass, ridges, brass
def compass(s, cx, cy, r):
    """A brass compass: a ring, navy enamel, a vermilion north needle and a pearl south one."""
    s.slab(circle(cx, cy, r), -1.5, .3, edge(s), bevel=.5)
    s.slab(circle(cx, cy, r - .4), -.4, 1.0, gold(s), bevel=.6)
    s.slab(circle(cx, cy, r * .76), .5, 1.3, s.mat('enamel', NAVY_D, rough=.35), bevel=.4)
    n, hw = r * .64, r * .2
    s.slab([(cx, cy - n), (cx - hw, cy), (cx + hw, cy)], 1.2, 2.3, s.mat('enamel', WAX, rough=.3), bevel=.25)
    s.slab([(cx, cy + n), (cx - hw, cy), (cx + hw, cy)], 1.2, 2.3, s.mat('pearl', PEARL), bevel=.25)
    s.beads([(cx, cy)], max(r * .11, .7), gold(s), z=2.3)


def rose(s, cx, cy, r):
    """A compass rose: a gold four-point star over a smaller one turned by 45 degrees, a ring, a vermilion north."""
    s.slab(rot(sparkle(cx, cy, r * .72, .3), cx, cy, math.pi / 4), -.6, .5, gold(s), bevel=.3)
    s.slab(sparkle(cx, cy, r, .24), -.2, 1.0, gold(s), bevel=.4)
    s.slab([(cx, cy - r * .98), (cx - r * .17, cy - r * .3), (cx + r * .17, cy - r * .3)], .9, 1.5,
           s.mat('enamel', WAX, rough=.3), bevel=.2)
    s.tube(circle(cx, cy, r * .3), r * .06, edge(s), z=1.1, closed=True, flat=.65, caps=None)
    s.beads([(cx, cy)], r * .1, gold(s), z=1.2)


sa('label-orn', lambda s, w, h: s.slab(sparkle(5, 5, 4.2, .25), -.3, .9, gold(s), bevel=.3), size=(10, 10), shear=.4)
sa('gem', lambda s, w, h: compass(s, 14, 14, 12.4), size=(28, 28), shear=.5)
sa('badge', lambda s, w, h: compass(s, 22, 23, 19.4), size=(44, 46))


def orn(s, w, h):
    """The mark at the left end of an option: a brass four-point star over a ring."""
    s.slab(sparkle(12, 12, 10.5, .22), -.3, 1.0, gold(s), bevel=.4)
    s.tube(circle(12, 12, 3.4), .45, edge(s), z=1.0, closed=True, flat=.65, caps=None)
    s.beads([(12, 12)], .9, gold(s), z=1.0)


sa('orn', orn, size=(24, 24), shear=.5)


def bubble(s, w, h):
    """A thought puff: a disc of tracing paper in a thin ink ring, a benchmark dot at its centre."""
    s.slab(circle(12, 12, 10), -.8, 1.0, s.mat('parchment', TRACE, grain=.6), bevel=2.6)
    s.tube(circle(12, 12, 10), .9, thread(s, INK), z=.8, closed=True, flat=.65, caps=None)
    s.tube(circle(12, 12, 6.2), .22, thread(s, INK_BLUE), z=.9, closed=True, flat=.7, caps=None)
    s.beads([(12, 12)], 1.3, thread(s, INK), z=1.0, flat=.8)


sa('bubble', bubble, size=(24, 24), shear=.5)


def knot(s, w, h):
    """The ornament over the corner of a plate: a compass rose and two brass sparkles."""
    rose(s, 28, 32, 24)
    for cx, cy, r in ((58, 7, 4.6), (7, 53, 3.4)):
        s.slab(sparkle(cx, cy, r, .2), -.4, .7, gold(s), bevel=.3)


sa('knot', knot, size=(64, 60), shear=.3)


def stat_icon(s, w, h):
    """The icon of a stat: navy enamel in a brass frame, a ridge of mountains in brass wire, a vermilion sun."""
    s.slab(rrect(1.25, 1.25, 32.75, 32.75, 6), -1, 1.0, gold(s), bevel=.6, holes=[rrect(3.4, 3.4, 30.6, 30.6, 4.2)])
    s.slab(rrect(3.2, 3.2, 30.8, 30.8, 4.4), -.6, .4, s.mat('enamel', NAVY_D, rough=.35), bevel=.4)
    s.facet(circle(25, 9.5, 3.1, 16), .3, 2.0, s.mat('gem', WAX), table=.45)
    ridge = [(5.5, 27), (11.5, 16.5), (15, 21.5), (21.5, 11.5), (28.5, 27)]
    s.tube(ridge, .8, gold(s), z=.6, flat=.75, caps='round')
    s.tubes([[(6, 28.2), (28, 28.2)]], .6, gold(s), z=.6, flat=.75)


sa('stat-steppe', stat_icon, size=(34, 34), shear=.5)


# ================================================================ the title
def brackets(s, w, h):
    """The brackets of the title: a double neatline of brass (a thick line and a thin one with ticks between them)
    and a registration ring at each end. 160x80, slice 30 / 50 (tile 60 x 20), no centre."""
    g = gold(s)
    for flip in (False, True):
        def v(pts):
            return [(x, h - y) for x, y in pts] if flip else pts
        s.tube(v([(14, 14), (w - 14, 14)]), 1.1, g, z=.6, flat=.75, caps='round')
        s.tube(v([(14, 19.4), (w - 14, 19.4)]), .45, g, z=.4, flat=.75, caps='round')
        s.tubes([v([(x, 15.4), (x, 18)]) for x in lattice(50, w - 50, s.borders.left, 5)], .3, g, z=.5)
        for x in (14, w - 14):
            s.tube(v(circle(x, 14, 4.2)), .5, g, z=.6, closed=True, flat=.75, caps=None)
            s.tubes([v([(x - 7, 14), (x + 7, 14)]), v([(x, 7), (x, 21)])], .4, g, z=.6)


sa('title-brackets', brackets, size=(160, 80), slice=Slice(30, 50, 50, 30, fill=False), halo=TITLE_CREAM, shear=.6)


def title_ornament(s, w, h):
    """The centre of a bracket: a compass rose between two diamonds and two brass ticks."""
    rose(s, 32, 16, 15)
    for x in (14, 50):
        s.slab(rhombus(x, 16, 2.6), -.4, .9, gold(s), bevel=.5)
    for x0, x1 in ((2, 9), (55, 62)):
        s.slab(stroke([(x0, 16), (x1, 16)], .7), -.3, .8, gold(s), bevel=.3)


sa('title-orn', title_ornament, size=(64, 32), shear=.5)


# ================================================================ the round buttons and the switches of the story screens
FOREST = '#2f5d46'


def round_seal(s, w, h):
    """40x42: the round button of the story screens (back, settings, collapse): a brass ring round navy leather, a ring of
    ruler ticks, the glyph is laid over it by the game."""
    from sprites3d import medallion
    medallion(s, 20, 20, 18, leather(s, NAVY_D))
    paths = []
    n = 24
    for k in range(n):
        a = math.tau * k / n
        r0, r1 = 12.6, 14.4 if k % 2 else 15.4
        paths.append([(20 + r0 * math.cos(a), 20 + r0 * math.sin(a)), (20 + r1 * math.cos(a), 20 + r1 * math.sin(a))])
    s.tubes(paths, .22, gold(s), z=.9)


sa('round', round_seal, size=(40, 42))


def switch(s, w, h, on):
    """The switch: 60x34, a navy leather track (forest green when on) in a brass rim, a brass knob with a ring on it."""
    body = rrect(1.5, 1.5, 58.5, 29.5, 14)
    plaque(s, body, leather(s, FOREST if on else NAVY_D), bevel=1.8)
    s.tubes(straight(s, (1.5, 1.5, 58.5, 29.5), 4.0, 15), .2, gold(s), z=.95)
    x = 43 if on else 17
    s.slab(circle(x, 15.5, 11.4), -1.6, 1.3, edge(s), bevel=.6)
    s.slab(circle(x, 15.5, 10.4), -.6, 2.0, gold(s), bevel=.7)
    s.slab(circle(x, 15.5, 7.6), .4, 2.7, s.mat('pearl', PEARL), bevel=2.0)
    s.tube(circle(x, 15.5, 4.2), .3, gold(s), z=2.7, closed=True, flat=.8, caps=None)


sa('switch-on', partial(switch, on=True), size=(60, 34))
sa('switch-off', partial(switch, on=False), size=(60, 34))
