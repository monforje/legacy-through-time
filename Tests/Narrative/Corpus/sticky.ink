VAR n = 0
-> loop

=== loop ===
~ n++
Loop {n}. Choice count before: {CHOICE_COUNT()}.
+ [Sticky] -> loop
* [Once A] -> loop
* [Once B] -> loop
* {n > 2} [Conditional once] -> loop
+ {n > 5} [Exit] -> done
* ->
    All once-only used up.
    -> done

=== done ===
Done after {n} loops. Loop visited {loop} times.
-> END
