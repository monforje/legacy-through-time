"""Plates of text: the plain plate (3 colours), the plate with its speech horn, the thought clouds, the info cloud."""
import math
from functools import partial

from kit.geometry import (arc_points, chamfer, corners, f, leaf, mirrored, poly, rhombus, shifted, sparkle,
                          spline, wave)
from kit.palette import EMERALD, INK, MINT, PAPER, ROSE_PAPER, SCARLET
from kit.registry import REGISTRY, Slice
from kit.svg import GRAIN, Sprite, grain
from .ornaments import corner_hook


# ---------------------------------------------------------------- plain plate
def bevel(octagon, dark=.13, width=3):
    """A soft shadow along the bottom and right inner edges. (There is no light strip along the top and left:
    on a big screen it read as a white inner frame.)"""
    p = octagon
    path = 'M' + ' L'.join(f'{f(x)},{f(y)}' for x, y in (p[2], p[3], p[4], p[5]))
    return (f'<path d="{path}" fill="none" stroke="#5a2e1a" stroke-opacity="{dark}" stroke-width="{width}" '
            f'stroke-linecap="round" stroke-linejoin="round"/>')


def plate(hand, name, fill, line=SCARLET):
    """96x96, slice 32. A black outline of 3.5, the sticker shadow (+3) under it, a double thread of colour
    `line` inside, hooks in the corners. Body 2..94 x 2..90."""
    w = h = 96
    body = chamfer(2, 2, 94, 90, 10)
    outline = hand.wobble(body)                      # one path for the fill and the grain: they never drift apart
    return Sprite(name, w, h, '\n'.join([
        GRAIN,
        f'<path d="{hand.wobble(shifted(body, 3))}" fill="{INK}"/>',
        f'<path d="{outline}" fill="{fill}" stroke="{INK}" stroke-width="3.5" stroke-linejoin="round"/>',
        grain(outline),
        f'<path d="{hand.wobble(chamfer(6.5, 6.5, 89.5, 85.5, 7), amp=.2)}" fill="none" stroke="{line}" stroke-width="2" stroke-linejoin="round"/>',
        f'<path d="{hand.wobble(chamfer(10, 10, 86, 82, 5), amp=.2)}" fill="none" stroke="{line}" stroke-width=".9" stroke-linejoin="round"/>',
        bevel(chamfer(14, 14, 82, 78, 4)),
        corners(corner_hook(), w, h),
    ]), 'border-image: slice 32 fill; body 2..94 x 2..90, shadow to 93')


for _name, _fill, _line in (('plate', PAPER, SCARLET), ('plate-mint', MINT, EMERALD), ('plate-rose', ROSE_PAPER, SCARLET)):
    REGISTRY.add(_name, partial(plate, name=_name, fill=_fill, line=_line), Slice.uniform(32))


