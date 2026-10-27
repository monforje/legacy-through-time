"""The scene of one sprite: an orthographic camera straight down, a warm "lamp" sun and a soft room light, Cycles,
a transparent background. Parts are filled slabs and tubes built from point lists (blender/shapes.py) and baked
to meshes. Runs inside Blender (bpy, numpy); see render.py.

Coordinates: 1 Blender unit = 1 px of the 1x sprite. X = x, Y = -y (the SVG kit's y points down), Z toward the
camera; z = 0 is the level of the face. The view is oblique: every part is sheared, Y += shear * (z - its top),
so the top of every part lies exactly where the SVG kit draws it, and its depth shows below it (the hard sticker
shadow of the SVG kit becomes the lit and shaded wall of a real slab)."""
import math

import bmesh
import bpy
import numpy as np
from mathutils import Matrix, Vector

from shapes import dedupe, resample

SCALE = 4                                 # PNG px per 1x px, as the SVG kit (`task assets:png`)
KEY_DIR = Vector((-.45, .8, .85)).normalized()   # where the lamp is: up-left of the screen, above it
KEY_COLOR = (1.0, .9, .76)                 # a warm lamp ...
KEY = .7                                   # (its share of the light on a flat face)
FILL_COLOR = (.84, .91, 1.0)               # ... in a cooler room: warm lights, cool shadows
FILL = .34
ZENITH = (.7, .97)                         # the room is dark from this cosine to the zenith (see _light)


def _room(z):
    """Strength of the room light from a direction of height z (the cosine to the zenith)."""
    t = min(max((z - ZENITH[0]) / (ZENITH[1] - ZENITH[0]), 0), 1)
    return FILL / .8 + (FILL * .1 - FILL / .8) * t


# The light a flat face gets, per channel: the lamp plus the room over the hemisphere (2 * integral of L(z) z dz).
# Colours are divided by it, so that a flat face renders as the palette colour; the lamp tints only what turns
# toward it or away from it.
_ROOM = 2 * sum(_room((i + .5) / 400) * (i + .5) / 400 / 400 for i in range(400))
FLAT = tuple(KEY * k + _ROOM * f for k, f in zip(KEY_COLOR, FILL_COLOR))


def linear(hex_color):
    """sRGB '#rrggbb' -> linear RGB for shader inputs."""
    h = hex_color.lstrip('#')
    if len(h) == 3:
        h = ''.join(c * 2 for c in h)
    out = []
    for i in (0, 2, 4):
        c = int(h[i:i + 2], 16) / 255
        out.append(c / 12.92 if c <= .04045 else ((c + .055) / 1.055) ** 2.4)
    return tuple(out)


def albedo(hex_color):
    """The surface colour that renders as `hex_color` on a flat face under this light."""
    return tuple(min(c / f, 1.0) for c, f in zip(linear(hex_color), FLAT))


def signed_area(pts):
    return sum(x0 * y1 - x1 * y0 for (x0, y0), (x1, y1) in zip(pts, pts[1:] + pts[:1])) / 2


