"""Controls: option buttons (3 states), arrow keys, the value field, the stat ribbon, the balance pill, the plus,
the round seal of the collapse arrow, the chevron masks."""
from functools import partial

from kit.geometry import chamfer, hexbar, rhombus, rrect, shifted
from kit.palette import CREAM, EMERALD, EMERALD_D, INK, PAPER_L, SCARLET, SPRING, WHITE
from kit.registry import REGISTRY, Slice
from kit.svg import Sprite


def button(hand, name, fill, line, line_opacity=.5):
    """120x50: body 1.5..45.5, shadow +3; slice 16 top, 20 bottom, 18 on the sides."""
    body = hexbar(1.5, 1.5, 118.5, 45.5, 9)
    return Sprite(name, 120, 50, '\n'.join([
        f'<path d="{hand.wobble(shifted(body, 3), step=12)}" fill="{INK}"/>',
        f'<path d="{hand.wobble(body, step=12)}" fill="{fill}" stroke="{INK}" stroke-width="2.5" stroke-linejoin="round"/>',
        f'<path d="{hand.wobble(hexbar(6, 6, 114, 41, 6.6), step=12, amp=.15)}" fill="none" stroke="{line}" stroke-opacity="{line_opacity}" stroke-width="1.1"/>',
        '<path d="M16,5.2 H104" stroke="#fff" stroke-opacity=".32" stroke-width="1.6" stroke-linecap="round"/>',
        '<path d="M16,41.6 H104" stroke="#000" stroke-opacity=".3" stroke-width="2.6" stroke-linecap="round"/>',
    ]), 'border-image: slice 16 18 20 fill')


for _name, _fill, _line, _opacity in (('btn', EMERALD, CREAM, .5), ('btn-pressed', SCARLET, CREAM, .7), ('btn-chosen', PAPER_L, SCARLET, .9)):
    REGISTRY.add(_name, partial(button, name=_name, fill=_fill, line=_line, line_opacity=_opacity), Slice(16, 18, 18, 20))


def key(hand, name, fill):
    """The square arrow button 48x48: body 1.5..44.5, shadow +3; slice 16."""
    body = chamfer(1.5, 1.5, 46.5, 44.5, 6)
    return Sprite(name, 48, 48, '\n'.join([
        f'<path d="{hand.wobble(shifted(body, 3))}" fill="{INK}"/>',
        f'<path d="{hand.wobble(body)}" fill="{fill}" stroke="{INK}" stroke-width="2.5" stroke-linejoin="round"/>',
        f'<path d="{hand.wobble(chamfer(5.5, 5.5, 42.5, 40.5, 4), amp=.15)}" fill="none" stroke="{CREAM}" stroke-opacity=".5" stroke-width="1"/>',
        '<path d="M10,5.3 H38" stroke="#fff" stroke-opacity=".32" stroke-width="1.6" stroke-linecap="round"/>',
        '<path d="M10,41 H38" stroke="#000" stroke-opacity=".3" stroke-width="2.4" stroke-linecap="round"/>',
    ]), 'border-image: slice 16 fill')


REGISTRY.add('key', partial(key, name='key', fill=EMERALD), Slice.uniform(16))
REGISTRY.add('key-pressed', partial(key, name='key-pressed', fill=SCARLET), Slice.uniform(16))


@REGISTRY.sprite('field', Slice.uniform(14))
def field(hand):
    """The field of the value between the arrows. 48x48, slice 14."""
    return Sprite('field', 48, 48, '\n'.join([
        f'<path d="{rrect(1.25, 1.25, 46.75, 46.75, 12)}" fill="{PAPER_L}" stroke="{INK}" stroke-width="2.5"/>',
        f'<path d="{rrect(5, 5, 43, 43, 8.5)}" fill="none" stroke="{SPRING}" stroke-width="1.2" stroke-dasharray="1 3" stroke-linecap="round"/>',
    ]), 'border-image: slice 14 fill')


