"""3D-иконки оболочки: строит объекты в Blender и рендерит PNG 512x512 с прозрачным фоном.

Запуск из Blender (Scripting) или через MCP:
    exec(open('<repo>/docs/brainstorm/templates/shell/assets/src/build_ui_assets.py').read())
    build(['gem', 'coin'])      # или build() для всех
Список, смысл и правила стиля: docs/brainstorm/shell-3d-assets.md.
"""
import math
import os

import bmesh
import bpy
from mathutils import Matrix, Vector

HERE = os.path.dirname(os.path.abspath(__file__)) if '__file__' in globals() else '/home/dan/projects/legacy-through-time/docs/brainstorm/templates/shell/assets/src'
OUT = os.path.abspath(os.path.join(HERE, '..'))
SIZE = 512

# цвета токенов kit.css
PAL = {
    'energy': '#d6aaff', 'gem': '#66d6ff', 'coin': '#ffd25f', 'accent': '#a8b0ff',
    'surface': '#393d7e', 'text': '#f6f7ff', 'ink': '#14163a', 'line': '#8087d4', 'art': '#6c60c5',
    'paper': '#e4e7ff', 'muted': '#8f97e0', 'deep': '#4a4f9c',
}


def lin(h, k=1.0):
    h = h.lstrip('#')
    c = [int(h[i:i + 2], 16) / 255 for i in (0, 2, 4)]
    c = [((x + 0.055) / 1.055) ** 2.4 if x > 0.04045 else x / 12.92 for x in c]
    return (c[0] * k, c[1] * k, c[2] * k, 1.0)


# ---------------------------------------------------------------- сцена
def reset():
    for o in list(bpy.data.objects):
        bpy.data.objects.remove(o, do_unlink=True)
    for coll in (bpy.data.meshes, bpy.data.materials, bpy.data.lights, bpy.data.cameras, bpy.data.curves):
        for b in list(coll):
            coll.remove(b)
    sc = bpy.context.scene
    for engine in ('BLENDER_EEVEE', 'BLENDER_EEVEE_NEXT'):
        try:
            sc.render.engine = engine
            break
        except TypeError:
            continue
    sc.render.resolution_x = sc.render.resolution_y = SIZE
    sc.render.resolution_percentage = 100
    sc.render.film_transparent = True
    sc.render.image_settings.file_format = 'PNG'
    sc.render.image_settings.color_mode = 'RGBA'
    sc.view_settings.view_transform = 'Standard'
    sc.view_settings.look = 'None'
    world = bpy.data.worlds.get('UIWorld') or bpy.data.worlds.new('UIWorld')
    world.use_nodes = True
    bg = next(n for n in world.node_tree.nodes if n.type == 'BACKGROUND')
    bg.inputs['Color'].default_value = lin('#b9bff0')
    bg.inputs['Strength'].default_value = 0.55
    sc.world = world


def mat(name, color, rough=0.5, metal=0.0, coat=0.0, emit=0.0):
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    p = next(n for n in m.node_tree.nodes if n.type == 'BSDF_PRINCIPLED')
    p.inputs['Base Color'].default_value = lin(color)
    p.inputs['Roughness'].default_value = rough
    p.inputs['Metallic'].default_value = metal
    for key in ('Coat Weight', 'Clearcoat'):
        if key in p.inputs:
            p.inputs[key].default_value = coat
            break
    if emit:
        for key in ('Emission Color', 'Emission'):
            if key in p.inputs:
                p.inputs[key].default_value = lin(color)
                break
        if 'Emission Strength' in p.inputs:
            p.inputs['Emission Strength'].default_value = emit
    return m


def finish(obj, material, smooth=True, bevel=0.0, segments=3):
    obj.data.materials.append(material)
    for poly in obj.data.polygons:
        poly.use_smooth = smooth
    if bevel:
        mod = obj.modifiers.new('bevel', 'BEVEL')
        mod.width = bevel
        mod.segments = segments
        mod.limit_method = 'ANGLE'
    return obj


def new_mesh(name, bm):
    bm.normal_update()
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me)
    bm.free()
    ob = bpy.data.objects.new(name, me)
    bpy.context.scene.collection.objects.link(ob)
    return ob


