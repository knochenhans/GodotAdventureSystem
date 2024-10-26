INCLUDE ../includes.ink
INCLUDE robot.ink

=== function verb(thing_id, verb_id) ===
    VAR action_count = 0
    ~ action_count = get_action_count(thing_id, verb_id)
    
    { thing_id:
    - "cloud1":
        { verb_id:
        - "look":
            This is a test! #player
            A cloud high above in the sky… #player
            ~ move_rel(75, 20)
            A cloooooud high above in the skyyyy… #player
            ~ move_rel(-50, 0)
            A cloooooooooooud high above in the skyyyyyyyyy… #player
            ~ move_rel(-25, 0)
            Sorry. #player
        - "talk_to":
            This cloud’s not talking back to me. What a shame! #player
        - "give":
            ~ play_anim("left_hand_up")
        - else: ~ return false
        }

    - "cloud2":
        { verb_id:
        - "look":
            Another. #player

        - "talk_to":
            ~ wait(2)
            I’m controlled by an Ink script! #player
            ~ wait(0.5)
            Lets walk a bit to the right… #player
            ~ move_rel(75, 0)
            ~ wait(0.5)
            And now to the left… #player
            ~ move_rel(-150, 0)
            ~ wait(0.5)
            And I can also do custom animations, see? #player
            ~ play_anim("left_hand_up")
            It’s great! #player

        - else: ~ return false
        }

    - "cloud3":
        { verb_id:
        - "look": Yet another a cloud high above in the sky. #player
        - "talk_to": Come on, cloud, talk to me! #player
        - else: ~ return false
        }
    
    - "note":
        { verb_id:
        - "look":
            { is_in_inventory(thing_id) == false:
                A paper note. #player
            - else:
                It’s a paper note. It reads: “Why are you reading this?” #player
                ~ set_name("note", "Paper note with a pointless question")
            }
        - "pick_up":
            ~ pick_up(thing_id)
            I picked up the note. #player
        - else: ~ return false
        }

    - "bush":
        { verb_id:
        - "look":
            {
                - action_count == 0:
                    Well, that’s a bush. Looks unsuspecting, right? #player
                - else:
                    {
                        - action_count == 1:
                            Wait a second, someone left a coin in there! #player
                            ~ create("coin")
                            ~ set_name(thing_id, "Generous bush")
                        - else:
                            I already found the coin in the bush. #player
                    }
            }
        - "pick_up":
            ~ verb(thing_id, "look")
        - else: ~ return false
        }
    
    - "coin":
        { verb_id:
        - "look":
            A coin I found in the bush. It has a strange symbol on it. #player
            ~ set_name("coin", "Strange coin")
        - else: ~ return false
        }

    - "robot":
        { verb_id:
        - "look":
            A robot standing in the meadow. It seems to be looking for something or someone. #player
        - "talk_to":
            ~ start_dialog(thing_id)
        - "use":
            Erm... #player
            ~ wait(1.0)
            It’s not *that* kind of robot. #player
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