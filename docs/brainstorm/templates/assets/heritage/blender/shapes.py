"""Outlines of the Blender kit: lists of (x, y) points in px of the sprite (x right, y down, like the SVG kit).
Pure functions, no bpy: exact curves (arcs, Béziers), round joins (`fillet`), dashes, evenly spaced points."""
import math

from kit.geometry import chamfer, hexbar  # noqa: F401  (the same polygons as the SVG kit)

STEP = .35          # px between points of a curve: smooth at @4x


def arc(cx, cy, r, a0, a1, n=None):
    """Points of an arc from angle a0 to a1 (radians, y down: positive is clockwise on screen), both ends included."""
    n = n or max(6, int(abs(a1 - a0) * r / STEP))
    return [(cx + r * math.cos(a0 + (a1 - a0) * i / n), cy + r * math.sin(a0 + (a1 - a0) * i / n)) for i in range(n + 1)]


def circle(cx, cy, r, n=None):
    """A closed circle (the first point is not repeated)."""
    return arc(cx, cy, r, 0, math.tau, n or max(48, int(math.tau * r / STEP)))[:-1]


def rrect(x0, y0, x1, y1, r):
    """A rounded rectangle, clockwise from the top-left arc."""
    return (arc(x0 + r, y0 + r, r, math.pi, 1.5 * math.pi) + arc(x1 - r, y0 + r, r, 1.5 * math.pi, 2 * math.pi)
            + arc(x1 - r, y1 - r, r, 0, .5 * math.pi) + arc(x0 + r, y1 - r, r, .5 * math.pi, math.pi))


def rhombus(cx, cy, r):
    return [(cx, cy - r), (cx + r, cy), (cx, cy + r), (cx - r, cy)]


def bezier(p0, p1, p2, p3, n=None):
    """A cubic Bézier, both ends included."""
    n = n or max(8, int((dist(p0, p1) + dist(p1, p2) + dist(p2, p3)) / STEP))
    out = []
    for i in range(n + 1):
        t = i / n
        u = 1 - t
        out.append(tuple(u ** 3 * p0[k] + 3 * u * u * t * p1[k] + 3 * u * t * t * p2[k] + t ** 3 * p3[k] for k in (0, 1)))
    return out


def quad(p0, p1, p2, n=None):
    """A quadratic Bézier, both ends included."""
    return bezier(p0, lerp(p0, p1, 2 / 3), lerp(p2, p1, 2 / 3), p2, n)


def sparkle(cx, cy, r, pinch=.28):
    """The four-pointed star of the SVG kit (concave sides, rays along the axes)."""
    k = r * pinch
    top, right, bottom, left = (cx, cy - r), (cx + r, cy), (cx, cy + r), (cx - r, cy)
    pts = []
    for a, c, b in ((top, (cx + k, cy - k), right), (right, (cx + k, cy + k), bottom),
                    (bottom, (cx - k, cy + k), left), (left, (cx - k, cy - k), top)):
        pts += quad(a, c, b)[:-1]
    return pts


def star(cx, cy, n, r_out, r_in, a0=-math.pi / 2):
    """A star polygon of n rays."""
    return [(cx + (r_out if i % 2 == 0 else r_in) * math.cos(a0 + i * math.pi / n),
             cy + (r_out if i % 2 == 0 else r_in) * math.sin(a0 + i * math.pi / n)) for i in range(2 * n)]


# ---------------------------------------------------------------- operations on point lists
def dist(a, b):
    return math.hypot(b[0] - a[0], b[1] - a[1])


def lerp(a, b, t):
    return a[0] + (b[0] - a[0]) * t, a[1] + (b[1] - a[1]) * t


def length(pts, closed=False):
    seq = pts + pts[:1] if closed else pts
    return sum(dist(a, b) for a, b in zip(seq, seq[1:]))


def dedupe(pts, closed=True, eps=1e-4):
    """Without repeated points (where two arcs of a rounded rectangle meet, say)."""
    out = [p for k, p in enumerate(pts) if k == 0 or dist(p, pts[k - 1]) > eps]
    if closed and len(out) > 1 and dist(out[0], out[-1]) <= eps:
        out.pop()
    return out


