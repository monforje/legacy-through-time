VAR target = -> room_a
VAR visits = 0
EXTERNAL host_add(a, b)
-> hub

=== function host_add(a, b) ===
~ return a + b

=== hub ===
~ visits++
Hub {visits}. External: {host_add(visits, 10)}.
{visits > 3:
    -> finale
}
* [Go to target] -> target
* [Switch target to B]
    ~ target = -> room_b
    Target switched.
    -> hub
* [Go with param] -> param_room(visits * 2)
* [Label check] -> labels
* [Jump into middle] -> counted.mid
* [Jump deep] -> counted.deep_stitch.deep_label
* [Enter counted normally] -> counted

=== room_a ===
Room A. Seen room B: {room_b}.
-> hub

=== room_b ===
Room B.
-> hub

=== param_room(n) ===
Param room got {n}.
{ n > 4: Large param. }
-> hub

=== labels ===
= inner
Inner stitch. Inner read count {inner}.
* (opt_one) [Option one] Chose one.
* (opt_two) [Option two] Chose two.
- (after) After labels: one {opt_one}, two {opt_two}, after {after}.
-> hub

=== finale ===
Finale: target was {target == -> room_b: B|A}.
-> END

=== counted ===
First line of counted.
- (mid) Middle of counted. counted={counted} mid={mid}
-> hub
= deep_stitch
Deep stitch start.
- (deep_label) Deep label. counted={counted} stitch={deep_stitch} label={deep_label}
-> hub