class Studio:
    def __init__(self, width, height, shear=1.0, period=None, samples=160):
        """`period`: (px, py) of the paper grain, the size of the tile of a tiled sprite: the grain then repeats
        without a seam. `shear`: px of drop per px of depth."""
        bpy.ops.wm.read_factory_settings(use_empty=True)
        self.w, self.h, self.k, self.period = width, height, shear, period
        self.scene = bpy.context.scene
        self.materials = {}
        self._camera()
        self._light()
        self._render_settings(samples)

    # ------------------------------------------------------------ scene
    def _camera(self):
        cam = bpy.data.cameras.new('camera')
        cam.type = 'ORTHO'
        cam.ortho_scale = max(self.w, self.h)
        cam.clip_start, cam.clip_end = .1, 400
        ob = bpy.data.objects.new('camera', cam)
        ob.location = (self.w / 2, -self.h / 2, 100)
        self.scene.collection.objects.link(ob)
        self.scene.camera = ob

    def _light(self):
        sun = bpy.data.lights.new('lamp', 'SUN')
        sun.color = KEY_COLOR
        sun.energy = KEY * math.pi / KEY_DIR.z       # irradiance of a flat face = KEY (x albedo)
        sun.angle = math.radians(9)                  # soft shadows of the threads and reliefs
        ob = bpy.data.objects.new('lamp', sun)
        ob.rotation_euler = KEY_DIR.to_track_quat('Z', 'Y').to_euler()
        self.scene.collection.objects.link(ob)

        # The room is dark straight overhead (the ceiling) and light around: a flat glossy face mirrors the dark
        # zenith and keeps its colour (a uniform room greys emerald by 4 % of white); bevels mirror the bright sides.
        world = bpy.data.worlds.new('room')
        world.use_nodes = True
        nt = world.node_tree
        bg = next(n for n in nt.nodes if n.type == 'BACKGROUND')
        bg.inputs['Color'].default_value = (*FILL_COLOR, 1)
        coord = nt.nodes.new('ShaderNodeTexCoord')
        sep = nt.nodes.new('ShaderNodeSeparateXYZ')
        ramp = nt.nodes.new('ShaderNodeMapRange')
        ramp.inputs['From Min'].default_value, ramp.inputs['From Max'].default_value = ZENITH
        ramp.inputs['To Min'].default_value, ramp.inputs['To Max'].default_value = _room(0), _room(1)
        nt.links.new(coord.outputs['Generated'], sep.inputs[0])
        nt.links.new(sep.outputs['Z'], ramp.inputs['Value'])
        nt.links.new(ramp.outputs['Result'], bg.inputs['Strength'])
        self.scene.world = world

    def _render_settings(self, samples):
        sc, r = self.scene, self.scene.render
        try:
            r.engine = 'CYCLES'
        except TypeError as e:
            raise SystemExit(f'Cycles is not available: {e}')
        sc.cycles.device = 'CPU'
        sc.cycles.samples = samples
        sc.cycles.use_denoising = True
        sc.cycles.max_bounces = 6
        sc.cycles.caustics_reflective = sc.cycles.caustics_refractive = False
        r.film_transparent = True
        r.resolution_x, r.resolution_y = self.w * SCALE, self.h * SCALE
        r.resolution_percentage = 100
        r.image_settings.file_format = 'PNG'
        r.image_settings.color_mode = 'RGBA'
        r.image_settings.color_depth = '8'
        try:
            sc.view_settings.view_transform = 'Standard'   # the palette must come out as the palette, not AgX
        except TypeError:
            pass
        sc.view_settings.look = 'None'
        sc.view_settings.exposure = 0
        sc.view_settings.gamma = 1

    def render(self, path):
        self.scene.render.filepath = str(path)
        bpy.ops.render.render(write_still=True)

    # ------------------------------------------------------------ materials
    def mat(self, kind, color='#ffffff', **o):
        key = (kind, color, tuple(sorted(o.items())))
        if key in self.materials:
            return self.materials[key]
        m = bpy.data.materials.new(f'{kind}{color}')
        m.use_nodes = True
        nt = m.node_tree
        bsdf = next(n for n in nt.nodes if n.type == 'BSDF_PRINCIPLED')
        i = bsdf.inputs
        i['Base Color'].default_value = (*albedo(color), 1)
        if kind == 'enamel':           # glassy enamel of the buttons: a clear coat over a satin colour
            i['Roughness'].default_value = o.get('rough', .4)
            i['Coat Weight'].default_value = o.get('coat', .45)
            i['Coat Roughness'].default_value = .08
            i['Specular IOR Level'].default_value = .3
        elif kind == 'lacquer':        # the black-green ink rims and backs: dark lacquer
            i['Roughness'].default_value = o.get('rough', .5)
            i['Coat Weight'].default_value = .2
            i['Coat Roughness'].default_value = .15
            i['Specular IOR Level'].default_value = .25
        elif kind == 'metal':          # gilt wires
            i['Metallic'].default_value = 1
            i['Roughness'].default_value = o.get('rough', .3)
        elif kind == 'bronze':         # the coin: half metal, a soft struck sheen rather than a mirror
            i['Metallic'].default_value = .5
            i['Roughness'].default_value = .42
            i['Coat Weight'].default_value = .2
        elif kind == 'thread':         # silk and cotton threads: a soft sheen
            i['Roughness'].default_value = .62
            i['Sheen Weight'].default_value = o.get('sheen', .6)
            i['Sheen Roughness'].default_value = .35
            i['Specular IOR Level'].default_value = .3
        elif kind == 'satin':          # the ribbon of the stat banner
            i['Roughness'].default_value = .4
            i['Sheen Weight'].default_value = 1
            i['Sheen Roughness'].default_value = .25
        elif kind == 'paper':
            i['Roughness'].default_value = .9
            i['Specular IOR Level'].default_value = .2
            i['Sheen Weight'].default_value = .25
            self._grain(nt, bsdf, color, o.get('grain', 1.5))
        elif kind == 'matte':          # the white glyph masks
            i['Roughness'].default_value = .55
            i['Specular IOR Level'].default_value = .3
        else:
            raise ValueError(kind)
        self.materials[key] = m
        return m

    def _grain(self, nt, bsdf, color, amount):
        """Paper grain and fibres: two noises on the position, tinting the colour and bumping the normal. With a
        period, the noise runs on a torus (x and y wrapped into circles of that length in 4D noise space), so the
        grain repeats exactly once per tile."""
        n = nt.nodes
        geo = n.new('ShaderNodeNewGeometry')
        sep = n.new('ShaderNodeSeparateXYZ')
        nt.links.new(geo.outputs['Position'], sep.inputs[0])

        def math_node(op, a, b=None):
            node = n.new('ShaderNodeMath')
            node.operation = op
            for k, v in enumerate((a, b)):
                if v is None:
                    continue
                if isinstance(v, (int, float)):
                    node.inputs[k].default_value = v
                else:
                    nt.links.new(v, node.inputs[k])
            return node.outputs[0]

        def noise(scale, detail, seed):
            tex = n.new('ShaderNodeTexNoise')
            tex.inputs['Scale'].default_value = scale
            tex.inputs['Detail'].default_value = detail
            tex.inputs['Roughness'].default_value = .55
            if self.period:
                px, py = self.period
                tex.noise_dimensions = '4D'
                u = math_node('MULTIPLY', sep.outputs['X'], math.tau / px)
                v = math_node('MULTIPLY', sep.outputs['Y'], math.tau / py)
                rx, ry = px / math.tau, py / math.tau      # radius: 1 noise unit = 1 px along the sprite
                vec = n.new('ShaderNodeCombineXYZ')
                nt.links.new(math_node('MULTIPLY', math_node('COSINE', u), rx), vec.inputs[0])
                nt.links.new(math_node('MULTIPLY', math_node('SINE', u), rx), vec.inputs[1])
                nt.links.new(math_node('ADD', math_node('MULTIPLY', math_node('COSINE', v), ry), seed), vec.inputs[2])
                nt.links.new(vec.outputs[0], tex.inputs['Vector'])
                nt.links.new(math_node('MULTIPLY', math_node('SINE', v), ry), tex.inputs['W'])
            else:
                tex.noise_dimensions = '3D'
                off = n.new('ShaderNodeVectorMath')
                off.operation = 'ADD'
                off.inputs[1].default_value = (seed, seed * 1.7, 0)
                nt.links.new(geo.outputs['Position'], off.inputs[0])
                nt.links.new(off.outputs[0], tex.inputs['Vector'])
            return tex.outputs['Fac']

        fine = noise(1.25, 3, 0)           # grain: ~1 px
        fibre = noise(.3, 2, 37)           # cloudy fibres: ~4 px
        # brightness 1 +- a few per cent
        b = math_node('ADD', math_node('MULTIPLY', math_node('SUBTRACT', fine, .5), .13 * amount),
                      math_node('MULTIPLY', math_node('SUBTRACT', fibre, .5), .1 * amount))
        b = math_node('ADD', b, 1)
        tint = n.new('ShaderNodeVectorMath')
        tint.operation = 'SCALE'
        tint.inputs[0].default_value = albedo(color)
        nt.links.new(b, tint.inputs['Scale'])
        nt.links.new(tint.outputs[0], bsdf.inputs['Base Color'])
        bump = n.new('ShaderNodeBump')
        bump.inputs['Strength'].default_value = .35 * amount
        bump.inputs['Distance'].default_value = .25
        nt.links.new(math_node('ADD', fine, math_node('MULTIPLY', fibre, .6)), bump.inputs['Height'])
        nt.links.new(bump.outputs['Normal'], bsdf.inputs['Normal'])

    # ------------------------------------------------------------ parts
    def _spline(self, cu, pts, closed, zs=None):
        if zs is None:
            pts = dedupe(pts, closed)
        sp = cu.splines.new('POLY')
        sp.points.add(len(pts) - 1)
        for k, (p, (x, y)) in enumerate(zip(sp.points, pts)):
            p.co = (x, -y, zs[k] if zs else 0, 1)
        sp.use_cyclic_u = closed
        sp.use_smooth = True
        return sp

    def slab(self, pts, z0, z1, mat, bevel=.5, holes=(), anchor=None):
        """A filled outline from z0 to z1 with a round bevel `bevel` px wide along its top and bottom edges; the
        outline is exact (the bevel does not grow it). A bevel wider than half the thickness is squashed into an
        elliptic roll (the soft cushion of the enamel). `holes`: outlines cut out of it."""
        cu = bpy.data.curves.new('slab', 'CURVE')
        cu.dimensions = '2D'
        cu.fill_mode = 'BOTH'
        bevel = max(bevel, 0)
        half = (z1 - z0) / 2
        squash = min(1.0, half / (bevel + 1e-3)) if bevel else 1.0
        cu.bevel_depth = bevel
        cu.bevel_resolution = 6
        cu.extrude = max(half / squash - bevel, 0)
        cu.offset = -bevel
        # The offset moves an outline along its normals: the outer one must turn one way, the holes the other.
        for k, outline in enumerate([pts, *holes]):
            blender = [(x, -y) for x, y in outline]
            ccw = signed_area(blender) > 0
            if ccw != (k == 0):
                outline = outline[::-1]
            self._spline(cu, outline, True)
        ob = bpy.data.objects.new('slab', cu)
        ob.location.z = (z0 + z1) / 2
        ob.scale.z = squash
        return self._bake(ob, mat, anchor)

    def tube(self, pts, r, mat, z=0.0, closed=False, flat=.6, caps='round', twist=None, zs=None, anchor=None):
        """A round cord along the line, its section flattened to `flat` of its width (a thread lying on paper).
        caps: 'round' (stroke-linecap round), 'flat' or None. twist: (pitch, lobes) - a twisted thread: the section
        has `lobes` lobes and turns so that a ridge passes every `pitch` px (a tile must be a whole number of them).
        zs: the height of every point instead of z (the over-and-under of the knot)."""
        if zs is None:
            pts = dedupe(pts, closed)
        if twist:
            pts = resample(pts, .2, closed)
            zs = None                                   # no sprite twists a thread that goes up and down
        cu = bpy.data.curves.new('tube', 'CURVE')
        cu.dimensions = '3D'
        cu.fill_mode = 'FULL'
        cu.twist_mode = 'Z_UP'
        cu.use_fill_caps = caps is not None
        if twist:
            pitch, lobes = twist
            cu.bevel_mode = 'OBJECT'
            cu.bevel_object = self._profile(r, lobes)
        else:
            cu.bevel_mode = 'ROUND'
            cu.bevel_depth = r
            cu.bevel_resolution = 5
        local = [h / flat for h in zs] if zs else [z / flat] * len(pts)
        sp = self._spline(cu, pts, closed, local)
        if twist:
            s = 0.0
            for k, p in enumerate(sp.points):
                if k:
                    s += math.dist(pts[k - 1], pts[k])
                p.tilt = math.tau / lobes * s / pitch
        ob = bpy.data.objects.new('tube', cu)
        ob.scale = (1, 1, flat)
        out = [self._bake(ob, mat, anchor)]
        if caps == 'round' and not closed:
            for k in (0, -1):
                h = zs[k] if zs else z
                out.append(self.ball(pts[k][0], pts[k][1], h, r, mat, flat, anchor=anchor if anchor is not None else
                                     (max(zs) if zs else z) + r * flat))
        return out

    def _profile(self, r, lobes, depth=.2):
        """The section of a twisted thread: a circle with `lobes` soft ridges."""
        cu = bpy.data.curves.new('profile', 'CURVE')
        cu.dimensions = '2D'
        n = 48
        pts = []
        for i in range(n):
            a = math.tau * i / n
            rr = r * (1 - depth / 2 + depth / 2 * math.cos(lobes * a))
            pts.append((rr * math.cos(a), -rr * math.sin(a)))
        self._spline(cu, pts, True)
        ob = bpy.data.objects.new('profile', cu)
        self.scene.collection.objects.link(ob)
        ob.hide_render = True
        return ob

    def ball(self, x, y, z, r, mat, flat=1.0, anchor=None):
        """A sphere (a dome when flattened): the round caps of the tubes, beads, knobs."""
        me = bpy.data.meshes.new('ball')
        bm = bmesh.new()
        bmesh.ops.create_uvsphere(bm, u_segments=40, v_segments=20, radius=r)
        bm.to_mesh(me)
        bm.free()
        for p in me.polygons:
            p.use_smooth = True
        ob = bpy.data.objects.new('ball', me)
        ob.location = (x, -y, z)
        ob.scale = (1, 1, flat)
        return self._bake(ob, mat, anchor)

    def _bake(self, ob, mat, anchor=None):
        """The evaluated part as a mesh in world space, sheared (see the module doc) and given its material."""
        self.scene.collection.objects.link(ob)
        dg = bpy.context.evaluated_depsgraph_get()
        me = bpy.data.meshes.new_from_object(ob.evaluated_get(dg))
        me.transform(ob.matrix_world)
        data = ob.data
        bpy.data.objects.remove(ob)
        if isinstance(data, bpy.types.Curve):
            bpy.data.curves.remove(data)
        top = max((v.co.z for v in me.vertices), default=0) if anchor is None else anchor
        me.transform(Matrix(((1, 0, 0, 0), (0, 1, self.k, -self.k * top), (0, 0, 1, 0), (0, 0, 0, 1))))
        me.materials.append(mat)
        out = bpy.data.objects.new('part', me)
        self.scene.collection.objects.link(out)
        return out


