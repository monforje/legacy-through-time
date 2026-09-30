# title: Basics
# author: test
VAR gold = 3
VAR name = "Aru"
VAR ratio = 0.5
VAR flag = false
CONST MAX_GOLD = 10

Hello, {name}! You have {gold} gold. # greeting
It is {ratio * 3} and {7 / 2} and {7.0 / 2} and {7 % 3} and {-gold}. # math: yes
Glue test <>
continues here.
Line with    many   spaces   and a trailing tab	
{flag: Flag is on.|Flag is off.}
{gold > 2 && not flag: Rich and quiet.}
{gold == 3 or flag: Or works.}
String {"a" + "b"} and {name == "Aru"} and {name != "x"} and {"hello" ? "ell"} and {"hello" !? "z"}.
Compare {1 < 2} {2 <= 2} {3 > 4} {4 >= 4} {1.5 == 1.5} {true == 1}.
Funcs {MIN(3, 7)} {MAX(3, 7)} {POW(2, 10)} {FLOOR(2.7)} {CEILING(2.2)} {INT(3.9)} {FLOAT(4)} {INT("x" == "x")}.
-> hub

=== hub ===
You are at the hub (visit {hub}). # location: hub
{once: First time only.|Second time.|Third and after.}
{cycle: tick|tock}
{stopping: A|B|C}
{&alpha|beta|gamma} and {!once-a|once-b}
~ gold = gold + 1
* [Take the left path] -> left
* (right_choice) Take the right path -> right
+ [Wait]
    You wait. {~Nothing happens.|A bird sings.|Wind blows.}
    -> hub
* {gold > MAX_GOLD} [Secret rich option] -> END
* ->
    Fallback reached.
    -> END

=== left ===
Left side. # side: left
~ temp local = gold * 2
Local is {local}.
* * Nested choice A -> hub
* * Nested choice B
    - - (gatherB) Gathered at B.
    -> hub

=== right ===
Right side. Chose right {hub.right_choice} time(s). Turns: {TURNS()}. Since hub: {TURNS_SINCE(-> hub)}.
{flag:
    - true: flag branch
    - else: not flag branch
}
{
    - gold > 5: Much gold.
    - gold > 3: Some gold.
    - else: Little gold.
}
{ gold:
- 4: Exactly four.
- 5: Exactly five.
- else: Other amount {gold}.
}
-> hub
