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
    return FILL / .8 + (FILL * .55 - FILL / .8) * t


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


class Nodes:
    """Shader node plumbing for the materials. Every texture samples the position in px; with a period (the tile of
    a tiled sprite) x and y are wrapped into circles of that length in 4D texture space, so a texture repeats
    exactly once per tile and the round tiles of KitImage join without a seam."""

    def __init__(self, nt, period):
        self.nt, self.n, self.period = nt, nt.nodes, period
        geo = self.n.new('ShaderNodeNewGeometry')
        self.pos = geo.outputs['Position']
        sep = self.n.new('ShaderNodeSeparateXYZ')
        self.link(self.pos, sep.inputs[0])
        self.x, self.y = sep.outputs['X'], sep.outputs['Y']

    def link(self, a, b):
        self.nt.links.new(a, b)

    def math(self, op, a, b=None):
        node = self.n.new('ShaderNodeMath')
        node.operation = op
        for k, v in enumerate((a, b)):
            if v is None:
                continue
            if isinstance(v, (int, float)):
                node.inputs[k].default_value = v
            else:
                self.link(v, node.inputs[k])
        return node.outputs[0]

    def mul(self, a, b):
        return self.math('MULTIPLY', a, b)

    def add(self, a, b):
        return self.math('ADD', a, b)

    def _place(self, tex, seed, dimensions):
        """Sets the dimensions of a texture node (its W input exists only in 4D) and feeds the (wrapped) position."""
        setattr(tex, dimensions, '4D' if self.period else '3D')
        if self.period:
            px, py = self.period
            u, v = self.mul(self.x, math.tau / px), self.mul(self.y, math.tau / py)
            rx, ry = px / math.tau, py / math.tau      # 1 texture unit = 1 px along the sprite
            vec = self.n.new('ShaderNodeCombineXYZ')
            self.link(self.mul(self.math('COSINE', u), rx), vec.inputs[0])
            self.link(self.mul(self.math('SINE', u), rx), vec.inputs[1])
            self.link(self.add(self.mul(self.math('COSINE', v), ry), seed), vec.inputs[2])
            self.link(vec.outputs[0], tex.inputs['Vector'])
            self.link(self.mul(self.math('SINE', v), ry), tex.inputs['W'])
            return
        off = self.n.new('ShaderNodeVectorMath')
        off.operation = 'ADD'
        off.inputs[1].default_value = (seed, seed * 1.7, 0)
        self.link(self.pos, off.inputs[0])
        self.link(off.outputs[0], tex.inputs['Vector'])

    def noise(self, scale, detail, seed):
        """Value noise 0..1 with features of about 1/scale px."""
        tex = self.n.new('ShaderNodeTexNoise')
        self._place(tex, seed, 'noise_dimensions')
        tex.inputs['Scale'].default_value = scale
        tex.inputs['Detail'].default_value = detail
        tex.inputs['Roughness'].default_value = .55
        return tex.outputs['Fac']

    def voronoi(self, scale, seed, out='Distance'):
        """Cells of about 1/scale px: their distance field is a pebbled grain."""
        tex = self.n.new('ShaderNodeTexVoronoi')
        self._place(tex, seed, 'voronoi_dimensions')
        tex.inputs['Scale'].default_value = scale
        return tex.outputs[out]

    def wave(self, scale, distortion):
        """Bands across y (wood grain), wobbling: not periodic (nothing tiled is made of wood)."""
        tex = self.n.new('ShaderNodeTexWave')
        tex.wave_type = 'BANDS'
        tex.bands_direction = 'Y'
        tex.inputs['Scale'].default_value = scale
        tex.inputs['Distortion'].default_value = distortion
        tex.inputs['Detail'].default_value = 3
        self.link(self.pos, tex.inputs['Vector'])
        return tex.outputs['Fac']

    def colour(self, bsdf, color, variations, ao=None, dark=None):
        """Base colour: the albedo of `color` times 1 + sum(amount * (value - .5)), darkened toward `dark` (or
        black) where ambient occlusion within ao[0] px finds a crevice, by up to ao[1]."""
        b = 1.0
        for value, amount in variations:
            b = self.add(b, self.mul(self.math('SUBTRACT', value, .5), amount))
        tint = self.n.new('ShaderNodeVectorMath')
        tint.operation = 'SCALE'
        tint.inputs[0].default_value = albedo(color)
        if isinstance(b, float):
            tint.inputs['Scale'].default_value = b
        else:
            self.link(b, tint.inputs['Scale'])
        out = tint.outputs[0]
        if ao:
            occ = self.n.new('ShaderNodeAmbientOcclusion')
            occ.samples = 8
            occ.inputs['Distance'].default_value = ao[0]
            mix = self.n.new('ShaderNodeMix')
            mix.data_type = 'RGBA'
            # the Mix node keeps sockets of every type under the same names: take the colour ones by identifier
            sock = {s.identifier: s for s in mix.inputs}
            self.link(self.mul(self.math('SUBTRACT', 1.0, occ.outputs['AO']), ao[1]), sock['Factor_Float'])
            self.link(out, sock['A_Color'])
            sock['B_Color'].default_value = (*albedo(dark), 1) if dark else (0, 0, 0, 1)
            out = next(s for s in mix.outputs if s.identifier == 'Result_Color')
        self.link(out, bsdf.inputs['Base Color'])

    def bump(self, bsdf, heights, strength, distance):
        h = 0.0
        for value, amount in heights:
            h = self.add(h, self.mul(value, amount))
        node = self.n.new('ShaderNodeBump')
        node.inputs['Distance'].default_value = distance
        # only where the surface faces the camera: on a steep bevel the grain catches the bright room and sparkles
        geo = self.n.new('ShaderNodeNewGeometry')
        nz = self.n.new('ShaderNodeSeparateXYZ')
        self.link(geo.outputs['Normal'], nz.inputs[0])
        flat = self.n.new('ShaderNodeMapRange')
        flat.inputs['From Min'].default_value, flat.inputs['From Max'].default_value = .75, .97
        flat.inputs['To Min'].default_value, flat.inputs['To Max'].default_value = 0, strength
        self.link(nz.outputs['Z'], flat.inputs['Value'])
        self.link(flat.outputs['Result'], node.inputs['Strength'])
        self.link(h, node.inputs['Height'])
        self.link(node.outputs['Normal'], bsdf.inputs['Normal'])