def poly_extrude(name, pts, depth):
    """Плоский многоугольник в XY, выдавленный вдоль Z (центрирован по Z)."""
    bm = bmesh.new()
    verts = [bm.verts.new((x, y, -depth / 2)) for x, y in pts]
    face = bm.faces.new(verts)
    ext = bmesh.ops.extrude_face_region(bm, geom=[face])
    top = [v for v in ext['geom'] if isinstance(v, bmesh.types.BMVert)]
    bmesh.ops.translate(bm, vec=(0, 0, depth), verts=top)
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces[:])
    return new_mesh(name, bm)


def place(ob, loc=(0, 0, 0), rot=(0, 0, 0), scale=(1, 1, 1)):
    ob.location = loc
    ob.rotation_euler = [math.radians(r) for r in rot]
    ob.scale = scale
    return ob


def cylinder(name, r, depth, seg=48):
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=True, segments=seg, radius1=r, radius2=r, depth=depth)
    return new_mesh(name, bm)


def box(name, sx, sy, sz):
    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=1.0)
    bmesh.ops.scale(bm, vec=(sx, sy, sz), verts=bm.verts)
    return new_mesh(name, bm)


def sphere(name, r, seg=32):
    bm = bmesh.new()
    bmesh.ops.create_uvsphere(bm, u_segments=seg, v_segments=seg // 2, radius=r)
    return new_mesh(name, bm)


def torus(name, major, minor, seg=40, rings=16):
    bm = bmesh.new()
    verts = []
    for i in range(seg):
        a = 2 * math.pi * i / seg
        ring = []
        for j in range(rings):
            b = 2 * math.pi * j / rings
            r = major + minor * math.cos(b)
            ring.append(bm.verts.new((r * math.cos(a), r * math.sin(a), minor * math.sin(b))))
        verts.append(ring)
    for i in range(seg):
        for j in range(rings):
            bm.faces.new([verts[i][j], verts[(i + 1) % seg][j], verts[(i + 1) % seg][(j + 1) % rings], verts[i][(j + 1) % rings]])
    return new_mesh(name, bm)


def group(objs):
    root = bpy.data.objects.new('group', None)
    bpy.context.scene.collection.objects.link(root)
    for o in objs:
        o.parent = root
    return root


# ---------------------------------------------------------------- кадр
def frame_and_render(name, margin=1.16, size=SIZE, outdir=OUT, direction=(0.42, -1.0, 0.55)):
    sc = bpy.context.scene
    sc.render.resolution_x = sc.render.resolution_y = size
    meshes = [o for o in bpy.data.objects if o.type in ('MESH', 'CURVE')]
    bpy.context.view_layer.update()
    corners = [o.matrix_world @ Vector(c) for o in meshes for c in o.bound_box]
    center = sum(corners, Vector()) / len(corners)
    direction = Vector(direction).normalized()
    cam_data = bpy.data.cameras.new('cam')
    cam_data.type = 'ORTHO'
    cam = bpy.data.objects.new('cam', cam_data)
    bpy.context.scene.collection.objects.link(cam)
    cam.location = center + direction * 20
    cam.rotation_euler = (-direction).to_track_quat('-Z', 'Y').to_euler()
    bpy.context.view_layer.update()
    inv = cam.matrix_world.inverted()
    xs = [(inv @ c).x for c in corners]
    ys = [(inv @ c).y for c in corners]
    cam_data.ortho_scale = max(max(xs) - min(xs), max(ys) - min(ys)) * margin
    # сдвиг кадра, чтобы объект был по центру
    cam_data.shift_x = ((max(xs) + min(xs)) / 2) / cam_data.ortho_scale
    cam_data.shift_y = ((max(ys) + min(ys)) / 2) / cam_data.ortho_scale
    sc.camera = cam
    for name_, strength, rot, angle, color in (
        ('key', 3.2, (48, 0, 32), 0.5, '#ffffff'),
        ('fill', 1.0, (62, 0, -128), 0.9, '#c9ceff'),
        ('rim', 1.4, (-38, 0, 168), 0.4, '#ffffff'),
    ):
        ld = bpy.data.lights.new(name_, 'SUN')
        ld.energy = strength
        ld.angle = angle
        ld.color = lin(color)[:3]
        lo = bpy.data.objects.new(name_, ld)
        bpy.context.scene.collection.objects.link(lo)
        lo.rotation_euler = [math.radians(r) for r in rot]
    os.makedirs(outdir, exist_ok=True)
    sc.render.filepath = os.path.join(outdir, name + '.png')
    bpy.ops.render.render(write_still=True)


# ---------------------------------------------------------------- модели
def gem_object(name='gem', color=PAL['gem'], scale=1.0):
    """Огранка: павильон (остриё вниз) + корона + площадка сверху."""
    n = 8
    r = 1.0
    bm = bmesh.new()
    girdle_h = 0.0
    table_r = 0.55
    top_h = 0.55
    bot_h = 1.0
    bot = bm.verts.new((0, 0, -bot_h))
    gird = [bm.verts.new((r * math.cos(2 * math.pi * i / n), r * math.sin(2 * math.pi * i / n), girdle_h)) for i in range(n)]
    table = [bm.verts.new((table_r * math.cos(2 * math.pi * (i + 0.5) / n), table_r * math.sin(2 * math.pi * (i + 0.5) / n), top_h)) for i in range(n)]
    for i in range(n):
        bm.faces.new([bot, gird[(i + 1) % n], gird[i]])
        bm.faces.new([gird[i], gird[(i + 1) % n], table[i]])
        bm.faces.new([gird[(i + 1) % n], table[(i + 1) % n], table[i]])
    bm.faces.new(table[::-1])
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces[:])
    ob = new_mesh(name, bm)
    finish(ob, mat(name, color, rough=0.32, coat=0.4), smooth=False)
    ob.scale = (scale, scale, scale)
    return ob


