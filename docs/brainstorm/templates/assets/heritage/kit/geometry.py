"""Shapes and paths. Pure functions of their arguments (no hidden state); the only randomness is `Hand`."""
import math
import random



def f(x):
    """A number for an SVG attribute: at most two decimals, no trailing zeros."""
    return f'{x:.2f}'.rstrip('0').rstrip('.')


# ---------------------------------------------------------------- polygons
def chamfer(x0, y0, x1, y1, c):
    """Rectangle with cut corners (8 points, clockwise from the top-left cut)."""
    return [(x0 + c, y0), (x1 - c, y0), (x1, y0 + c), (x1, y1 - c), (x1 - c, y1), (x0 + c, y1), (x0, y1 - c), (x0, y0 + c)]


def hexbar(x0, y0, x1, y1, c):
    """A strip with hexagonal ends (name tags, buttons)."""
    ym = (y0 + y1) / 2
    return [(x0 + c, y0), (x1 - c, y0), (x1, ym), (x1 - c, y1), (x0 + c, y1), (x0, ym)]


def rrect(x0, y0, x1, y1, r):
    """Rounded rectangle as an SVG path."""
    return (f'M{f(x0 + r)},{f(y0)} H{f(x1 - r)} A{f(r)},{f(r)} 0 0 1 {f(x1)},{f(y0 + r)} V{f(y1 - r)} '
            f'A{f(r)},{f(r)} 0 0 1 {f(x1 - r)},{f(y1)} H{f(x0 + r)} A{f(r)},{f(r)} 0 0 1 {f(x0)},{f(y1 - r)} '
            f'V{f(y0 + r)} A{f(r)},{f(r)} 0 0 1 {f(x0 + r)},{f(y0)}Z')


def poly(pts):
    return 'M' + ' L'.join(f'{f(x)},{f(y)}' for x, y in pts)


def shifted(points, dy):
    """The same shape lower by `dy`: the hard sticker shadow."""
    return [(x, y + dy) for x, y in points]


# ---------------------------------------------------------------- curves
def spline(pts):
    """Cubic Béziers through the points (Catmull-Rom): a hand-drawn line that is smooth, not faceted.
    Returns the C commands without the leading M; pts[0] is the current point."""
    out = []
    for i in range(len(pts) - 1):
        p0 = pts[max(i - 1, 0)]; p1 = pts[i]; p2 = pts[i + 1]; p3 = pts[min(i + 2, len(pts) - 1)]
        c1 = (p1[0] + (p2[0] - p0[0]) / 6, p1[1] + (p2[1] - p0[1]) / 6)
        c2 = (p2[0] - (p3[0] - p1[0]) / 6, p2[1] - (p3[1] - p1[1]) / 6)
        out.append(f'C{f(c1[0])},{f(c1[1])} {f(c2[0])},{f(c2[1])} {f(p2[0])},{f(p2[1])}')
    return ' '.join(out)


def bezier(p, t):
    """Point of a cubic Bézier with control points p[0..3] at t."""
    u = 1 - t
    return tuple(u ** 3 * p[0][k] + 3 * u * u * t * p[1][k] + 3 * u * t * t * p[2][k] + t ** 3 * p[3][k] for k in (0, 1))


def wave(x0, x1, y, amp, period=40, n=20, vertical=False, phase=0.0):
    """A wavy line with a whole number of periods, so that a repeating tile (round) joins without a seam."""
    pts = []
    for i in range(n + 1):
        s = x0 + (x1 - x0) * i / n
        o = y + amp * math.sin(math.tau * (s - x0) / period + phase)
        pts.append((o, s) if vertical else (s, o))
    return pts


def arc_points(cx, cy, r, a0, a1, n=14):
    return [(cx + r * math.cos(a0 + (a1 - a0) * i / n), cy + r * math.sin(a0 + (a1 - a0) * i / n)) for i in range(n + 1)]


