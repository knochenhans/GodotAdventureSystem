INCLUDE includes.ink

=== function verb(thing_id, verb_id) ===
    { thing_id:
    
    - "cloud1":

        { verb_id:

        - "look":
            ~ bubble("A cloud high above in the sky…")
            ~ move_rel(75, 20)
            ~ bubble("A cloooooud high above in the skyyyy…")
            ~ move_rel(-50, 0)
            ~ bubble("A cloooooooooooud high above in the skyyyyyyyyy…")
            ~ move_rel(-25, 0)
            ~ bubble("Sorry.")
        - "talk_to": ~ bubble("This cloud’s not talking back to me. What a shame!")
        - "give":
            ~ play_anim("left_hand_up")
        - else: ~ return false
        }

    - "cloud2":

        { verb_id:

        - "look":
            ~ bubble("Another.")
            ~ bubble("Another a cloud high above in the sky.")
            ~ bubble("Another a cloud high above in the sky. Really. Really. Really.")
        - "talk_to":
            ~ wait(2)
            ~ bubble("I’m controlled by an Ink script!")
            ~ wait(0.5)
            ~ bubble("Lets walk a bit to the right…")
            ~ move_rel(75, 0)
            ~ wait(0.5)
            ~ bubble("And now to the left…")
            ~ move_rel(-150, 0)
            ~ wait(0.5)
            ~ bubble("And I can also do custom animations, see?")
            ~ play_anim("left_hand_up")
            ~ bubble("It’s great!")

        - else: ~ return false
        }

    - "cloud3":

        { verb_id:

        - "look": ~ bubble("Yet another a cloud high above in the sky.")
        - "talk_to": ~ bubble("Come on, cloud, talk to me!")
        - else: ~ return false
        }
    
    - "note":

        { verb_id:

        - "look":
            { is_in_inventory(thing_id) == false:
                ~ bubble("A paper note.")
            - else:
                ~ bubble("It’s a paper note. It reads: “Why are you reading this?”")
                ~ set_name("note", "Paper note with a pointless question")
            }
        - "pick_up":
            ~ pick_up(thing_id)
            ~ bubble("I picked up the note.")
        - else: ~ return false
        }

    - "bush":

        { verb_id:

        - "look":
            {
                - get_var("bush_looked_once") == false:
                    ~ bubble("Well, that’s a bush. Looks unsuspecting, right?")
                    ~ set_var("bush_looked_once", true)
                - else:
                    {
                        - get_var("bush_looked_twice") == false:
                            ~ bubble("Wait a second, someone left a coin in there!")
                            ~ create("coin")
                            ~ set_var("bush_looked_twice", true)
                            ~ set_name(thing_id, "Generous bush")
                        - else:
                            ~ bubble("I already found the coin in the bush.")
                    }
            }
        - else: ~ return false
        }
    
    - "coin":
    
        { verb_id:

        - "look":
            ~ bubble("A coin I found in the bush. It has a strange symbol on it.")
            ~ set_name("coin", "Strange coin")
        - else: ~ return false
        }

    - "robot":
    
            { verb_id:
    
            - "look":
                ~ bubble("A robot standing in meadow. It seems to be looking for something or someone.")
            - "talk_to":
                ~ dialog(thing_id)
            - else: ~ return false
            }

    - else:
        ~ print_error("Unknown thing.")
        ~ return false
    }
    
    ~ return true

=== robot ===

Hi there! #player

Hello, fleshperson. #robot

Weird meeting a robot out here in the nature. #player

You think so? #robot

~ play_anim_char("robot", "idle")
~ wait(0.5)

Well doesn’t seem weird at all to me, you know? #robot

So what do you want, fleshperson? #robot

    * [Who are you?]
        My name is… err, I have no idea to be honest. No one ever asked me that before. #robot

        Wait, you don’t know your own name? #player
    * [What are you doing here?]
        I’m looking for someone. #robot

        ~ play_anim_char("robot", "idle")
        ~ wait(0.5)

- -> END