def bolt_object(name='bolt', color=PAL['energy'], scale=1.0, depth=0.34):
    pts = [(0.22, 1.05), (-0.62, -0.08), (-0.1, -0.08), (-0.34, -1.05), (0.62, 0.18), (0.08, 0.18), (0.56, 1.05)]
    pts = [(x * 0.9, y) for x, y in pts]
    ob = poly_extrude(name, pts, depth)
    finish(ob, mat(name, color, rough=0.45, coat=0.25), smooth=True, bevel=0.07, segments=4)
    ob.scale = (scale, scale, scale)
    return ob


def build_gem():
    ob = gem_object()
    place(ob, rot=(0, 0, 12))


def build_coin():
    gold = mat('gold', PAL['coin'], rough=0.42, coat=0.2)
    dark = mat('gold-dark', '#e8b232', rough=0.5)
    rim = cylinder('rim', 1.0, 0.22, 64)
    finish(rim, gold, bevel=0.07, segments=4)
    face = cylinder('face', 0.76, 0.1, 64)
    finish(face, dark, bevel=0.03, segments=3)
    place(face, loc=(0, 0, 0.1))
    star = poly_extrude('star', [(0.4 * math.cos(math.radians(a)) * (1 if i % 2 == 0 else 0.45) * 1.0, 0.4 * math.sin(math.radians(a)) * (1 if i % 2 == 0 else 0.45)) for i, a in enumerate(range(90, 90 + 360, 36))], 0.08)
    finish(star, gold, bevel=0.015, segments=2)
    place(star, loc=(0, 0, 0.2))
    root = group([rim, face, star])
    root.rotation_euler = (math.radians(90), 0, math.radians(10))


def stand(objs, z_rot=0):
    """Ставит плоскую в XY модель лицом к камере."""
    root = group(objs)
    root.rotation_euler = (math.radians(90), 0, math.radians(z_rot))
    return root


def thick_polyline(points, w):
    """Многоугольник «толстой ломаной» (для галочки): смещаем точки по нормалям с мitre."""
    def norm(a, b):
        d = Vector((b[0] - a[0], b[1] - a[1])).normalized()
        return Vector((-d.y, d.x))
    left, right = [], []
    for i, p in enumerate(points):
        n0 = norm(points[i - 1], p) if i > 0 else None
        n1 = norm(p, points[i + 1]) if i < len(points) - 1 else None
        n = (n0 + n1).normalized() if n0 and n1 else (n0 or n1)
        k = (w / 2) / max(0.4, (n.dot(n0 or n1)))
        left.append((p[0] + n.x * k, p[1] + n.y * k))
        right.append((p[0] - n.x * k, p[1] - n.y * k))
    return left + right[::-1]


def build_energy():
    ob = bolt_object()
    stand([ob], 8)


