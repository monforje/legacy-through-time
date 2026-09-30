VAR depth = 0
-> main

=== main ===
Start of main.
-> tunnel_a ->
Back in main after A.
-> tunnel_b ->
Back in main after B.
-> tunnel_override ->
This line should be skipped.
-> END

=== tunnel_a ===
~ depth++
In tunnel A (depth {depth}).
-> tunnel_nested ->
Still in A.
->->

=== tunnel_nested ===
~ depth++
Nested tunnel (depth {depth}).
* [Choice inside nested] Picked inside nested.
* [Other inside nested] Picked other.
- Gather in nested.
->->

=== tunnel_b ===
Tunnel B with a choice.
+ [Continue B] ->->
+ [Leave early] -> ending

=== tunnel_override ===
Override tunnel.
->-> ending

=== ending ===
The end (depth {depth}).
-> END