# ---------------------------------------------------------------- plate with the speech horn
def plate_tail(hand, name, mirror=False):
    """A plate with its horn in ONE outline (no seam to align), 32 px of headroom above the plate for the horn.
    180x128; slice: 64 on top (headroom + corner), 32 on the left, 140 on the right (the horn lives there and does
    not stretch), 32 at the bottom. The horn points left-up, 72..132 px from the right edge: that is the tail of
    the heroine's lines; `mirror` gives everybody else. The sticker shadow (+3) is shared by plate and horn."""
    width, height, top = 180, 128, 32
    x0, x1, y0, y1, cut = 2, width - 2, 2 + top, 90 + top, 10
    ox = width - 132                                  # origin of the horn: the coordinates of the old tail.svg
    left, right = ox + 26, ox + 58                    # the base of the horn on the top outline
    apex = (ox + 3, 2)

    pts = chamfer(x0, y0, x1, y1, cut)
    # the left side leaves the plate almost vertically, the right one is a long concave arc into the top outline
    horn = (f'L{left},{y0} C{ox + 24},26 {ox + 8},19 {apex[0]},{apex[1]} '
            f'C{ox + 17},20 {ox + 34},{y0} {right + 4},{y0}')
    d = f'M{f(pts[0][0])},{f(pts[0][1])} ' + spline([pts[0], *hand.edge(pts[0], (left, y0), 12, .3)])
    d += ' ' + horn
    d += ' ' + spline([(right + 4, y0), *hand.edge((right + 4, y0), pts[1], 12, .3)])
    for a, b in zip(pts[1:], pts[2:] + [pts[0]]):
        d += ' ' + spline([a] + hand.edge(a, b, 12, .3))
    body = d + 'Z'

    # The horn is too narrow for a thread of its own: the plate's threads run unbroken under it.
    shape = '\n'.join([
        GRAIN,
        f'<path d="{body}" transform="translate(0,3)" fill="{INK}"/>',
        f'<path d="{body}" fill="{PAPER}" stroke="{INK}" stroke-width="3.5" stroke-linejoin="round"/>',
        grain(body),
        f'<path d="{hand.wobble(chamfer(6.5, 38.5, width - 6.5, 117.5, 7), amp=.2)}" fill="none" stroke="{SCARLET}" stroke-width="2" stroke-linejoin="round"/>',
        f'<path d="{hand.wobble(chamfer(10, 42, width - 10, 114, 5), amp=.2)}" fill="none" stroke="{SCARLET}" stroke-width=".9" stroke-linejoin="round"/>',
        corners(corner_hook(), width, height, top=top),
    ])
    return Sprite(name, width, height, mirrored(shape, width) if mirror else shape,
                  'border-image: top 64, left 32, right 140, bottom 32 (mirror: the other way round)')


REGISTRY.add('plate-tail-r', partial(plate_tail, name='plate-tail-r'), Slice(64, 32, 140, 32))
REGISTRY.add('plate-tail-l', partial(plate_tail, name='plate-tail-l', mirror=True), Slice(64, 140, 32, 32))


# ---------------------------------------------------------------- thought clouds (tiled edges)
def sparkle_in_corner(cx, cy, r, sw=1.6):
    """A red star with a dark outline. It must lie wholly inside one 40x40 corner cell (1 px to spare):
    anything that reaches an edge cell would repeat along the whole side."""
    return sparkle(cx, cy, r, SCARLET, outline=INK, sw=sw, pinch=.18)


def thought(hand, name='thought'):
    """The dashed "drop" cloud: an uneven elongated outline with a small wave, thick black dashes, red stars in the
    corners. 120x120, slice 40, tiled edges: one tile of 40 px = one period of the wave = 4 dashes."""
    cell, radius, inset, amp = 40, 34, 5, 1.5
    lo, hi = inset, 120 - inset
    top = wave(cell, 80, lo, amp)
    bottom = [(x, hi + amp * math.sin(math.tau * (x - cell) / 40)) for x, _ in wave(cell, 80, hi, amp)][::-1]
    left = [(lo + amp * math.sin(math.tau * (y - cell) / 40), y) for _, y in wave(cell, 80, lo, amp, vertical=True)][::-1]
    right = [(hi + amp * math.sin(math.tau * (y - cell) / 40), y) for _, y in wave(cell, 80, hi, amp, vertical=True)]
    tl = arc_points(lo + radius, lo + radius, radius, math.pi, 1.5 * math.pi)
    tr = arc_points(hi - radius, lo + radius, radius, 1.5 * math.pi, 2 * math.pi)
    br = arc_points(hi - radius, hi - radius, radius, 0, .5 * math.pi)
    bl = arc_points(lo + radius, hi - radius, radius, .5 * math.pi, math.pi)
    outline = poly(tl + top + tr + right + br + bottom + bl + left) + 'Z'
    stroke = f'fill="none" stroke="{INK}" stroke-width="3.2" stroke-linecap="butt" stroke-dasharray="6 4"'
    # No grain: the centre cell is stretched many times and the noise would turn into specks.
    parts = [f'<path d="{outline}" fill="{PAPER}"/>']
    for pts in (top, bottom[::-1], left[::-1], right):
        parts.append(f'<path d="{poly(pts)}" pathLength="40" {stroke}/>')
    for pts in (tl, tr, br, bl):
        parts.append(f'<path d="{poly(pts)}" pathLength="50" {stroke}/>')
    parts += [sparkle(106, 14, 13, SCARLET, INK, 1.5, .18), sparkle(14, 96, 9.5, SCARLET, INK, 1.5, .18),
              sparkle(24, 110, 6.5, SCARLET, INK, 1.5, .18)]
    return Sprite(name, 120, 120, '\n'.join(parts), 'border-image: slice 40 fill round')


