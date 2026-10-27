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


# ---------------------------------------------------------------- the richer kit: shapes and ornaments
def cartouche(x0, y0, x1, y1, r):
    """A rectangle with its corners cut in by quarter circles of radius r (concave corners): the plaque of the
    buttons and tags. Straight sides, so it tiles along both axes."""
    return (arc(x0, y0, r, .5 * math.pi, 0)[:-1] + arc(x1, y0, r, math.pi, .5 * math.pi)[:-1]
            + arc(x1, y1, r, 1.5 * math.pi, math.pi)[:-1] + arc(x0, y1, r, 2 * math.pi, 1.5 * math.pi)[:-1])


def stroke(pts, r):
    """The outline of an open polyline drawn with a pen of radius r, round caps and round joins: a closed outline
    that a slab can fill (the stamped strokes of the glyphs)."""
    pts = dedupe(pts, closed=False)
    n = len(pts)
    seg = [((pts[i + 1][0] - pts[i][0]) / dist(pts[i], pts[i + 1]), (pts[i + 1][1] - pts[i][1]) / dist(pts[i], pts[i + 1]))
           for i in range(n - 1)]

    def normal(e):
        return -e[1], e[0]

    def side(sign):
        out = []
        for i in range(n):
            p = pts[i]
            if i == 0 or i == n - 1:
                e = seg[0] if i == 0 else seg[-1]
                nx, ny = normal(e)
                out.append((p[0] + sign * nx * r, p[1] + sign * ny * r))
                continue
            e1, e2 = seg[i - 1], seg[i]
            n1, n2 = normal(e1), normal(e2)
            cross = e1[0] * e2[1] - e1[1] * e2[0]
            if abs(cross) < 1e-6:
                out.append((p[0] + sign * n1[0] * r, p[1] + sign * n1[1] * r))
            elif cross * sign < 0:                  # this side is the outer one of the turn: a round join
                a1 = math.atan2(sign * n1[1], sign * n1[0])
                a2 = math.atan2(sign * n2[1], sign * n2[0])
                delta = (a2 - a1 + math.pi) % math.tau - math.pi
                out += arc(p[0], p[1], r, a1, a1 + delta, max(2, int(abs(delta) * r / STEP)))
            else:                                   # the inner one: the offset lines meet
                m = 1 + n1[0] * n2[0] + n1[1] * n2[1]
                out.append((p[0] + sign * (n1[0] + n2[0]) / m * r, p[1] + sign * (n1[1] + n2[1]) / m * r))
        return out

    def cap(p, e, start):
        nx, ny = normal(e)
        a = math.atan2(ny, nx) if not start else math.atan2(-ny, -nx)
        return arc(p[0], p[1], r, a, a - math.pi)[1:-1]

    left, right = side(1), side(-1)
    return left + cap(pts[-1], seg[-1], False) + right[::-1] + cap(pts[0], seg[0], True)


def ring(pts, r):
    """A closed line drawn with a pen of radius r: (outer outline, inner outline) for a slab with a hole."""
    return offset(pts, r), offset(pts, -r)


def spiral(cx, cy, r0, r1, turns, a0=0.0, cw=True):
    """An Archimedean spiral from radius r0 to r1 (a filigree scroll), `turns` turns starting at angle a0."""
    total = turns * math.tau
    n = max(16, int(total * max(r0, r1) / STEP))
    sign = 1 if cw else -1
    return [(cx + (r0 + (r1 - r0) * k / n) * math.cos(a0 + sign * total * k / n),
             cy + (r0 + (r1 - r0) * k / n) * math.sin(a0 + sign * total * k / n)) for k in range(n + 1)]


