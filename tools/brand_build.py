from PIL import Image, ImageDraw
import numpy as np, os
B = "docs/brand/"
def key(path, bg=(246, 3, 250.)):
    a = np.asarray(Image.open(path).convert("RGB")).astype(float)
    bg = np.array(bg); d = np.sqrt(((a - bg) ** 2).sum(2))
    al = np.clip((d - 40) / 90, 0, 1)
    rgb = np.clip((a - (1 - al[..., None]) * bg) / np.maximum(al, 1e-3)[..., None], 0, 255)
    edge = al < .98
    rgb[..., 2] = np.where(edge, np.minimum(rgb[..., 2], rgb[..., 1]), rgb[..., 2])   # magenta spill: blue never above green on edges
    rgb[..., 0] = np.where(edge, np.minimum(rgb[..., 0], rgb[..., 1] * 1.6 + 20), rgb[..., 0])
    rgb[al < .05] = 0
    return Image.fromarray(np.dstack([rgb, al * 255]).astype(np.uint8), "RGBA")
def trim(im, pad=0, thr=24):
    bb = im.getchannel("A").point(lambda v: 255 if v > thr else 0).getbbox()
    im = im.crop(bb)
    if pad:
        c = Image.new("RGBA", (im.width + 2 * pad, im.height + 2 * pad)); c.paste(im, (pad, pad)); im = c
    return im

# splash logo (keyed from the clean magenta render)
full = key(B + "Heritage Through Time Emblem.png")
splash = trim(full, 16)
os.makedirs("Assets/Resources/Brand", exist_ok=True); os.makedirs("Assets/Art/Brand", exist_ok=True)
splash.save("Assets/Resources/Brand/logo.png")
print("logo", splash.size)

# emblem without wreath and title, for the launcher icon
emb = Image.open(B + "cut/emblem.png").convert("RGBA")
print("emblem", emb.size)
S = 1024
def fg(scale):
    e = emb.copy(); w = int(S * scale); h = int(e.height * w / e.width)
    e = e.resize((w, h), Image.LANCZOS)
    c = Image.new("RGBA", (S, S)); c.paste(e, ((S - w) // 2, (S - h) // 2), e); return c
# adaptive foreground: content within the 66% safe zone
fg(0.62).save("Assets/Art/Brand/icon-fg.png")
# background: emerald, soft radial light, paper grain
y, x = np.mgrid[0:S, 0:S]; r = np.hypot(x - S * .5, y - S * .42) / (S * .75)
base = np.array([21, 117, 84.]); dark = np.array([10, 62, 46.])
t = np.clip(r, 0, 1)[..., None]
rng = np.random.default_rng(7)
grain = rng.normal(0, 4, (S, S, 1))
bgarr = np.clip(base * (1 - t) + dark * t + grain, 0, 255).astype(np.uint8)
bg = Image.fromarray(bgarr, "RGB").convert("RGBA"); bg.save("Assets/Art/Brand/icon-bg.png")
def mask(shape):
    m = Image.new("L", (S * 2, S * 2), 0); d = ImageDraw.Draw(m)
    if shape == "round": d.ellipse((0, 0, S * 2 - 1, S * 2 - 1), fill=255)
    else: d.rounded_rectangle((0, 0, S * 2 - 1, S * 2 - 1), radius=int(S * 2 * .18), fill=255)
    return m.resize((S, S), Image.LANCZOS)

FACES = B + "app-icon-faces.png"
if os.path.exists(FACES):
    # the painted icon with the faces: the full square for the legacy/store icon; adaptive and round get it
    # inside the safe zone on the emerald background, edges feathered
    art = Image.open(FACES).convert("RGB").resize((S, S), Image.LANCZOS)
    art.save(B + "cut/app-icon-1024.png")
    def inset(scale):
        w = int(S * scale); a = art.resize((w, w), Image.LANCZOS).convert("RGBA")
        f = int(w * .06); m = Image.new("L", (w, w), 0); ImageDraw.Draw(m).rectangle((f, f, w - f, w - f), fill=255)
        from PIL import ImageFilter
        a.putalpha(m.filter(ImageFilter.GaussianBlur(f / 2)))
        c = bg.copy(); c.alpha_composite(a, ((S - w) // 2, (S - w) // 2)); return c
    Image.new("RGBA", (S, S)).save("Assets/Art/Brand/icon-fg.png")       # adaptive: everything is in the background layer
    inset(.74).save("Assets/Art/Brand/icon-bg.png")
    legacy = art.convert("RGBA"); legacy.putalpha(mask("sq")); legacy.save("Assets/Art/Brand/icon-legacy.png")
    rnd = inset(.8); rnd.putalpha(mask("round")); rnd.save("Assets/Art/Brand/icon-round.png")
else:
    comp = bg.copy(); comp.alpha_composite(fg(0.74))
    for name, shape in (("icon-legacy", "sq"), ("icon-round", "round")):
        c = comp.copy(); c.putalpha(mask(shape)); c.save(f"Assets/Art/Brand/{name}.png")
    comp.convert("RGB").save(B + "cut/app-icon-1024.png")

# loader: ring + hourglass keyed from the clean magenta render
sh = key(B + "Ornate Wreath and Antique Hourglass.png")
half = sh.width // 2
trim(sh.crop((0, 0, half, sh.height)), 4, 128).save("Assets/Resources/Brand/loader-ring.png")
trim(sh.crop((half, 0, sh.width, sh.height)), 4, 128).save("Assets/Resources/Brand/loader-glass.png")
# favicon/glyphs from the sheet cut
for n in ("glyph-dark", "glyph-white"):
    Image.open(B + f"cut/{n}.png").save(f"Assets/Resources/Brand/{n}.png")