def thought_cloud(hand, name='thought-cloud'):
    """The puffy cloud with scallops: arcs of 20 px, small outlined stars inside. 120x120, slice 40, tiled edges:
    one tile of 40 px = 2 scallops."""
    b = 8
    e = 120 - b
    # clockwise chain of points; each arc is a scallop bulging out
    chain = [(b, 40), (b, 20), (20, b), (40, b), (60, b), (80, b), (100, b), (e, 20), (e, 40), (e, 60), (e, 80),
             (e, 100), (100, e), (80, e), (60, e), (40, e), (20, e), (b, 100), (b, 80), (b, 60), (b, 40)]
    radii = {i: 11 for i in range(len(chain) - 1)}
    radii.update({1: 12, 6: 12, 11: 12, 16: 12})        # the diagonals of the corners are a little rounder
    d = f'M{chain[0][0]},{chain[0][1]}' + ''.join(f' A{radii[i]},{radii[i]} 0 0 1 {x},{y}' for i, (x, y) in enumerate(chain[1:])) + 'Z'
    parts = [f'<path d="{d}" fill="{PAPER}"/>']
    for i, (x, y) in enumerate(chain[1:]):
        x0, y0 = chain[i]
        r = radii[i]
        parts.append(f'<path d="M{x0},{y0} A{r},{r} 0 0 1 {x},{y}" pathLength="32" stroke-dasharray="6.5 1.5" '
                     f'fill="none" stroke="{INK}" stroke-width="3" stroke-linecap="butt"/>')
    # Stars only inside the corner cells (x < 40 or x > 80, y < 40 or y > 80), ray and outline wholly inside.
    parts += [sparkle_in_corner(102, 24, 9, 2), sparkle_in_corner(86, 34, 5),
              sparkle_in_corner(18, 96, 9, 2), sparkle_in_corner(34, 86, 5)]
    return Sprite(name, 120, 120, '\n'.join(parts), 'border-image: slice 40 fill round')


REGISTRY.add('thought', thought, Slice.uniform(40, 'tiled'))
REGISTRY.add('thought-cloud', thought_cloud, Slice.uniform(40, 'tiled'))


# ---------------------------------------------------------------- info cloud
def info(hand):
    """The info cloud with scallops. 96x96, slice 32, tiled edges: 2 scallops per tile."""
    base, cell, size = 6, 32, 96
    small, big = 10, 20
    e = size - base
    d = [f'M{cell},{base}']
    d += [f'A{small},{small} 0 0 1 {x},{base}' for x in (48, 64)]
    d.append(f'A{big},{big} 0 0 1 {e},{cell}')
    d += [f'A{small},{small} 0 0 1 {e},{y}' for y in (48, 64)]
    d.append(f'A{big},{big} 0 0 1 64,{e}')
    d += [f'A{small},{small} 0 0 1 {x},{e}' for x in (48, 32)]
    d.append(f'A{big},{big} 0 0 1 {base},64')
    d += [f'A{small},{small} 0 0 1 {base},{y}' for y in (48, 32)]
    d.append(f'A{big},{big} 0 0 1 {cell},{base}Z')
    leaves = corners(''.join([leaf(14, 20, 15, 32, 3, EMERALD), leaf(20, 14, 32, 15, -3, EMERALD), rhombus(17, 17, 3.2, SCARLET)]),
                     size, size, inset=0)
    return Sprite('info', size, size, '\n'.join([
        f'<path d="{" ".join(d)}" fill="{PAPER}" stroke="{INK}" stroke-width="2.5" stroke-linejoin="round"/>', leaves,
    ]), 'border-image: slice 32 fill round')


REGISTRY.add('info', info, Slice.uniform(32, 'tiled'))