def bolt_fan(specs):
    """specs: (x, y, scale, rot, z) для каждой молнии, строим в плоскости XY, потом ставим к камере."""
    bolts = []
    for i, (x, y, s, rot, z) in enumerate(specs):
        b = bolt_object(f'bolt{i}', scale=s)
        place(b, loc=(x, y, z), rot=(0, 0, rot), scale=(s, s, s))
        bolts.append(b)
    stand(bolts, 8)


def build_energy_pack_s():
    bolt_fan([(-0.8, -0.05, 0.85, 14, -0.3), (0.8, -0.05, 0.85, -14, -0.3), (0, 0.05, 1.05, 0, 0.3)])


def build_energy_pack_l():
    bolt_fan([(-1.05, 0.25, 0.8, 20, -0.7), (0, 0.4, 0.92, 0, -0.7), (1.05, 0.25, 0.8, -20, -0.7),
              (-0.62, -0.1, 0.9, 10, 0.0), (0.62, -0.1, 0.9, -10, 0.0),
              (0, -0.35, 1.05, 0, 0.7)])


def gem_cluster(specs):
    for i, (x, y, z, s, rz, tilt) in enumerate(specs):
        g = gem_object(f'gem{i}', scale=s)
        place(g, loc=(x, y, z), rot=(tilt, 0, rz), scale=(s, s, s))


def build_gem_pack_s():
    gem_cluster([(-1.0, 0.5, 0.1, 0.7, 20, 12), (1.0, 0.45, 0.05, 0.74, -14, -10), (0, -0.25, 0.75, 0.95, 8, 0)])


def build_gem_pack_l():
    gem_cluster([(-1.3, 0.6, 0.0, 0.72, 18, 12), (0, 0.8, 0.05, 0.78, -8, -4), (1.3, 0.6, 0.0, 0.72, -20, -12),
                 (-0.65, 0.2, 1.0, 0.78, 10, 8), (0.65, 0.2, 1.0, 0.78, -12, -8),
                 (0, -0.2, 1.95, 0.8, 24, 6), (0.0, -1.0, 0.55, 0.95, -6, 0)])


def gift_parts(open_lid=False):
    body_h = 1.0 if open_lid else 1.2
    parts = []
    body = finish(box('body', 1.5, 1.5, body_h), mat('gift-body', PAL['accent'], rough=0.55, coat=0.15), bevel=0.06)
    place(body, loc=(0, 0, body_h / 2))
    parts.append(body)
    rib = mat('ribbon', PAL['paper'], rough=0.6)
    for sx, sy in ((0.34, 1.56), (1.56, 0.34)):
        band = finish(box('band', sx, sy, body_h + 0.02), rib, bevel=0.03)
        place(band, loc=(0, 0, (body_h + 0.02) / 2))
        parts.append(band)
    lid_z = body_h + 0.16 + (0.95 if open_lid else 0)
    lid = finish(box('lid', 1.64, 1.64, 0.32), mat('gift-lid', PAL['accent'], rough=0.55, coat=0.15), bevel=0.06)
    place(lid, loc=(-0.15 if open_lid else 0, 0.05 if open_lid else 0, lid_z), rot=(14, 0, -18) if open_lid else (0, 0, 0))
    parts.append(lid)
    for sx, sy in ((0.36, 1.68), (1.68, 0.36)):
        band = finish(box('lidband', sx, sy, 0.34), rib, bevel=0.03)
        place(band, loc=(-0.15 if open_lid else 0, 0.05 if open_lid else 0, lid_z), rot=(14, 0, -18) if open_lid else (0, 0, 0))
        parts.append(band)
    if open_lid:
        inner = finish(box('inner', 1.34, 1.34, 0.02), mat('inner', PAL['deep'], rough=0.9), smooth=False)
        place(inner, loc=(0, 0, body_h + 0.011))
        parts.append(inner)
    else:
        for rz in (28, -28):
            ear = finish(sphere('ear', 0.3), rib)
            place(ear, loc=(0.26 * math.sin(math.radians(rz)) * 1.6, 0, lid_z + 0.3), rot=(0, rz, 0), scale=(1.0, 0.42, 0.72))
            parts.append(ear)
        knot = finish(sphere('knot', 0.17), rib)
        place(knot, loc=(0, 0, lid_z + 0.2))
        parts.append(knot)
    root = group(parts)
    root.rotation_euler = (0, 0, math.radians(-14))


