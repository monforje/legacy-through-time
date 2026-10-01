"""The title screen: brackets, the sun ornament between them, the landscape."""
import math

from kit.geometry import f, rhombus
from kit.palette import CREAM, INK, MINT, ROSE, SCARLET
from kit.registry import REGISTRY, Slice
from kit.svg import Sprite


@REGISTRY.sprite('title-brackets', Slice(30, 50, 50, 30, fill=False))
def brackets(hand):
    """The brackets of the title: the upper and the lower in one file. 160x80, slice 30 vertically and 50 on the
    sides, an empty middle. The ornament in the centre is title-orn.svg."""
    half_top = '\n'.join([
        f'<path d="M50,14 H160" stroke="{CREAM}" stroke-width="3"/>',          # a double thread: cream, and red under it
        f'<path d="M30,20 H160" stroke="{SCARLET}" stroke-width="1.5"/>',
        # the end curls toward the text in a wave
        f'<path d="M50,14 H20 Q10,14 9,21 Q8.5,27 14,27 Q18.5,27 18,22.5 Q17.5,19.5 14.5,20.5" fill="none" '
        f'stroke="{CREAM}" stroke-width="3" stroke-linecap="round"/>',
        rhombus(30, 14, 4, SCARLET, CREAM, 1.2),
        f'<circle cx="40" cy="14" r="1.6" fill="{CREAM}"/>',
    ])
    half = f'<g>{half_top}</g><g transform="translate(160,0) scale(-1,1)">{half_top}</g>'
    return Sprite('title-brackets', 160, 80, f'{half}\n<g transform="translate(0,80) scale(1,-1)">{half}</g>',
                  'border-image: slice 30 50; no fill: the middle is transparent', halo=CREAM)


@REGISTRY.sprite('title-orn')
def title_ornament(hand):
    """The centre of a bracket: the solar rhombus with rays."""
    rays = ''.join(
        f'<path d="M{f(32 + 9 * math.cos(a))},{f(16 + 9 * math.sin(a))} L{f(32 + 14 * math.cos(a))},{f(16 + 14 * math.sin(a))}" '
        f'stroke="{CREAM}" stroke-width="1.6" stroke-linecap="round"/>'
        for a in (i * math.tau / 8 + math.tau / 16 for i in range(8)))
    return Sprite('title-orn', 64, 32, '\n'.join([
        rays,
        rhombus(32, 16, 9, SCARLET, INK, 2),
        rhombus(32, 16, 5, CREAM),
        rhombus(32, 16, 2.2, SCARLET),
        rhombus(14, 16, 2.4, CREAM), rhombus(50, 16, 2.4, CREAM),
    ]))


def fir(x, base, height, color):
    """A fir of four tiers and a trunk."""
    width = height * .42
    tiers = []
    for k in range(4):
        top = base - height + k * height * .2
        bottom = top + height * .42
        half = width * (.45 + k * .18) / 2
        tiers.append(f'M{f(x)},{f(top)} L{f(x + half)},{f(bottom)} L{f(x - half)},{f(bottom)}Z')
    return f'<path d="{" ".join(tiers)} M{f(x - 1.2)},{f(base - 4)} h2.4 v6 h-2.4Z" fill="{color}"/>'


def bird(x, y, s):
    return (f'<path d="M{f(x - 7 * s)},{f(y - 2 * s)} Q{f(x - 3 * s)},{f(y - 4 * s)} {f(x)},{f(y)} '
            f'Q{f(x + 3 * s)},{f(y - 4 * s)} {f(x + 7 * s)},{f(y - 2 * s)} Q{f(x + 3 * s)},{f(y - 1.5 * s)} {f(x)},{f(y + 1.5 * s)} '
            f'Q{f(x - 3 * s)},{f(y - 1.5 * s)} {f(x - 7 * s)},{f(y - 2 * s)}Z" fill="{INK}"/>')


def cloud(x, y, s, color):
    return (f'<path d="M{f(x)},{f(y)} q{f(6 * s)},{f(-10 * s)} {f(16 * s)},{f(-4 * s)} q{f(6 * s)},{f(-10 * s)} {f(16 * s)},0 '
            f'q{f(10 * s)},{f(-6 * s)} {f(14 * s)},{f(4 * s)} q{f(-4 * s)},{f(-5 * s)} {f(-9 * s)},{f(-1 * s)} '
            f'q{f(-3 * s)},{f(5 * s)} {f(2 * s)},{f(6 * s)} H{f(x)}Z" fill="{color}"/>')


@REGISTRY.sprite('title-land')
def landscape(hand):
    """The landscape under the title: sun, birds, mountains, firs, red clouds. 360 wide; the game sticks it to the
    bottom and scales it with the width."""
    firs_back = ''.join(fir(x, 300 - (x * 7 % 23), 60 + (x * 13 % 30), '#1c4a39') for x in range(-6, 370, 17))
    firs_front = ''.join(fir(x, 332, 70 + (x * 11 % 40), '#14301f') for x in range(4, 370, 26))
    return Sprite('title-land', 360, 330, '\n'.join([
        f'<circle cx="250" cy="150" r="54" fill="{SCARLET}" fill-opacity=".85"/>',
        f'<circle cx="250" cy="150" r="64" fill="none" stroke="{ROSE}" stroke-opacity=".35" stroke-width="2" stroke-dasharray="2 6"/>',
        bird(70, 70, 1.4), bird(96, 54, 1), bird(300, 66, 1.2),
        cloud(150, 190, 2.4, ROSE),
        '<path d="M-10,290 L90,170 L140,220 L200,140 L270,215 L320,180 L380,250 V330 H-10Z" fill="#245c47"/>',      # mountains with snowy caps
        f'<path d="M90,170 L108,192 L98,188 L90,198 L80,190 L74,190Z M200,140 L222,166 L210,160 L200,172 L190,160 L180,163Z" fill="{MINT}"/>',
        firs_back,
        cloud(10, 262, 2.2, SCARLET), cloud(250, 270, 1.8, ROSE),
        firs_front,
        f'<path d="M0,318 Q90,300 180,316 T360,312" fill="none" stroke="{SCARLET}" stroke-width="2.2" stroke-linecap="round"/>',
    ]), halo=None)
