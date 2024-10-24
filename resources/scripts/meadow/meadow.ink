INCLUDE ../includes.ink
INCLUDE robot.ink

=== function verb(thing_id, verb_id) ===
    VAR action_count = 0
    ~ action_count = get_action_count(thing_id, verb_id)
    
    { thing_id:
    - "cloud1":
        { verb_id:
        - "look":
            ~ talk("A cloud high above in the sky…")
            ~ move_rel(75, 20)
            ~ talk("A cloooooud high above in the skyyyy…")
            ~ move_rel(-50, 0)
            ~ talk("A cloooooooooooud high above in the skyyyyyyyyy…")
            ~ move_rel(-25, 0)
            ~ talk("Sorry.")
        - "talk_to": ~ talk("This cloud’s not talking back to me. What a shame!")
        - "give":
            ~ play_anim("left_hand_up")
        - else: ~ return false
        }

    - "cloud2":
        { verb_id:
        - "look":
            ~ talk("Another.")

        - "talk_to":
            ~ wait(2)
            ~ talk("I’m controlled by an Ink script!")
            ~ wait(0.5)
            ~ talk("Lets walk a bit to the right…")
            ~ move_rel(75, 0)
            ~ wait(0.5)
            ~ talk("And now to the left…")
            ~ move_rel(-150, 0)
            ~ wait(0.5)
            ~ talk("And I can also do custom animations, see?")
            ~ play_anim("left_hand_up")
            ~ talk("It’s great!")

        - else: ~ return false
        }

    - "cloud3":
        { verb_id:

        - "look": ~ talk("Yet another a cloud high above in the sky.")
        - "talk_to": ~ talk("Come on, cloud, talk to me!")
        - else: ~ return false
        }
    
    - "note":
        { verb_id:

        - "look":
            { is_in_inventory(thing_id) == false:
                ~ talk("A paper note.")
            - else:
                ~ talk("It’s a paper note. It reads: “Why are you reading this?”")
                ~ set_name("note", "Paper note with a pointless question")
            }
        - "pick_up":
            ~ pick_up(thing_id)
            ~ talk("I picked up the note.")
        - else: ~ return false
        }

    - "bush":
        { verb_id:
        - "look":
            {
                - action_count == 0:
                    ~ talk("Well, that’s a bush. Looks unsuspecting, right?")
                - else:
                    {
                        - action_count == 1:
                            ~ talk("Wait a second, someone left a coin in there!")
                            ~ create("coin")
                            ~ set_name(thing_id, "Generous bush")
                        - else:
                            ~ talk("I already found the coin in the bush.")
                    }
            }
        - "pick_up":
            ~ verb(thing_id, "look")
        - else: ~ return false
        }
    
    - "coin":
        { verb_id:
        - "look":
            ~ talk("A coin I found in the bush. It has a strange symbol on it.")
            ~ set_name("coin", "Strange coin")
        - else: ~ return false
        }

    - "robot":
        { verb_id:
        - "look":
            ~ talk("A robot standing in the meadow. It seems to be looking for something or someone.")
        - "talk_to":
            ~ start_dialog(thing_id)
        - "use":
            ~ talk("Erm...")
            ~ wait(1.0)
            ~ talk("It’s not *that* kind of robot.")
        - else: ~ return false
        }

    - "exit_right":
        { verb_id:
        - "walk":
            ~ switch_stage("Street", "default")
        - else: ~ return false
        }

    - else:
        ~ print_error("Unknown thing.")
        ~ return false
    }
    
    ~ return true