# ---------------------------------------------------------------- after the render
def flatten(block, axis):
    """Every line of the block along `axis` that is uniform but for render noise becomes exactly its mean; a line
    that crosses a real feature (the slanted end of a button in its side border) is kept."""
    mean = block.mean(axis=axis, keepdims=True)
    spread = np.abs(block - mean).max(axis=axis, keepdims=True).max(axis=-1, keepdims=True)
    return np.where(spread < 6 / 255, np.broadcast_to(mean, block.shape), block)


def finish(raw, out, slice_px=None, sliced=False, mask=False, halo='#231a17'):
    """The rendered PNG -> the sprite. `sliced`: the edges and the centre of a 9-slice sprite are made exactly
    uniform along their stretch (render noise in them would stretch into streaks). `mask`: a glyph the game
    tints: grey shading normalised to white. `halo`: the colour of the transparent pixels (mip-maps blend it into
    the edges, as the bed of the SVG kit)."""
    img = bpy.data.images.load(str(raw))
    w, h = img.size
    a = np.empty(w * h * 4, np.float32)
    img.pixels.foreach_get(a)
    bpy.data.images.remove(img)
    a = a.reshape(h, w, 4)[::-1].copy()              # top row first

    if mask:
        lum = a[..., 0] * .2126 + a[..., 1] * .7152 + a[..., 2] * .0722
        solid = lum[a[..., 3] > .9]
        ref = np.percentile(solid, 92) if solid.size else 1
        g = np.clip(lum / max(ref, 1e-3), 0, 1)
        a[..., 0] = a[..., 1] = a[..., 2] = g

    if sliced and slice_px:
        left, bottom, right, top = slice_px
        pm = a.copy()
        pm[..., :3] *= pm[..., 3:4]
        x0, x1, y0, y1 = left, w - right, top, h - bottom
        if x1 > x0:
            for rows in (slice(0, y0), slice(y1, h)):
                pm[rows, x0:x1] = flatten(pm[rows, x0:x1], axis=1)
        if y1 > y0:
            for cols in (slice(0, x0), slice(x1, w)):
                pm[y0:y1, cols] = flatten(pm[y0:y1, cols], axis=0)
        if x1 > x0 and y1 > y0:
            pm[y0:y1, x0:x1] = flatten(flatten(pm[y0:y1, x0:x1], axis=1), axis=0)
        alpha = pm[..., 3:4]
        a = np.concatenate([np.where(alpha > 1e-6, pm[..., :3] / np.maximum(alpha, 1e-6), 0), alpha], axis=-1)

    empty = a[..., 3] < .5 / 255
    hx = halo.lstrip('#')
    hx = ''.join(c * 2 for c in hx) if len(hx) == 3 else hx
    a[empty, :3] = [int(hx[i:i + 2], 16) / 255 for i in (0, 2, 4)]
    a[empty, 3] = 0

    res = bpy.data.images.new('sprite', w, h, alpha=True)
    res.alpha_mode = 'STRAIGHT'
    res.pixels.foreach_set(np.clip(a[::-1], 0, 1).ravel())
    res.filepath_raw = str(out)
    res.file_format = 'PNG'
    res.save()
    bpy.data.images.remove(res)