def build_gift():
    gift_parts(False)


def build_gift_open():
    gift_parts(True)


def build_calendar():
    page = finish(box('page', 1.8, 0.26, 1.9), mat('page', PAL['paper'], rough=0.6), bevel=0.08)
    head = finish(box('head', 1.82, 0.3, 0.55), mat('head', PAL['accent'], rough=0.5, coat=0.15), bevel=0.05)
    place(head, loc=(0, -0.01, 0.68))
    rings = []
    for x in (-0.5, 0.5):
        r = finish(cylinder('ring', 0.1, 0.62, 24), mat('ring', PAL['deep'], rough=0.5), bevel=0.02)
        place(r, loc=(x, 0, 1.0), rot=(90, 0, 0))
        rings.append(r)
    check = poly_extrude('check', thick_polyline([(-0.5, -0.02), (-0.2, -0.32), (0.5, 0.38)], 0.24), 0.12)
    finish(check, mat('check', PAL['accent'], rough=0.5, coat=0.1), bevel=0.03)
    stand([check], 0)
    check.parent.location = (0, -0.2, -0.28)
    root = group([page, head, *rings, check.parent])
    root.rotation_euler = (0, 0, math.radians(-14))


def build_star():
    pts = [(math.cos(math.radians(90 + i * 36)) * (1.0 if i % 2 == 0 else 0.46), math.sin(math.radians(90 + i * 36)) * (1.0 if i % 2 == 0 else 0.46)) for i in range(10)]
    s = poly_extrude('star', pts, 0.36)
    finish(s, mat('star', PAL['coin'], rough=0.42, coat=0.2), bevel=0.09, segments=4)
    stand([s], 10)


def build_play():
    slab = finish(box('slab', 2.0, 0.3, 1.45), mat('slab', PAL['art'], rough=0.55, coat=0.1), bevel=0.12)
    disc = finish(cylinder('disc', 0.52, 0.14, 48), mat('disc', PAL['accent'], rough=0.5, coat=0.15), bevel=0.03)
    place(disc, loc=(0, -0.2, 0), rot=(90, 0, 0))
    tri = poly_extrude('tri', [(-0.17, -0.3), (-0.17, 0.3), (0.3, 0.0)], 0.1)
    finish(tri, mat('tri', PAL['paper'], rough=0.5), bevel=0.02)
    stand([tri])
    tri.parent.location = (0, -0.31, 0)
    root = group([slab, disc, tri.parent])
    root.rotation_euler = (0, 0, math.radians(-12))


def build_chat():
    body = finish(box('bubble', 2.1, 0.5, 1.4), mat('bubble', PAL['muted'], rough=0.55, coat=0.1), bevel=0.32, segments=8)
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=True, segments=3, radius1=0.38, radius2=0.0, depth=0.5)
    tail = new_mesh('tail', bm)
    finish(tail, mat('tail', PAL['muted'], rough=0.55, coat=0.1), bevel=0.03)
    place(tail, loc=(-0.55, 0, -0.85), rot=(0, 0, 90), scale=(0.85, 1.0, 1.0))
    tail.rotation_euler = (math.radians(0), math.radians(180), math.radians(90))
    dots = []
    for x in (-0.5, 0, 0.5):
        d = finish(sphere('dot', 0.14), mat('dot', PAL['paper'], rough=0.5))
        place(d, loc=(x, -0.27, 0.05))
        dots.append(d)
    root = group([body, tail, *dots])
    root.rotation_euler = (0, 0, math.radians(-14))


def build_book():
    cover = mat('cover', PAL['art'], rough=0.55, coat=0.12)
    pages = mat('pages', PAL['paper'], rough=0.65)
    parts = []
    front = finish(box('front', 1.5, 0.1, 2.0), cover, bevel=0.03)
    place(front, loc=(0, -0.25, 0))
    back = finish(box('back', 1.5, 0.1, 2.0), cover, bevel=0.03)
    place(back, loc=(0, 0.25, 0))
    spine = finish(box('spine', 0.12, 0.6, 2.0), cover, bevel=0.03)
    place(spine, loc=(-0.69, 0, 0))
    block = finish(box('block', 1.34, 0.42, 1.88), pages, bevel=0.01)
    place(block, loc=(0.03, 0, 0))
    badge = finish(cylinder('badge', 0.28, 0.04, 32), mat('badge', PAL['accent'], rough=0.5, coat=0.15))
    place(badge, loc=(0.0, -0.32, 0.35), rot=(90, 0, 0))
    mark = finish(box('mark', 0.14, 0.05, 0.55), mat('mark', PAL['coin'], rough=0.5), bevel=0.01)
    place(mark, loc=(0.4, -0.31, -1.1))
    parts = [front, back, spine, block, badge, mark]
    root = group(parts)
    root.rotation_euler = (0, 0, math.radians(-28))


