"""Name tags: the sticker on the top edge of a plate, its ornaments."""
from functools import partial

from kit.geometry import rhombus, shifted
from kit.palette import CREAM, EMERALD, INK, ROSE, SCARLET
from kit.registry import REGISTRY, Slice
from kit.svg import GRAIN, Sprite, grain


def tag(hand, name, fill):
    """80x36, slice 15 / 14 / 17. Slanted, with chipped edges; the sticker shadow +3; body 1..32. No inner frame and
    no white highlight: on a big screen they read as a white rectangle inside the tag."""
    body = [(6, 2.5), (40, 1.8), (74, 1.2), (78.5, 4), (77, 11), (79, 21), (75, 31), (40, 32), (5, 31.4),
            (2, 28), (3.5, 21), (1.5, 12), (3, 6)]
    outline = hand.wobble(body, step=10, amp=.5)      # the same path for the fill and the grain
    return Sprite(name, 80, 36, '\n'.join([
        GRAIN,
        f'<path d="{hand.wobble(shifted(body, 3), step=10)}" fill="{INK}"/>',
        f'<path d="{outline}" fill="{fill}" stroke="{INK}" stroke-width="3" stroke-linejoin="round"/>',
        grain(outline),
        '<path d="M12,29.6 H68" stroke="#000" stroke-opacity=".28" stroke-width="2" stroke-linecap="round"/>',
    ]), 'border-image: slice 15 14 17 fill')


REGISTRY.add('label', partial(tag, name='label', fill=EMERALD), Slice(15, 14, 14, 17))
REGISTRY.add('label-rose', partial(tag, name='label-rose', fill=ROSE), Slice(15, 14, 14, 17))


@REGISTRY.sprite('gem')
def gem(hand):
    """The gem in a black frame at the end of the "Факт" tag."""
    return Sprite('gem', 28, 28, '\n'.join([
        rhombus(14, 15.5, 11.5, INK),
        rhombus(14, 14, 11.5, SCARLET, INK, 2.2),
        rhombus(14, 14, 6.2, INK),
        rhombus(14, 14, 3.4, SCARLET),
    ]))


@REGISTRY.sprite('label-orn')
def tag_ornament(hand):
    return Sprite('label-orn', 10, 10, rhombus(5, 5, 3.8, SCARLET, CREAM, 1.2))
