VAR rolls = 0
-> roll

=== roll ===
~ rolls++
Dice: {RANDOM(1, 6)} and {RANDOM(10, 20)}. Shuffle: {~a|b|c|d|e}. Shuffle once: {~!x|y|z}.
{shuffle:
- Shuffled one.
- Shuffled two.
- Shuffled three.
}
{rolls == 3:
    ~ SEED_RANDOM(42)
    Reseeded.
}
+ {rolls < 8} [Roll again] -> roll
+ [Stop] -> END
