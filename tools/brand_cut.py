from PIL import Image
import numpy as np, os
out = "docs/brand/cut"; os.makedirs(out, exist_ok=True)
im = Image.open("docs/brand/Fantasy Heritage Logo Collection.png").convert("RGB")
a = np.asarray(im).astype(float)
bg = np.array([248, 2, 250.])
dist = np.sqrt(((a - bg) ** 2).sum(2))
alpha = np.clip((dist - 40) / 90, 0, 1)
# un-mix the background from edge pixels
safe = np.maximum(alpha, 1e-3)[..., None]
rgb = np.clip((a - (1 - alpha[..., None]) * bg) / safe, 0, 255)
rgba = np.dstack([rgb, alpha * 255]).astype(np.uint8)
full = Image.fromarray(rgba, "RGBA")
names = [["emblem", "wordmark", "icon-tile"], ["glyph-dark", "glyph-white", "loader-glyph"]]
for r in range(2):
    for c in range(3):
        cell = full.crop((c * 512, r * 512, (c + 1) * 512, (r + 1) * 512))
        bb = cell.getchannel("A").point(lambda v: 255 if v > 24 else 0).getbbox()
        cell.crop(bb).save(f"{out}/{names[r][c]}.png")
        print(names[r][c], bb)