def tulip(cx, cy, s, angle=0.0):
    """The tulip of the Tatar ornament: three petals, the middle one pointed, s = height, standing on (cx, cy) and
    pointing up when angle = 0 (radians, clockwise on screen)."""
    raw = (bezier((0, -1), (.17, -.8), (.24, -.6), (.2, -.36)) + bezier((.2, -.36), (.32, -.5), (.44, -.66), (.5, -.8))[1:]
           + bezier((.5, -.8), (.58, -.42), (.42, -.04), (0, 0))[1:] + bezier((0, 0), (-.42, -.04), (-.58, -.42), (-.5, -.8))[1:]
           + bezier((-.5, -.8), (-.44, -.66), (-.32, -.5), (-.2, -.36))[1:] + bezier((-.2, -.36), (-.24, -.6), (-.17, -.8), (0, -1))[1:-1])
    c, sn = math.cos(angle), math.sin(angle)
    return [(cx + (x * c - y * sn) * s, cy + (x * sn + y * c) * s) for x, y in raw]


def along(pts, spacing, phase=0.0, closed=False):
    """Points every `spacing` px along the line, the first at `phase` (beads, stitches)."""
    seq = pts + pts[:1] if closed else pts
    out, s, target = [], 0.0, phase
    for a, b in zip(seq, seq[1:]):
        d = dist(a, b)
        while d > 0 and target <= s + d + 1e-9:
            out.append(lerp(a, b, (target - s) / d))
            target += spacing
        s += d
    return out


def chain(pts, stitch, width, closed=False, phase=0.0):
    """Tambour (chain) stitch along the line: one closed loop per stitch, `stitch` long and `width` wide, each
    starting inside the end of the previous one. Loops as closed outlines for thread tubes."""
    seq = pts + pts[:1] if closed else pts
    total = length(seq)
    count = max(1, int(round((total - phase) / stitch))) if closed else max(1, int((total - phase) // stitch))
    step = (total - phase) / count if closed else stitch
    loops = []
    for k in range(count):
        s0 = phase + k * step
        a = _at(seq, s0)
        b = _at(seq, s0 + step * 1.15)              # a little longer: it reaches into the next one
        mid, d = lerp(a, b, .5), dist(a, b)
        if d < 1e-6:
            continue
        ex, ey = (b[0] - a[0]) / d, (b[1] - a[1]) / d
        n = 14
        loop = []
        for i in range(n):
            t = math.tau * i / n
            u = math.cos(t) * d / 2
            v = math.sin(t) * width / 2 * (1 - .35 * max(0.0, -math.cos(t)))   # narrower at its tail: a drop
            loop.append((mid[0] + ex * u - ey * v, mid[1] + ey * u + ex * v))
        loops.append(loop)
    return loops


def _at(seq, s):
    """The point at arc length s of a polyline (clamped)."""
    acc = 0.0
    for a, b in zip(seq, seq[1:]):
        d = dist(a, b)
        if acc + d >= s and d > 0:
            return lerp(a, b, (s - acc) / d)
        acc += d
    return seq[-1]


def hatch(poly, spacing, angle):
    """Parallel segments filling the polygon (satin stitch): lines at `angle` (radians) every `spacing` px,
    clipped to the polygon by the even-odd rule."""
    c, s = math.cos(angle), math.sin(angle)
    rot = [(x * c + y * s, -x * s + y * c) for x, y in poly]      # into a frame where the lines are horizontal
    ys = [p[1] for p in rot]
    out = []
    y = min(ys) + spacing / 2
    while y < max(ys):
        xs = []
        for (x0, y0), (x1, y1) in zip(rot, rot[1:] + rot[:1]):
            if (y0 <= y < y1) or (y1 <= y < y0):
                xs.append(x0 + (y - y0) * (x1 - x0) / (y1 - y0))
        xs.sort()
        for a, b in zip(xs[0::2], xs[1::2]):
            if b - a > .3:
                out.append([(a * c - y * s, a * s + y * c), (b * c - y * s, b * s + y * c)])
        y += spacing
    return out


def transform(pts, dx=0.0, dy=0.0, s=1.0, angle=0.0, mirror=False):
    """Scale, rotate (clockwise on screen), mirror in x and move a motif drawn around (0, 0)."""
    c, sn = math.cos(angle), math.sin(angle)
    out = []
    for x, y in pts:
        x = -x if mirror else x
        out.append((dx + (x * c - y * sn) * s, dy + (x * sn + y * c) * s))
    return out