# ---------------------------------------------------------------- small figures
def rhombus(cx, cy, r, fill, stroke=None, sw=0):
    s = f' stroke="{stroke}" stroke-width="{sw}" stroke-linejoin="round"' if stroke else ''
    return f'<path d="M{f(cx)},{f(cy - r)} L{f(cx + r)},{f(cy)} L{f(cx)},{f(cy + r)} L{f(cx - r)},{f(cy)}Z" fill="{fill}"{s}/>'


def leaf(x0, y0, x1, y1, bulge, fill):
    mx, my = (x0 + x1) / 2, (y0 + y1) / 2
    nx, ny = -(y1 - y0), x1 - x0
    ln = math.hypot(nx, ny)
    nx, ny = nx / ln * bulge, ny / ln * bulge
    return (f'<path d="M{f(x0)},{f(y0)} Q{f(mx + nx)},{f(my + ny)} {f(x1)},{f(y1)} '
            f'Q{f(mx - nx)},{f(my - ny)} {f(x0)},{f(y0)}Z" fill="{fill}"/>')


def sparkle(cx, cy, r, fill, outline=None, sw=1.5, pinch=.28):
    """Four-pointed star with concave sides; rays exactly along the axes. With `outline` it is stroked
    (round joins, never miter: a miter at a sharp ray tip sticks out of a 9-slice cell and tiles)."""
    k = r * pinch
    d = (f'M{f(cx)},{f(cy - r)} Q{f(cx + k)},{f(cy - k)} {f(cx + r)},{f(cy)} '
         f'Q{f(cx + k)},{f(cy + k)} {f(cx)},{f(cy + r)} Q{f(cx - k)},{f(cy + k)} {f(cx - r)},{f(cy)} '
         f'Q{f(cx - k)},{f(cy - k)} {f(cx)},{f(cy - r)}Z')
    stroke = f' stroke="{outline}" stroke-width="{sw}" stroke-linejoin="round"' if outline else ''
    return f'<path d="{d}" fill="{fill}"{stroke}/>'


def corners(motif, w, h, top=0, inset=4):
    """The motif drawn for the top-left corner, repeated in the other three. `top`: extra height above the shape;
    `inset`: how far above the sprite's bottom edge the mirror line of the lower corners lies (the shadow room)."""
    return ''.join(f'<g transform="{t}">{motif}</g>' for t in (
        f'translate(0,{top})', f'translate({w},{top}) scale(-1,1)',
        f'translate(0,{h - inset}) scale(1,-1)', f'translate({w},{h - inset}) scale(-1,-1)'))


def mirrored(content, width):
    return f'<g transform="translate({width},0) scale(-1,1)">{content}</g>'


# ---------------------------------------------------------------- the hand
class Hand:
    """Hand-drawn jitter. The generator is seeded by the sprite's name, so one sprite never changes because
    another one was added, removed or built in a different order."""

    def __init__(self, name):
        self.rnd = random.Random(f'heritage:{name}')

    def edge(self, a, b, step=8, amp=.35):
        """Points of the segment a->b (without a, with b); the middle wobbles sideways, the ends stay put
        so that corners remain sharp."""
        (x0, y0), (x1, y1) = a, b
        n = max(1, int(math.hypot(x1 - x0, y1 - y0) // step))
        nx, ny = -(y1 - y0), x1 - x0
        ln = math.hypot(nx, ny) or 1
        nx, ny = nx / ln, ny / ln
        pts = []
        for i in range(1, n):
            t = i / n
            j = self.rnd.uniform(-amp, amp)
            pts.append((x0 + (x1 - x0) * t + nx * j, y0 + (y1 - y0) * t + ny * j))
        pts.append((x1, y1))
        return pts

    def wobble(self, points, closed=True, step=8, amp=.35):
        """An outline "by hand": every edge wobbles slightly but is one smooth spline; corners stay sharp."""
        pts = points + ([points[0]] if closed else [])
        d = f'M{f(pts[0][0])},{f(pts[0][1])}'
        for a, b in zip(pts, pts[1:]):
            d += ' ' + spline([a] + self.edge(a, b, step, amp))
        return d + ('Z' if closed else '')
