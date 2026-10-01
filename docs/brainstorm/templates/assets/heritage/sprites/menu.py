"""The main menu and the settings toast: the story card, the frame of its preview, the gear seal, the sound switch,
and the white glyph masks (sound, yurt = "to the menu", restart) that the game tints."""
import math
from functools import partial

from kit.geometry import chamfer, rhombus, rrect, shifted, sparkle
from kit.palette import CREAM, EMERALD, EMERALD_D, INK, PAPER, PAPER_L, SCARLET, SPRING, WHITE
from kit.registry import REGISTRY, Slice
from kit.svg import GRAIN, Sprite, grain


@REGISTRY.sprite('card', Slice(26, 26, 26, 30))
def card(hand):
    """The story card and the settings toast: paper with an ink outline, the hard shadow +4, a scarlet hairline and a
    rhombus in each corner (all inside the 26 px corners). 96x96: body 1.5..90.5, slice 26 / 30 at the bottom."""
    body = chamfer(1.5, 1.5, 94.5, 90.5, 8)
    fill = hand.wobble(body, step=12)
    corners = ''.join(rhombus(x, y, 3, SCARLET, INK, 1.2) for x, y in ((13, 13), (83, 13), (13, 79), (83, 79)))
    return Sprite('card', 96, 96, '\n'.join([
        GRAIN,
        f'<path d="{hand.wobble(shifted(body, 4), step=12)}" fill="{INK}"/>',
        f'<path d="{fill}" fill="{PAPER}"/>',
        grain(fill),
        f'<path d="{fill}" fill="none" stroke="{INK}" stroke-width="2.5" stroke-linejoin="round"/>',
        f'<path d="{hand.wobble(chamfer(7, 7, 89, 85, 5), step=12, amp=.15)}" fill="none" stroke="{SCARLET}" stroke-width="1.3"/>',
        corners,
    ]), 'border-image: slice 26 26 30 fill')


@REGISTRY.sprite('frame', Slice.uniform(10, fill=False))
def frame(hand):
    """The frame laid over a preview picture: an ink line with a cream hairline inside; the corners are ink, so the
    square corners of the picture under it are hidden. 32x32, slice 10, no centre."""
    return Sprite('frame', 32, 32, '\n'.join([
        f'<path d="{rrect(1.5, 1.5, 30.5, 30.5, 3)}" fill="none" stroke="{INK}" stroke-width="3"/>',
        f'<path d="{rrect(4, 4, 28, 28, 1.5)}" fill="none" stroke="{CREAM}" stroke-opacity=".55" stroke-width="1"/>',
    ]), 'border-image: slice 10')


@REGISTRY.sprite('gear')
def gear(hand):
    """The settings button: the dark round seal (like `round`) with a cream eight-tooth gear. 44x46."""
    teeth = []
    for i in range(8):
        a = i * math.tau / 8
        teeth.append(f'<rect x="-2.6" y="-11.2" width="5.2" height="5" rx="1" transform="translate(22,22) rotate({math.degrees(a):.1f})" fill="{CREAM}"/>')
    return Sprite('gear', 44, 46, '\n'.join([
        f'<circle cx="22" cy="24.5" r="20" fill="{INK}"/>',
        f'<circle cx="22" cy="22" r="20" fill="{EMERALD_D}" stroke="{INK}" stroke-width="2.5"/>',
        f'<circle cx="22" cy="22" r="16.2" fill="none" stroke="{CREAM}" stroke-opacity=".4" stroke-width="1" stroke-dasharray="2 2.6"/>',
        ''.join(teeth),
        f'<circle cx="22" cy="22" r="7.6" fill="none" stroke="{CREAM}" stroke-width="3.6"/>',
        f'<circle cx="22" cy="22" r="2" fill="{SCARLET}"/>',
    ]))