def build_cart():
    metal = mat('cartm', PAL['muted'], rough=0.5, coat=0.1)
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=True, segments=4, radius1=0.62, radius2=0.95, depth=0.85)
    basket = new_mesh('basket', bm)
    finish(basket, metal, bevel=0.05)
    place(basket, loc=(0, 0, 0.55), rot=(0, 0, 45))
    inner = finish(box('inner', 1.12, 1.12, 0.02), mat('cartin', PAL['deep'], rough=0.9), smooth=False)
    place(inner, loc=(0, 0, 0.97))
    post = finish(cylinder('post', 0.075, 1.15, 16), metal)
    place(post, loc=(-0.95, 0, 1.0), rot=(0, 12, 0))
    grip = finish(cylinder('grip', 0.075, 0.85, 16), metal)
    place(grip, loc=(-1.08, 0, 1.58), rot=(90, 0, 0))
    wheels = []
    for y in (-0.42, 0.42):
        w = finish(cylinder('wheel', 0.2, 0.14, 24), mat('wheel', PAL['deep'], rough=0.7), bevel=0.02)
        place(w, loc=(-0.1, y, 0.0), rot=(90, 0, 0))
        wheels.append(w)
    root = group([basket, inner, post, grip, *wheels])
    root.rotation_euler = (0, 0, math.radians(-20))


def build_sparkle():
    pts = [(0, 1.0), (0.2, 0.2), (1.0, 0), (0.2, -0.2), (0, -1.0), (-0.2, -0.2), (-1.0, 0), (-0.2, 0.2)]
    s = poly_extrude('sparkle', pts, 0.1)
    finish(s, mat('sparkle', '#fff1b8', rough=0.4, emit=1.6), bevel=0.02)
    stand([s], 0)


def build_wheel():
    base = finish(cylinder('base', 1.0, 0.3, 64), mat('wheel-base', PAL['deep'], rough=0.55, coat=0.1), bevel=0.05)
    parts = [base]
    tones = (PAL['accent'], PAL['art'])
    for i in range(8):
        pts = [(0, 0)] + [(0.9 * math.cos(math.radians(i * 45 + k * 45 / 6)), 0.9 * math.sin(math.radians(i * 45 + k * 45 / 6))) for k in range(7)]
        w = poly_extrude('wedge', pts, 0.07)
        finish(w, mat('wedge', tones[i % 2], rough=0.5, coat=0.12), bevel=0.012)
        place(w, loc=(0, 0, 0.17))
        parts.append(w)
    rim = finish(torus('rim', 1.0, 0.09, 64, 14), mat('rim', PAL['paper'], rough=0.5))
    place(rim, loc=(0, 0, 0.1))
    hub = finish(sphere('hub', 0.28), mat('hub', PAL['paper'], rough=0.45, coat=0.2))
    place(hub, loc=(0, 0, 0.24))
    parts += [rim, hub]
    stand(parts, 0)


# ---------------------------------------------------------------- глифы: пухлые 3D-трубки (собственная графика)
def arc_pts(cx, cy, r, a0, a1, n=20):
    return [(cx + r * math.cos(math.radians(a0 + (a1 - a0) * i / n)), cy + r * math.sin(math.radians(a0 + (a1 - a0) * i / n))) for i in range(n + 1)]


def circle_pts(cx, cy, r, n=36):
    return arc_pts(cx, cy, r, 0, 360, n)[:-1]


def rect_pts(x0, y0, x1, y1):
    return [(x0, y0), (x1, y0), (x1, y1), (x0, y1)]


def line(*pts, joints=True):
    return ('line', list(pts), joints)


def loop(pts, joints=True):
    return ('loop', list(pts), joints)


