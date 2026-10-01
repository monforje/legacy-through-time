"""The sprite document: what a builder returns and how it becomes an SVG file."""
from dataclasses import dataclass

from .palette import INK

# Paper grain: a quiet noise inside the shape only, over the fill.
GRAIN = ('<defs><filter id="g" x="0" y="0" width="100%" height="100%">'
         '<feTurbulence type="fractalNoise" baseFrequency=".9" numOctaves="2" seed="4" result="n"/>'
         '<feColorMatrix in="n" values="0 0 0 0 .35  0 0 0 0 .22  0 0 0 0 .15  0 0 0 .07 0" result="c"/>'
         '<feComposite in="c" in2="SourceGraphic" operator="in"/></filter></defs>')


def grain(path_d):
    """The grain layer for a shape. The same path as the fill, so that the grain never leaves the fill."""
    return f'<path d="{path_d}" fill="#000" filter="url(#g)"/>'


@dataclass
class Sprite:
    name: str
    width: int
    height: int
    body: str
    note: str = ''
    # Colour of the almost invisible bed under the drawing (alpha 1/255). A transparent PNG pixel from rsvg is
    # white or black; mip-maps blend that colour into the edges. With a bed of the colour of the outline there is
    # nothing wrong to blend. None: no bed (the landscape).
    halo: str = INK

    def render(self):
        head = f'<!-- {self.note} -->\n' if self.note else ''
        bed = f'<rect width="{self.width}" height="{self.height}" fill="{self.halo}" fill-opacity=".0045"/>\n' if self.halo else ''
        return (f'<svg xmlns="http://www.w3.org/2000/svg" width="{self.width}" height="{self.height}" '
                f'viewBox="0 0 {self.width} {self.height}">\n{head}{bed}{self.body}\n</svg>\n')
