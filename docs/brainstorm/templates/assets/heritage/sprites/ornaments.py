"""Ornaments: the corner hook of a plate, the knot over a plate, the thought bubble, the "!" seal, the solar rhombus."""
import math

from kit.geometry import f, leaf, rhombus, sparkle
from kit.palette import BRONZE, CREAM, INK, PAPER, SCARLET, SPRING
from kit.registry import REGISTRY
from kit.svg import Sprite


def corner_hook():
    """The ornament of the top-left corner of a plate: a red hook with squares on its ends and a thin thread inside.
    Drawn in a cell 16..33, scaled to 0.72 and tucked into the corner (it occupies 13..25 px from the edge), so that
    the text of the plate (padding 30) never touches it."""
    def square(x, y, s=1.7):
        return f'<rect x="{f(x - s)}" y="{f(y - s)}" width="{f(2 * s)}" height="{f(2 * s)}" fill="{SCARLET}"/>'
    body = ''.join([
        f'<path d="M16,28 V20.5 Q16,16.5 20,16.5 H28" fill="none" stroke="{SCARLET}" stroke-width="1.8" stroke-linecap="round"/>',
        f'<path d="M21,28 V24.5 Q21,21.5 24,21.5 H29" fill="none" stroke="{SCARLET}" stroke-width="1.1" stroke-linecap="round"/>',
        square(16, 31.5), square(31.5, 16.5), rhombus(20.5, 20.5, 2.3, SCARLET),
    ])
    return f'<g transform="translate(1.5,1.5) scale(.72)">{body}</g>'


@REGISTRY.sprite('knot')
def knot(hand):
    """A big red knot of rhombuses over the top-right corner of a plate: two interlaced rhombus threads and a small
    gem. At every crossing the thread goes over, then under."""
    def diamond(cx, cy, r):
        return [(cx, cy - r), (cx + r, cy), (cx, cy + r), (cx - r, cy)]

    def edges(pts):
        return list(zip(pts, pts[1:] + pts[:1]))

    def cross(a, b):
        (x1, y1), (x2, y2) = a
        (x3, y3), (x4, y4) = b
        d = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4)
        if abs(d) < 1e-9:
            return None
        t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / d
        u = -((x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3)) / d
        if 0 < t < 1 and 0 < u < 1:
            return x1 + t * (x2 - x1), y1 + t * (y2 - y1)

    def thread(pts, outer=5.2, inner=2.4):
        d = 'M' + ' L'.join(f'{f(x)},{f(y)}' for x, y in pts) + 'Z'
        return (f'<path d="{d}" fill="none" stroke="{INK}" stroke-width="{outer}" stroke-linejoin="miter"/>'
                f'<path d="{d}" fill="none" stroke="{SCARLET}" stroke-width="{inner}" stroke-linejoin="miter"/>')

    a, b, c = diamond(28, 32, 19), diamond(42, 26, 15), diamond(28, 32, 9.5)
    parts = [thread(pts) for pts in (a, b, c)]
    crossings = [(p, ea) for ea in edges(a) for eb in edges(b) if (p := cross(ea, eb))]
    for k, (p, ((x1, y1), (x2, y2))) in enumerate(crossings):
        if k % 2:
            continue                                          # every second crossing: A over B
        ln = math.hypot(x2 - x1, y2 - y1)
        ux, uy = (x2 - x1) / ln * 5, (y2 - y1) / ln * 5
        seg = f'M{f(p[0] - ux)},{f(p[1] - uy)} L{f(p[0] + ux)},{f(p[1] + uy)}'
        parts.append(f'<path d="{seg}" stroke="{INK}" stroke-width="5.2" stroke-linecap="butt"/>'
                     f'<path d="{seg}" stroke="{SCARLET}" stroke-width="2.4" stroke-linecap="butt"/>')
    parts += [rhombus(28, 32, 3.6, PAPER, INK, 1.4), rhombus(42, 26, 2.6, PAPER, INK, 1.2),
              sparkle(58, 7, 4.5, SCARLET), sparkle(7, 53, 3.2, SCARLET)]
    return Sprite('knot', 64, 60, '\n'.join(parts), 'rhombus knot over the top-right corner of a plate')


@REGISTRY.sprite('orn')
def orn(hand):
    """The solar rhombus with leaves at the left end of a button."""
    return Sprite('orn', 24, 24, '\n'.join([
        leaf(3, 12, 8.5, 12, 2.4, SPRING),
        leaf(15.5, 12, 21, 12, 2.4, SPRING),
        rhombus(12, 12, 5.6, SCARLET, CREAM, 1.2),
        rhombus(12, 12, 2, CREAM),
        f'<circle cx="12" cy="3.2" r="1.1" fill="{CREAM}"/>',
        f'<circle cx="12" cy="20.8" r="1.1" fill="{CREAM}"/>',
    ]))


@REGISTRY.sprite('bubble')
def bubble(hand):
    """A thought bubble (in the web version these are two CSS circles): parchment with an ink outline."""
    return Sprite('bubble', 24, 24, f'<circle cx="12" cy="12" r="10" fill="{PAPER}" stroke="{INK}" stroke-width="2.5"/>')


@REGISTRY.sprite('badge')
def badge(hand):
    """The "!" seal of the info cloud: a red star-shaped stamp."""
    pts = [(22 + (20 if i % 2 == 0 else 17) * math.cos(i * math.tau / 24 - math.pi / 2),
            22 + (20 if i % 2 == 0 else 17) * math.sin(i * math.tau / 24 - math.pi / 2)) for i in range(24)]
    return Sprite('badge', 44, 46, '\n'.join([
        f'<path d="{hand.wobble([(x, y + 2) for x, y in pts], step=20)}" fill="{INK}"/>',
        f'<path d="{hand.wobble(pts, step=20)}" fill="{SCARLET}" stroke="{INK}" stroke-width="2.5" stroke-linejoin="round"/>',
        f'<path d="M20,11.5 H24 L23,25 H21Z" fill="{CREAM}" stroke="{INK}" stroke-width="1" stroke-linejoin="round"/>',
        f'<circle cx="22" cy="30.5" r="2.6" fill="{CREAM}" stroke="{INK}" stroke-width="1"/>',
    ]))