def arc(cx, cy, r, a0, a1):
    return ('line', arc_pts(cx, cy, r, a0, a1), False)


def circ(cx, cy, r):
    return ('loop', circle_pts(cx, cy, r), False)


def dot(x, y, r=0.17):
    return ('dot', x, y, r)


GLYPHS = {
    'plus': [line((-0.62, 0), (0.62, 0)), line((0, -0.62), (0, 0.62))],
    'close': [line((-0.56, -0.56), (0.56, 0.56)), line((-0.56, 0.56), (0.56, -0.56))],
    'check': [line((-0.64, 0.0), (-0.2, -0.46), (0.66, 0.5))],
    'chevron-l': [line((0.34, 0.62), (-0.32, 0), (0.34, -0.62))],
    'chevron-r': [line((-0.34, 0.62), (0.32, 0), (-0.34, -0.62))],
    'arrow-right': [line((-0.66, 0), (0.62, 0)), line((0.06, 0.52), (0.64, 0), (0.06, -0.52))],
    'back': [circ(0, 0, 0.78), line((0.34, 0), (-0.34, 0)), line((-0.02, 0.32), (-0.36, 0), (-0.02, -0.32))],
    'search': [circ(-0.12, 0.12, 0.5), line((0.26, -0.26), (0.7, -0.7))],
    'gear': [loop([(r * math.cos(math.radians(a)), r * math.sin(math.radians(a))) for i in range(8) for r, a in ((0.6, i * 45 - 17), (0.88, i * 45 - 10), (0.88, i * 45 + 10), (0.6, i * 45 + 17))], False), circ(0, 0, 0.24)],
    'lock': [('solid', rect_pts(-0.55, -0.78, 0.55, 0.0), 0.3), line(*arc_pts(0, 0.0, 0.36, 0, 180), joints=False)],
    'timer': [circ(0, -0.08, 0.66), line((0, 0.2), (0, -0.08), (0.26, -0.24)), line((-0.2, 0.9), (0.2, 0.9), joints=False)],
    'flag': [line((-0.5, -0.82), (-0.5, 0.82)), loop([(-0.5, 0.8), (0.55, 0.52), (-0.5, 0.2)])],
    'shuffle': [line((-0.74, 0.42), (-0.2, 0.42), (0.26, -0.42), (0.66, -0.42)), line((0.42, -0.66), (0.68, -0.42), (0.42, -0.18)),
                line((-0.74, -0.42), (-0.2, -0.42), (0.26, 0.42), (0.66, 0.42)), line((0.42, 0.18), (0.68, 0.42), (0.42, 0.66))],
    'bell': [loop(arc_pts(0, 0.12, 0.5, 180, 0) + [(0.62, -0.42), (-0.62, -0.42)], joints=False), dot(0, -0.72, 0.16)],
    'sound': [loop([(-0.74, 0.24), (-0.42, 0.24), (0.0, 0.62), (0.0, -0.62), (-0.42, -0.24), (-0.74, -0.24)]), arc(0.0, 0, 0.3, -40, 40), arc(0.0, 0, 0.74, -40, 40)],
    'globe': [circ(0, 0, 0.74), line((-0.74, 0), (0.74, 0), joints=False), loop([(0.34 * math.cos(math.radians(a)), 0.74 * math.sin(math.radians(a))) for a in range(0, 360, 15)], False)],
    'user': [circ(0, 0.34, 0.32), line(*arc_pts(0, -0.78, 0.78, 28, 152), joints=False)],
    'support': [arc(0, 0.0, 0.62, 0, 180), line((-0.62, 0), (-0.62, -0.28), joints=False), line((0.62, 0), (0.62, -0.28), joints=False), dot(-0.62, -0.38, 0.2), dot(0.62, -0.38, 0.2)],
    'shield': [loop([(0, 0.82), (0.66, 0.56), (0.62, -0.1), (0, -0.82), (-0.62, -0.1), (-0.66, 0.56)])],
    'doc': [loop([(-0.5, -0.78), (0.5, -0.78), (0.5, 0.36), (0.1, 0.78), (-0.5, 0.78)]), line((0.1, 0.78), (0.1, 0.36), (0.5, 0.36)), line((-0.14, 0.0), (0.14, 0.0), joints=False), line((-0.14, -0.4), (0.1, -0.4), joints=False)],
    'tools': [line((-0.56, -0.56), (0.2, 0.2), joints=False), line(*arc_pts(0.4, 0.4, 0.34, 40, 320), joints=False)],
    'receipt': [loop([(-0.5, 0.82), (0.5, 0.82), (0.5, -0.82), (0.25, -0.58), (0, -0.82), (-0.25, -0.58), (-0.5, -0.82)]), line((-0.1, 0.36), (0.1, 0.36), joints=False), line((-0.1, -0.02), (0.1, -0.02), joints=False)],
    'ads': [loop([(-0.8, -0.58), (0.8, -0.58), (0.8, 0.58), (-0.8, 0.58)]), loop([(-0.14, 0.3), (-0.14, -0.3), (0.32, 0)])],
}


