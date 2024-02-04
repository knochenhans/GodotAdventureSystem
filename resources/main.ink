INCLUDE includes.ink

-> END

=== function verb(thing_id, verb_id) ===
    { thing_id:
    
    - "cloud1":

        { verb_id:

        - "look":
            ~ bubble("A cloud high above in the sky…")
            ~ move_relative(75, 20)
            ~ bubble("A cloooooud high above in the skyyyy…")
            ~ move_relative(-50, 0)
            ~ bubble("A cloooooooooooud high above in the skyyyyyyyyy…")
            ~ move_relative(-25, 0)
            ~ bubble("Sorry.")
        - "talk_to": ~ bubble("This cloud’s not talking back to me. What a shame!")
        - else: ~ return false
        }

    - "cloud2":

        { verb_id:

        - "look": ~ bubble("Another a cloud high above in the sky.")
        - "talk_to": ~ bubble("This cloud doesn’t seem eager to talk, either.")
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
                - get_variable("bush_looked_once") == false:
                    ~ bubble("Well, that’s a bush. Looks unsuspecting, right?")
                    ~ set_variable("bush_looked_once", true)
                - else:
                    {
                        - get_variable("bush_looked_twice") == false:
                            ~ bubble("Wait a second, someone left a coin in there!")
                            ~ create_object("coin")
                            ~ set_variable("bush_looked_twice", true)
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

    - else:
        ~ print_error("Unknown thing.")
        ~ return false
    }
    
    ~ return true