@REGISTRY.sprite('banner', Slice(18, 30, 30, 22))
def banner(hand):
    """The ribbon of a changed stat, with notches at the ends. 120x56, slice 18 / 30 / 22."""
    width, y0, y1 = 120, 1.5, 50.5
    mid = (y0 + y1) / 2

    def shape(dy):
        return [(1.5, y0 + dy), (width - 1.5, y0 + dy), (width - 9, mid + dy), (width - 1.5, y1 + dy), (1.5, y1 + dy), (9, mid + dy)]
    parts = [
        f'<path d="{hand.wobble(shape(3), step=12)}" fill="{INK}"/>',
        f'<path d="{hand.wobble(shape(0), step=12)}" fill="{EMERALD}" stroke="{INK}" stroke-width="2.5" stroke-linejoin="round"/>',
    ]
    for y in (7, 45):
        parts.append(f'<path d="M20,{y} H{width - 20}" stroke="{CREAM}" stroke-opacity=".6" stroke-width="1.1"/>')
        parts += [rhombus(16, y, 2.6, SCARLET, CREAM, .8), rhombus(width - 16, y, 2.6, SCARLET, CREAM, .8)]
    return Sprite('banner', width, 56, '\n'.join(parts), 'border-image: slice 18 30 22 fill')


@REGISTRY.sprite('pill', Slice(20, 24, 24, 24))
def pill(hand):
    """The coin balance. 96x48: body 1.5..44.5 (radius 21.5), shadow +3; slice 20 / 24 / 24."""
    return Sprite('pill', 96, 48, '\n'.join([
        f'<path d="{rrect(1.5, 4.5, 94.5, 47.5, 21.5)}" fill="{INK}"/>',
        f'<path d="{rrect(1.5, 1.5, 94.5, 44.5, 21.5)}" fill="{EMERALD_D}" stroke="{INK}" stroke-width="2.5"/>',
        f'<path d="{rrect(5.5, 5.5, 90.5, 40.5, 17.5)}" fill="none" stroke="{CREAM}" stroke-opacity=".35" stroke-width="1"/>',
        '<path d="M24,6 H72" stroke="#fff" stroke-opacity=".25" stroke-width="1.6" stroke-linecap="round"/>',
        '<path d="M24,41 H72" stroke="#000" stroke-opacity=".32" stroke-width="2.6" stroke-linecap="round"/>',
    ]), 'border-image: slice 20 24 24 fill')


@REGISTRY.sprite('plus')
def plus(hand):
    return Sprite('plus', 32, 34, '\n'.join([
        f'<circle cx="16" cy="18" r="14.5" fill="{INK}"/>',
        f'<circle cx="16" cy="16" r="14.5" fill="{EMERALD}" stroke="{INK}" stroke-width="2"/>',
        f'<circle cx="16" cy="16" r="11.5" fill="none" stroke="{CREAM}" stroke-opacity=".45" stroke-width="1"/>',
        f'<path d="M16,9.5 V22.5 M9.5,16 H22.5" stroke="{CREAM}" stroke-width="3.2" stroke-linecap="round"/>',
    ]))


@REGISTRY.sprite('round')
def round_seal(hand):
    """The dark round seal under the collapse arrow."""
    return Sprite('round', 40, 42, '\n'.join([
        f'<circle cx="20" cy="22" r="18" fill="{INK}"/>',
        f'<circle cx="20" cy="20" r="18" fill="{EMERALD_D}" stroke="{INK}" stroke-width="2.5"/>',
        f'<circle cx="20" cy="20" r="14.5" fill="none" stroke="{CREAM}" stroke-opacity=".4" stroke-width="1" stroke-dasharray="2 2.6"/>',
    ]))


# The chevrons are masks: white, the game tints them (and white is also the colour of their bed).
@REGISTRY.sprite('chevron')
def chevron(hand):
    return Sprite('chevron', 16, 20, f'<path d="M4,3 L12.5,10 L4,17" fill="none" stroke="{WHITE}" stroke-width="3.6" stroke-linecap="round" stroke-linejoin="round"/>',
                  halo=WHITE)


@REGISTRY.sprite('chevron-down')
def chevron_down(hand):
    return Sprite('chevron-down', 22, 14, f'<path d="M3,3 L11,11 L19,3" fill="none" stroke="{WHITE}" stroke-width="4" stroke-linecap="round" stroke-linejoin="round"/>',
                  halo=WHITE)