def fillet(pts, r, closed=True):
    """Rounds every corner with an arc of radius r (smaller where the edges are short): the round stroke-linejoin
    of the SVG kit. A tube along a sharp corner would otherwise grow a miter spike."""
    pts = dedupe(pts, closed)
    n = len(pts)
    out = []
    for i in range(n):
        if not closed and i in (0, n - 1):
            out.append(pts[i])
            continue
        a, p, b = pts[i - 1], pts[i], pts[(i + 1) % n]
        u = ((a[0] - p[0]) / dist(a, p), (a[1] - p[1]) / dist(a, p))
        v = ((b[0] - p[0]) / dist(b, p), (b[1] - p[1]) / dist(b, p))
        cos = max(-1.0, min(1.0, u[0] * v[0] + u[1] * v[1]))
        angle = math.acos(cos)
        if angle > math.pi - 1e-3:                    # a straight point: nothing to round
            out.append(p)
            continue
        t = r / math.tan(angle / 2)                   # the distance from the corner to the tangent points
        t = min(t, dist(a, p) * .5, dist(b, p) * .5)
        rr = t * math.tan(angle / 2)
        p1 = (p[0] + u[0] * t, p[1] + u[1] * t)
        p2 = (p[0] + v[0] * t, p[1] + v[1] * t)
        bis = (u[0] + v[0], u[1] + v[1])
        bl = math.hypot(*bis)
        c = (p[0] + bis[0] / bl * rr / math.sin(angle / 2), p[1] + bis[1] / bl * rr / math.sin(angle / 2))
        a1, a2 = math.atan2(p1[1] - c[1], p1[0] - c[0]), math.atan2(p2[1] - c[1], p2[0] - c[0])
        while a2 - a1 > math.pi:
            a2 -= math.tau
        while a1 - a2 > math.pi:
            a2 += math.tau
        out += arc(c[0], c[1], rr, a1, a2, max(3, int(abs(a2 - a1) * rr / STEP)))
    return out


def area(pts):
    """Shoelace area: positive when the points turn counter-clockwise in (x, y) as written."""
    return sum(x0 * y1 - x1 * y0 for (x0, y0), (x1, y1) in zip(pts, pts[1:] + pts[:1])) / 2


def offset(pts, d):
    """A closed outline moved outward by d (inward when d < 0), the way a stroke of width 2d grows a shape:
    the corners that turn away from the offset become arcs, the others meet at a point."""
    pts = dedupe(pts)
    if area(pts) < 0:
        pts = pts[::-1]
    out, n = [], len(pts)
    for i in range(n):
        a, p, b = pts[i - 1], pts[i], pts[(i + 1) % n]
        la, lb = dist(a, p), dist(p, b)
        if la < 1e-9 or lb < 1e-9:
            continue
        e1 = ((p[0] - a[0]) / la, (p[1] - a[1]) / la)
        e2 = ((b[0] - p[0]) / lb, (b[1] - p[1]) / lb)
        n1, n2 = (e1[1], -e1[0]), (e2[1], -e2[0])
        cross = e1[0] * e2[1] - e1[1] * e2[0]
        if abs(cross) < 1e-6:
            out.append((p[0] + n1[0] * d, p[1] + n1[1] * d))
        elif cross * d > 0:          # counter-clockwise points, outward normals to the right: a left turn is convex
            a1, a2 = math.atan2(n1[1], n1[0]), math.atan2(n2[1], n2[0])
            delta = (a2 - a1 + math.pi) % math.tau - math.pi
            steps = max(1, int(abs(delta) * abs(d) / STEP))
            out += [(p[0] + d * math.cos(a1 + delta * k / steps), p[1] + d * math.sin(a1 + delta * k / steps))
                    for k in range(steps + 1)]
        else:
            m = 1 + n1[0] * n2[0] + n1[1] * n2[1]
            out.append((p[0] + (n1[0] + n2[0]) / m * d, p[1] + (n1[1] + n2[1]) / m * d))
    return out


def resample(pts, step=STEP, closed=False):
    """Points every `step` px along the line (the corners are kept)."""
    seq = pts + pts[:1] if closed else pts
    out = [seq[0]]
    for a, b in zip(seq, seq[1:]):
        n = max(1, int(math.ceil(dist(a, b) / step)))
        out += [lerp(a, b, i / n) for i in range(1, n + 1)]
    return out[:-1] if closed else out


def sub(pts, s0, s1):
    """The piece of an open polyline between arc lengths s0 and s1."""
    out, s = [], 0.0
    for a, b in zip(pts, pts[1:]):
        d = dist(a, b)
        if d == 0:
            continue
        if s + d >= s0 and s <= s1:
            t0, t1 = max(0.0, (s0 - s) / d), min(1.0, (s1 - s) / d)
            if not out:
                out.append(lerp(a, b, t0))
            out.append(lerp(a, b, t1))
        s += d
    return out


def dashes(pts, on, off, total=None, phase=0.0, closed=False):
    """The dashes of a dashed stroke, as open polylines. `total`: the SVG pathLength (the pattern is measured in it,
    so that a whole number of dashes fits a 9-slice tile)."""
    seq = pts + pts[:1] if closed else pts
    real = length(seq)
    k = real / total if total else 1.0
    out, s = [], -phase * k
    while s < real:
        a, b = max(s, 0.0), min(s + on * k, real)
        if b > a + 1e-6:
            out.append(sub(seq, a, b))
        s += (on + off) * k
    return out


def mirror_x(pts, width):
    return [(width - x, y) for x, y in pts]


def shift(pts, dx=0.0, dy=0.0):
    return [(x + dx, y + dy) for x, y in pts]


def scale(pts, s, cx=0.0, cy=0.0):
    return [(cx + (x - cx) * s, cy + (y - cy) * s) for x, y in pts]