def add_tube(bm, p0, p1, r):
    """Цилиндр между двумя точками плоскости XY (в одном bmesh, чтобы потом сплавить)."""
    a, b = Vector((p0[0], p0[1], 0)), Vector((p1[0], p1[1], 0))
    d = b - a
    length = d.length
    if length < 1e-6:
        return
    up = 'X' if abs(d.normalized().y) > 0.99 else 'Y'
    m = Matrix.Translation((a + b) / 2) @ d.to_track_quat('Z', up).to_matrix().to_4x4()
    bmesh.ops.create_cone(bm, cap_ends=True, segments=16, radius1=r, radius2=r, depth=length, matrix=m)


def add_ball(bm, p, r):
    bmesh.ops.create_uvsphere(bm, u_segments=16, v_segments=10, radius=r, matrix=Matrix.Translation((p[0], p[1], 0)))


def glyph_builder(spec, color, r=0.13):
    """Все штрихи глифа собираются в одно тело и сплавляются воксельным ремешем:
    на пересечениях и стыках получается цельная мягкая поверхность без швов и узлов."""
    material = mat('glyph', color, rough=0.5, coat=0.08)
    bm = bmesh.new()
    solids = []
    for item in spec:
        kind = item[0]
        if kind in ('line', 'loop'):
            pts = item[1]
            pairs = list(zip(pts, pts[1:] + ([pts[0]] if kind == 'loop' else [])))
            for p0, p1 in pairs:
                add_tube(bm, p0, p1, r)
            for q in pts:
                add_ball(bm, q, r)
        elif kind == 'dot':
            _, x, y, rr = item
            if rr > 0:
                add_ball(bm, (x, y), rr)
        elif kind == 'solid':
            solids.append(item)
    ob = new_mesh('glyph', bm)
    ob.data.materials.append(material)
    remesh = ob.modifiers.new('remesh', 'REMESH')
    remesh.mode = 'VOXEL'
    remesh.voxel_size = 0.028
    remesh.use_smooth_shade = True
    smooth = ob.modifiers.new('smooth', 'SMOOTH')
    smooth.factor = 1.0
    smooth.iterations = 8
    made = [ob]
    for _, pts, depth in solids:
        s = poly_extrude('solid', pts, depth)
        finish(s, material, bevel=0.1, segments=4)
        made.append(s)
    stand(made, 0)


def build_glyphs(names=None, variants=(('', PAL['paper']), ('-ink', PAL['ink']))):
    for name in names or list(GLYPHS):
        for suffix, color in variants:
            reset()
            glyph_builder(GLYPHS[name], color)
            frame_and_render(name + suffix, margin=1.18, size=192, outdir=os.path.join(OUT, 'g'), direction=(0.14, -1.0, 0.2))
        print('glyph', name)


BUILDERS = {
    'gem': build_gem, 'coin': build_coin, 'energy': build_energy,
    'gem-pack-s': build_gem_pack_s, 'gem-pack-l': build_gem_pack_l,
    'energy-pack-s': build_energy_pack_s, 'energy-pack-l': build_energy_pack_l,
    'gift': build_gift, 'gift-open': build_gift_open, 'calendar': build_calendar, 'star': build_star,
    'wheel': build_wheel, 'play': build_play, 'chat': build_chat, 'book': build_book, 'cart': build_cart, 'sparkle': build_sparkle,
}


def build(names=None):
    """Крупные 3D-картинки в assets/*.png."""
    for name in names or list(BUILDERS):
        reset()
        BUILDERS[name]()
        frame_and_render(name)
        print('rendered', name)
