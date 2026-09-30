VAR counter = 0
VAR result = 0

-> start

=== function add(a, b) ===
~ return a + b

=== function fact(n) ===
{ n <= 1:
    ~ return 1
- else:
    ~ return n * fact(n - 1)
}

=== function describe(x) ===
    {x > 5:
        big
    - else:
        small
    }

=== function greet(who) ===
Hello, <>
{who}<>
!

=== function inc(ref v) ===
~ v = v + 1

=== function noisy() ===
~ counter++
~ return counter

=== function void_fn() ===
Void function text.

=== start ===
Add: {add(2, 3)}. Fact: {fact(6)}. Describe: {describe(10)} and {describe(1)}.
{greet("World")}
Before {greet("inline")} after.
~ inc(counter)
~ inc(counter)
Counter after inc: {counter}.
~ temp t = 10
~ inc(t)
Temp after inc: {t}.
Noisy: {noisy()} {noisy()} {noisy()}.
~ void_fn()
Mixed {add(1,1)} words {fact(3)} here.
~ result = add(fact(3), 4)
Result {result}.
* [One] -> one
* [Two] -> two

=== one ===
In one.
{ noisy() > 3: noisy big | noisy small }
-> END

=== two ===
In two: {add(counter, 100)}.
-> END
