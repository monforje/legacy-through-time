"""Icons of the heads-up parts: the coin, the timer ring, the stat icon, the pie disc, the web version's tail."""
import math

from kit.geometry import f, rrect
from kit.palette import BRONZE, BRONZE_D, BRONZE_L, CREAM, EMERALD_D, INK, PAPER, SCARLET, WHITE
from kit.registry import REGISTRY
from kit.svg import GRAIN, Sprite, grain


@REGISTRY.sprite('coin')
def coin(hand):
    """The coin with a tulip instead of a 💎. A flat print: no metallic gradients (DESIGN.md)."""
    dots = ''.join(
        f'<circle cx="{f(24 + 17.6 * math.cos(a))}" cy="{f(24 + 17.6 * math.sin(a))}" r=".95" fill="{BRONZE_D}"/>'
        for a in (i * math.tau / 20 for i in range(20)))
    tulip = (f'<path d="M24,13 C27,16 28.5,19 28,23 C30,21 32,18.5 33,16 C34,22 31,28.5 24,30 '
             f'C17,28.5 14,22 15,16 C16,18.5 18,21 20,23 C19.5,19 21,16 24,13Z" fill="{BRONZE_D}"/>'
             f'<path d="M24,30 V36 M24,34 C21,34 19,32.5 18,31 M24,34 C27,34 29,32.5 30,31" fill="none" '
             f'stroke="{BRONZE_D}" stroke-width="1.8" stroke-linecap="round"/>'
             f'<path d="M24,15.5 C25.5,17.5 26,20 25.6,23" fill="none" stroke="{BRONZE_L}" stroke-width="1.2" stroke-linecap="round"/>')
    return Sprite('coin', 48, 48, '\n'.join([
        f'<circle cx="24" cy="25.5" r="21.5" fill="{INK}"/>',
        f'<circle cx="24" cy="24" r="21.5" fill="{BRONZE}" stroke="{INK}" stroke-width="2.5"/>',
        f'<circle cx="24" cy="24" r="15" fill="none" stroke="{BRONZE_D}" stroke-width="1.4"/>',
        dots, tulip,
        f'<path d="M9.5,18 A15.5,15.5 0 0 1 18,9.5" fill="none" stroke="{BRONZE_L}" stroke-width="2" stroke-linecap="round"/>',
    ]))


@REGISTRY.sprite('timer')
def timer_ring(hand):
    """The ring of the timer, 60x60, over a 50x50 pie that the game draws itself."""
    ticks = ''.join(
        f'<path d="M{f(30 + 22.2 * math.cos(a))},{f(30 + 22.2 * math.sin(a))} L{f(30 + 24.6 * math.cos(a))},{f(30 + 24.6 * math.sin(a))}" '
        f'stroke="{INK}" stroke-width="1.3" stroke-linecap="round"/>'
        for a in (i * math.tau / 12 for i in range(12)))
    flame = (f'<path d="M30,0.8 C33.5,4 34,7 32.8,9.3 C32,10.8 30.8,11.4 30,11.4 C28.4,11.4 26.8,10 27,7.8 '
             f'C27.2,6.4 28.2,5.6 28.6,4.4 C29.6,5.6 29.8,6.6 29.6,7.6 C31,6.6 31.4,4 30,0.8Z" '
             f'fill="{SCARLET}" stroke="{INK}" stroke-width="1.3" stroke-linejoin="round"/>')
    return Sprite('timer', 60, 60, '\n'.join([
        f'<circle cx="30" cy="30" r="26" fill="none" stroke="{INK}" stroke-width="4"/>',
        f'<circle cx="30" cy="30" r="23.4" fill="none" stroke="{SCARLET}" stroke-width="2.4"/>',
        ticks, flame,
    ]))


@REGISTRY.sprite('stat-steppe')
def stat_steppe(hand):
    """The icon of the stat "Память степи": feather grass and the sun in a tile."""
    blades = ''.join(
        f'<path d="M{x0},28 Q{qx},{qy} {x1},{y1}" fill="none" stroke="{CREAM}" stroke-width="2" stroke-linecap="round"/>'
        for x0, qx, qy, x1, y1 in ((16, 14, 18, 8, 10), (17, 17, 16, 15, 6.5), (18, 20, 15, 22, 7),
                                   (19, 22, 19, 27, 12), (15, 12, 22, 7, 20)))
    return Sprite('stat-steppe', 34, 34, '\n'.join([
        f'<path d="{rrect(1.25, 1.25, 32.75, 32.75, 6)}" fill="{EMERALD_D}" stroke="{CREAM}" stroke-width="2"/>',
        f'<circle cx="25.5" cy="9" r="3.2" fill="{SCARLET}"/>',
        blades,
        f'<path d="M6,28.5 H28" stroke="{CREAM}" stroke-width="2" stroke-linecap="round"/>',
    ]))


@REGISTRY.sprite('disc')
def disc(hand):
    """A white disc: the fill of the timer's pie (the game tints it)."""
    return Sprite('disc', 64, 64, f'<circle cx="32" cy="32" r="32" fill="{WHITE}"/>', halo=WHITE)


@REGISTRY.sprite('tail')
def tail(hand):
    """The tail of the WEB version (heritage.css draws it over the plate as a pseudo-element). Unity does not use it:
    there the horn is part of plate-tail-r / plate-tail-l. 60x46; the plate outline at the base is at y=32.25..35.75."""
    left_base, right_base, line = 26, 58, 34
    left = f'M{left_base},{line + 1.5} C24,26 8,19 3,2'
    right = f'M3,2 C17,20 34,{line} {right_base + 4},{line}'
    fill = f'M{left_base},{line + 1.5} C24,26 8,19 3,2 C17,20 34,{line} {right_base},{line} L{right_base},46 L{left_base},46Z'
    return Sprite('tail', 60, 46, '\n'.join([
        GRAIN,
        '<defs><clipPath id="c"><rect width="60" height="35.75"/></clipPath></defs>',
        f'<path d="{fill}" fill="{PAPER}"/>',
        grain(fill),
        f'<path d="M12,12 C22,24 34,31 {right_base - 2},31.6" fill="none" stroke="{WHITE}" stroke-opacity=".35" stroke-width="1.6" stroke-linecap="round"/>',
        f'<path d="M{left_base + 1},38.5 C23,31 10,25 8.5,9.5 C18,23 34,38.5 {right_base},38.5" fill="none" stroke="{SCARLET}" stroke-width="2" stroke-linejoin="round" stroke-linecap="butt"/>',
        f'<g clip-path="url(#c)" fill="none" stroke="{INK}" stroke-width="3.5" stroke-linejoin="miter" stroke-miterlimit="14">'
        f'<path d="{left}"/><path d="{right}"/></g>',
    ]), 'tail: the plate outline at the base is at y=32.25..35.75')
