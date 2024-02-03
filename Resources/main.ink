EXTERNAL print_error(text)
EXTERNAL display_message(text)
EXTERNAL pick_up(object_id)
EXTERNAL create_object(object_id)
EXTERNAL is_in_inventory(thing_id)
EXTERNAL set_variable(id, value)
EXTERNAL get_variable(name)
EXTERNAL set_name(thing_id, name)
EXTERNAL wait(time)

-> END

=== function verb(thing_id, verb_id) ===
    { thing_id:
    
    - "cloud1":

        { verb_id:

        - "look":
            ~ display_message("A cloud high above in the sky…")
            ~ wait(5)
            ~ display_message("A cloooooud high above in the skyyyy…")
            ~ wait(5)
            ~ display_message("A cloooooooooooud high above in the skyyyyyyyyy…")
        - "talk_to": ~ display_message("This cloud’s not talking back to me. What a shame!")
        - else: ~ return false
        }

    - "cloud2":

        { verb_id:

        - "look": ~ display_message("Another a cloud high above in the sky.")
        - "talk_to": ~ display_message("This cloud doesn’t seem eager to talk, either.")
        - else: ~ return false
        }

    - "cloud3":

        { verb_id:

        - "look": ~ display_message("Yet another a cloud high above in the sky.")
        - "talk_to": ~ display_message("Come on, cloud, talk to me!")
        - else: ~ return false
        }
    
    - "note":

        { verb_id:

        - "look":
            { is_in_inventory(thing_id) == false:
                ~ display_message("A paper note.")
            - else:
                ~ display_message("It’a a paper note. It reads: “Why are you reading this?”")
                ~ set_name("note", "Paper note with a pointless question")
            }
        - "pick_up":
            ~ pick_up(thing_id)
            ~ display_message("I picked up the note.")
        - else: ~ return false
        }

    - "bush":

        { verb_id:

        - "look":
            {
                - get_variable("bush_looked_once") == false:
                    ~ display_message("Well, that’s a bush. Looks unsuspecting, right?")
                    ~ set_variable("bush_looked_once", true)
                - else:
                    {
                        - get_variable("bush_looked_twice") == false:
                            ~ display_message("Wait a second, someone left a coin in there!")
                            ~ create_object("coin")
                            ~ set_variable("bush_looked_twice", true)
                            ~ set_name(thing_id, "Generous bush")
                        - else:
                            ~ display_message("I already found the coin in the bush.")
                    }
            }
        - else: ~ return false
        }
    
    - "coin":
    
        { verb_id:

        - "look":
            ~ display_message("A coin I found in the bush. It has a strange symbol on it.")
            ~ set_name("coin", "Strange coin")
        - else: ~ return false
        }

    - else:
        ~ print_error("Unknown thing.")
        ~ return false
    }
    
    ~ return true