def switch(hand, name, on):
    """The sound switch, 60x34: a pill track (emerald when on, ink-dark when off) and a paper knob with a rhombus."""
    track = EMERALD if on else '#5b4a43'
    knob_x = 41 if on else 17
    return Sprite(name, 60, 34, '\n'.join([
        f'<path d="{rrect(1.5, 4.5, 58.5, 32.5, 14)}" fill="{INK}"/>',
        f'<path d="{rrect(1.5, 1.5, 58.5, 29.5, 14)}" fill="{track}" stroke="{INK}" stroke-width="2.5"/>',
        f'<path d="{rrect(5, 5, 55, 26, 10.5)}" fill="none" stroke="{SPRING if on else CREAM}" stroke-opacity=".45" stroke-width="1"/>',
        f'<circle cx="{knob_x}" cy="17" r="10.5" fill="{INK}"/>',
        f'<circle cx="{knob_x}" cy="15.5" r="10.5" fill="{PAPER_L}" stroke="{INK}" stroke-width="2.2"/>',
        rhombus(knob_x, 15.5, 3.4, SCARLET if on else '#b9a99a'),
    ]))


REGISTRY.add('switch-on', partial(switch, name='switch-on', on=True))
REGISTRY.add('switch-off', partial(switch, name='switch-off', on=False))


# ---- glyph masks: white, the game tints them (and white is also the colour of their bed)
SPEAKER = 'M4,12 H9 L16,6 V26 L9,20 H4Z'


@REGISTRY.sprite('sound')
def sound(hand):
    """A speaker with two waves. 32x32."""
    return Sprite('sound', 32, 32, '\n'.join([
        f'<path d="{SPEAKER}" fill="{WHITE}" stroke="{WHITE}" stroke-width="1.6" stroke-linejoin="round"/>',
        f'<path d="M20.5,11.5 Q23.5,16 20.5,20.5 M24.5,7.5 Q30,16 24.5,24.5" fill="none" stroke="{WHITE}" stroke-width="2.6" stroke-linecap="round"/>',
    ]), halo=WHITE)


@REGISTRY.sprite('sound-off')
def sound_off(hand):
    """The speaker with a cross instead of the waves. 32x32."""
    return Sprite('sound-off', 32, 32, '\n'.join([
        f'<path d="{SPEAKER}" fill="{WHITE}" stroke="{WHITE}" stroke-width="1.6" stroke-linejoin="round"/>',
        f'<path d="M20.5,12 L28.5,20 M28.5,12 L20.5,20" fill="none" stroke="{WHITE}" stroke-width="2.6" stroke-linecap="round"/>',
    ]), halo=WHITE)


@REGISTRY.sprite('home')
def home(hand):
    """A yurt: "back to the menu". 32x32."""
    return Sprite('home', 32, 32, '\n'.join([
        f'<path d="M3.5,16 Q16,3 28.5,16 V27.5 H3.5Z" fill="none" stroke="{WHITE}" stroke-width="2.6" stroke-linejoin="round"/>',
        f'<path d="M16,6 V3.5 M3.5,16 H28.5" stroke="{WHITE}" stroke-width="2.6" stroke-linecap="round"/>',
        f'<path d="M12.5,27.5 V21 Q16,18 19.5,21 V27.5Z" fill="{WHITE}"/>',
    ]), halo=WHITE)


@REGISTRY.sprite('restart')
def restart(hand):
    """A circular arrow: "from the beginning". 32x32."""
    return Sprite('restart', 32, 32, '\n'.join([
        f'<path d="M24.5,10.5 A10,10 0 1 0 26,17.5" fill="none" stroke="{WHITE}" stroke-width="2.8" stroke-linecap="round"/>',
        f'<path d="M26.5,3.5 V11.5 H18.5" fill="none" stroke="{WHITE}" stroke-width="2.8" stroke-linecap="round" stroke-linejoin="round"/>',
    ]), halo=WHITE)


@REGISTRY.sprite('star')
def star(hand):
    """The sparkle of a finished story on its card. 24x24."""
    return Sprite('star', 24, 24, sparkle(12, 12, 10.5, SCARLET, INK, 1.6))
