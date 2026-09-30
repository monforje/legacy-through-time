LIST colours = red, (green), blue, yellow
LIST sizes = small = 1, medium = 5, large = 10
VAR inventory = ()
VAR mood = ()
-> start

=== start ===
Colours: {colours}. All: {LIST_ALL(colours)}. Count {LIST_COUNT(colours)}.
~ inventory += red
~ inventory += blue
~ inventory += small
Inventory: {inventory}. Has red: {inventory ? red}. Hasnt green: {inventory !? green}.
Min {LIST_MIN(inventory)} max {LIST_MAX(inventory)} value {LIST_VALUE(LIST_MAX(sizes))}.
Invert colours: {LIST_INVERT(colours)}.
~ inventory -= red
After removal: {inventory}.
Range: {LIST_RANGE(LIST_ALL(colours), 2, 3)}. Range by item: {LIST_RANGE(LIST_ALL(sizes), medium, large)}.
~ mood = LIST_ALL(colours)
Mood {mood}; intersect {mood ^ (red, blue)}.
Next colour after green: {green + 1}. Before blue: {blue - 1}.
Compare {(red) < (blue)} {(blue) > (red)} {(red, blue) == (blue, red)} {(red) != (blue)}.
Colour from int: {colours(3)}.
Random pick: {LIST_RANDOM(LIST_ALL(colours))}.
~ mood = ()
Empty mood: "{mood}" all again: {LIST_ALL(mood)}.
{inventory has blue: Carrying blue.}
* [Paint it red]
    ~ colours = red
    Now {colours}.
    -> END
* [Paint it yellow]
    ~ colours = yellow
    Now {colours}.
    -> END
