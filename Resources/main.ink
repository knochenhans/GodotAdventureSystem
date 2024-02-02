EXTERNAL print_error(text)
EXTERNAL display_message(text)
EXTERNAL pick_up(thing_id)
EXTERNAL is_in_inventory(thing_id)

-> END

=== function verb(thing_id, verb_id) ===
    { thing_id:
    
    - "cloud1":

        { verb_id:

        - "look": ~ display_message("A cloud high above in the sky.")
        - "talk_to": ~ display_message("This cloud’s not talking back to me. What a shame!")
        - else: ~ return false
        }

    - "cloud2":

        { verb_id:

        - "look": ~ display_message("Another a cloud high above in the sky")
        - "talk_to": ~ display_message("This cloud doesn’t seem eager to talk, either.")
        - else: ~ return false
        }

    - "cloud3":

        { verb_id:

        - "look": ~ display_message("Yet another a cloud high above in the sky")
        - "talk_to": ~ display_message("Come on, cloud, talk to me!")
        - else: ~ return false
        }
    
    - "note":

        { verb_id:

        - "look": ~ display_message("A paper note. It reads: “Why are you reading this?”")
        - "pick_up":
            { is_in_inventory(thing_id) == true:
                - print_error("I already have the note.")
            - else:
                ~ pick_up(thing_id)
                ~ display_message("I picked up the note.")
            }
        - else: ~ return false
        }

    - "bush":

        { verb_id:

        - "look": ~ display_message("Well, that’s a bush. Looks unsuspecting, right?")
        - else: ~ return false
        }

    - else:
        ~ print_error("Unknown thing.")
        ~ return false
    }
    
    ~ return true