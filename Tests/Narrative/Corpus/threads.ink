VAR visited_kitchen = false
-> house

=== house ===
You stand in the house.
<- kitchen_options
<- garden_options
+ [Leave] -> outside

=== kitchen_options ===
+ {not visited_kitchen} [Go to kitchen]
    ~ visited_kitchen = true
    The kitchen smells of bread.
    -> house
+ {visited_kitchen} [Kitchen again] Nothing new in the kitchen. -> house

=== garden_options ===
* [Go to garden] The garden is quiet.
    <- bird_thread
    * * [Stay] You stay a while. -> house
- -> DONE

=== bird_thread ===
* [Watch the bird] A bird flies away. -> house
- -> DONE

=== outside ===
Outside now. Kitchen visited: {visited_kitchen}. House seen {house} times.
-> END
