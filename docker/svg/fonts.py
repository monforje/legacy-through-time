"""Static TTFs for Unity from Google Fonts variable sources (OFL). Unity cannot pick a weight from a
variable font, so each weight we use is instantiated to its own file.
Usage: python3 fonts.py <out_dir>"""
import io
import sys
import urllib.request
from pathlib import Path

from fontTools.ttLib import TTFont
from fontTools.varLib import instancer

BASE = "https://raw.githubusercontent.com/google/fonts/main/ofl/"
OUT = Path(sys.argv[1])
OUT.mkdir(parents=True, exist_ok=True)


def fetch(path):
    with urllib.request.urlopen(BASE + path.replace("[", "%5B").replace("]", "%5D")) as r:
        return r.read()


(OUT / "RussoOne-Regular.ttf").write_bytes(fetch("russoone/RussoOne-Regular.ttf"))
for src, weights in (("rubik/Rubik[wght].ttf", {"Rubik-Bold": 700}),
                     ("nunito/Nunito[wght].ttf", {"Nunito-Medium": 500, "Nunito-Bold": 700})):
    data = fetch(src)
    for name, wght in weights.items():
        font = instancer.instantiateVariableFont(TTFont(io.BytesIO(data)), {"wght": wght})
        font.save(OUT / f"{name}.ttf")
(OUT / "OFL.txt").write_text("Russo One, Rubik, Nunito: SIL Open Font License 1.1 - https://openfontlicense.org\n"
                             "Sources: https://github.com/google/fonts (ofl/russoone, ofl/rubik, ofl/nunito)\n")
print("fonts ok:", sorted(p.name for p in OUT.iterdir()))