class Studio:
    def __init__(self, width, height, shear=1.0, period=None, samples=160):
        """`period`: (px, py) of the paper grain, the size of the tile of a tiled sprite: the grain then repeats
        without a seam. `shear`: px of drop per px of depth."""
        bpy.ops.wm.read_factory_settings(use_empty=True)
        self.w, self.h, self.k, self.period = width, height, shear, period
        self.borders = None             # the 9-slice borders of the sprite (render.py): the lattices of its patterns
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
        """A material of the kit. Textures are periodic with the tile of a tiled sprite (see Nodes)."""
        key = (kind, color, tuple(sorted(o.items())))
        if key in self.materials:
            return self.materials[key]
        m = bpy.data.materials.new(f'{kind}{color}')
        m.use_nodes = True
        nt = m.node_tree
        bsdf = next(n for n in nt.nodes if n.type == 'BSDF_PRINCIPLED')
        i = bsdf.inputs
        i['Base Color'].default_value = (*albedo(color), 1)
        g = Nodes(nt, self.period)
        set_ = lambda name, v: setattr(i[name], 'default_value', v)  # noqa: E731
        if kind == 'leather':          # tooled leather of the buttons, tags and seals: pebbled grain, worn crevices
            pebble = g.voronoi(1.1, 3)
            mottle = g.noise(.12, 3, 11)
            fine = g.noise(2.2, 2, 5)
            g.colour(bsdf, color, [(pebble, .1), (mottle, .12), (fine, .05)], ao=(1.6, .5))
            g.link(g.add(g.mul(pebble, .14), .48), i['Roughness'])
            g.bump(bsdf, [(pebble, 1.0), (fine, .35)], strength=.5, distance=.3)
            set_('Sheen Weight', .1); set_('Sheen Roughness', .4); set_('Specular IOR Level', .4)
            set_('Coat Weight', o.get('coat', .15)); set_('Coat Roughness', .2)
        elif kind == 'gold':           # antique gold: a soft hammered sheen, patina in the crevices
            ham = g.noise(.9, 3, 23)
            g.colour(bsdf, color, [(ham, .08)], ao=(1.2, .45), dark='#4a2f12')
            g.link(g.add(g.mul(ham, .16), o.get('rough', .2)), i['Roughness'])
            g.bump(bsdf, [(ham, 1.0)], strength=.12, distance=.2)
            set_('Metallic', 1)
        elif kind == 'gauze':          # the Voice's veil: thin silk you see the night through, its weave catches light
            weave = g.voronoi(2.6, 41)
            drift = g.noise(.09, 2, 53)
            g.colour(bsdf, color, [(weave, .06), (drift, .1)])
            set_('Roughness', .7); set_('Sheen Weight', 1); set_('Sheen Roughness', .25); set_('Specular IOR Level', .2)
            g.link(g.add(g.mul(drift, .16), o.get('alpha', .45) - .08), i['Alpha'])
            g.bump(bsdf, [(weave, 1.0)], strength=.2, distance=.15)
        elif kind == 'parchment':      # paper: fibres, cloudy stains, flecks, darker where it is shaded
            fine = g.noise(1.4, 3, 0)
            fibre = g.noise(.35, 4, 37)
            stain = g.noise(.07, 2, 71)
            fleck = g.voronoi(.45, 91, out='Distance')
            g.colour(bsdf, color, [(fine, .1 * o.get('grain', 1)), (fibre, .1), (stain, .14), (fleck, .05)], ao=(2.0, .35))
            set_('Roughness', .88); set_('Specular IOR Level', .2); set_('Sheen Weight', .2)
            g.bump(bsdf, [(fine, 1.0), (fibre, .7)], strength=.45, distance=.25)
        elif kind == 'velvet':         # the ribbon of the stat banner: velvet, its pile lit at the edges
            pile = g.noise(1.8, 2, 13)
            g.colour(bsdf, color, [(pile, .08)], ao=(1.4, .4))
            set_('Roughness', .85); set_('Sheen Weight', 1); set_('Sheen Roughness', .3); set_('Specular IOR Level', .2)
            g.bump(bsdf, [(pile, 1.0)], strength=.25, distance=.2)
        elif kind == 'wood':           # the chest: wood grain in bands, darker in the cracks
            grain = g.wave(.18, 6)
            fine = g.noise(1.2, 2, 29)
            g.colour(bsdf, color, [(grain, .22), (fine, .08)], ao=(1.4, .5))
            set_('Roughness', .62); set_('Specular IOR Level', .3); set_('Coat Weight', .2)
            g.bump(bsdf, [(grain, .6), (fine, .4)], strength=.3, distance=.3)
        elif kind == 'gem':            # rubies and emeralds: facets mirror the lamp
            set_('Roughness', .04); set_('Coat Weight', 1); set_('Coat Roughness', .02); set_('Specular IOR Level', 1)
        elif kind == 'pearl':
            set_('Roughness', .28); set_('Coat Weight', .7); set_('Coat Roughness', .1)
            set_('Thin Film Thickness', 380); set_('Thin Film IOR', 1.5)
        elif kind == 'wax':            # the seal of the info cloud
            lump = g.noise(.5, 3, 17)
            g.colour(bsdf, color, [(lump, .1)], ao=(1.5, .45))
            set_('Roughness', .3); set_('Coat Weight', .5); set_('Subsurface Weight', .15)
            g.bump(bsdf, [(lump, 1.0)], strength=.2, distance=.3)
        elif kind == 'enamel':         # small enamel inlays
            set_('Roughness', o.get('rough', .3)); set_('Coat Weight', o.get('coat', .6)); set_('Coat Roughness', .05)
            set_('Specular IOR Level', .4)
        elif kind == 'thread':         # silk and cotton threads: a soft sheen, darker where they dive in
            g.colour(bsdf, color, [], ao=(1.0, .45))
            set_('Roughness', .6); set_('Sheen Weight', o.get('sheen', .7)); set_('Sheen Roughness', .35)
            set_('Specular IOR Level', .3)
        elif kind == 'lacquer':        # dark edges and backs
            g.colour(bsdf, color, [(g.noise(.8, 2, 3), .1)], ao=(1.5, .5))
            set_('Roughness', o.get('rough', .55)); set_('Coat Weight', .15); set_('Specular IOR Level', .25)
        elif kind == 'matte':          # the white glyph masks
            set_('Roughness', .5); set_('Specular IOR Level', .3)
        else:
            raise ValueError(kind)
        self.materials[key] = m
        return m

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
        if twist:                                       # dense points: the tilt turns between them
            pts, zs = resample_z(pts, zs, .2, closed)
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

    def tubes(self, paths, r, mat, z=0.0, closed=False, flat=.6, zs=None, anchor=None):
        """Many cords of one material in one object (stitches, loops of a chain, hatching): flat caps.
        zs: per path, the heights of its points."""
        cu = bpy.data.curves.new('tubes', 'CURVE')
        cu.dimensions = '3D'
        cu.fill_mode = 'FULL'
        cu.twist_mode = 'Z_UP'
        cu.use_fill_caps = not closed
        cu.bevel_mode = 'ROUND'
        cu.bevel_depth = r
        cu.bevel_resolution = 3
        for k, pts in enumerate(paths):
            if len(pts) < 2:
                continue
            heights = [h / flat for h in zs[k]] if zs else [z / flat] * len(pts)
            self._spline(cu, pts, closed, heights)
        ob = bpy.data.objects.new('tubes', cu)
        ob.scale = (1, 1, flat)
        return self._bake(ob, mat, anchor if anchor is not None else z + r * flat)

    def beads(self, points, r, mat, z=0.0, flat=.8, anchor=None):
        """Many small spheres in one object: granulation, pearls, studs."""
        me = bpy.data.meshes.new('beads')
        bm = bmesh.new()
        for x, y in points:
            geom = bmesh.ops.create_uvsphere(bm, u_segments=16, v_segments=8, radius=r)
            bmesh.ops.scale(bm, vec=(1, 1, flat), verts=geom['verts'])
            bmesh.ops.translate(bm, vec=(x, -y, z), verts=geom['verts'])
        bm.to_mesh(me)
        bm.free()
        for p in me.polygons:
            p.use_smooth = True
        return self._bake(bpy.data.objects.new('beads', me), mat, anchor if anchor is not None else z + r * flat)

    def facet(self, outline, z0, z1, mat, table=.45, girdle=.25, anchor=None):
        """A cut stone: a short girdle on the outline, a crown of flat facets up to a table (the outline shrunk to
        `table` around its centre). Flat shading: every facet mirrors the lamp on its own."""
        cx = sum(p[0] for p in outline) / len(outline)
        cy = sum(p[1] for p in outline) / len(outline)
        bm = bmesh.new()
        zg = z0 + (z1 - z0) * girdle
        low = [bm.verts.new((x, -y, z0)) for x, y in outline]
        mid = [bm.verts.new((x, -y, zg)) for x, y in outline]
        top = [bm.verts.new((cx + (x - cx) * table, -(cy + (y - cy) * table), z1)) for x, y in outline]
        n = len(outline)
        for i in range(n):
            j = (i + 1) % n
            bm.faces.new((low[i], low[j], mid[j], mid[i]))
            bm.faces.new((mid[i], mid[j], top[j], top[i]))
        bm.faces.new(top)
        bm.faces.new(low[::-1])
        bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
        me = bpy.data.meshes.new('facet')
        bm.to_mesh(me)
        bm.free()
        return self._bake(bpy.data.objects.new('facet', me), mat, anchor)

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


def resample_z(pts, zs, step, closed):
    """Points every `step` px along the line with their heights interpolated (None: no heights)."""
    seq = pts + pts[:1] if closed else pts
    hs = (zs + zs[:1] if closed else zs) if zs else [0.0] * len(seq)
    out, oz = [seq[0]], [hs[0]]
    for (a, b), (za, zb) in zip(zip(seq, seq[1:]), zip(hs, hs[1:])):
        n = max(1, int(math.ceil(math.dist(a, b) / step)))
        for i in range(1, n + 1):
            out.append((a[0] + (b[0] - a[0]) * i / n, a[1] + (b[1] - a[1]) * i / n))
            oz.append(za + (zb - za) * i / n)
    if closed:
        out, oz = out[:-1], oz[:-1]
    return out, (oz if zs else None)


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
