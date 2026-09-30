VAR x = 1
-> start

=== function trimmed() ===
    Function text with spaces   

=== function empty_line_fn() ===
~ x = x + 1

=== start ===
A <>
B.
   Leading spaces line.
C {trimmed()} D.
{trimmed()}
E
~ empty_line_fn()
F {x}
Before glue-newline
<> after glue.
Tag after content # t1 # t2
# lonely tag
Line after lonely tag.
Dynamic tag # score: {x * 10}
{x > 1: Conditional inline} text.
Multi <> 
<> glue.
* Choice with text[] and suffix # choice_tag
    After choice text.
* [Choice two # tag_two] Picked two.
- Gathered.
-> END
