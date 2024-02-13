INCLUDE includes.ink

=== function verb(thing_id, verb_id) ===
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
                - get_var("bush_looked_once") == false:
                    ~ talk("Well, that’s a bush. Looks unsuspecting, right?")
                    ~ set_var("bush_looked_once", true)
                - else:
                    {
                        - get_var("bush_looked_twice") == false:
                            ~ talk("Wait a second, someone left a coin in there!")
                            ~ create("coin")
                            ~ set_var("bush_looked_twice", true)
                            ~ set_name(thing_id, "Generous bush")
                        - else:
                            ~ talk("I already found the coin in the bush.")
                    }
            }
        - "pick_up":
            ~ set_var("bush_looked_once", true)
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
            ~ talk("A robot standing in meadow. It seems to be looking for something or someone.")
        - "talk_to":
            ~ start_dialog(thing_id)
        - else: ~ return false
        }

    - "exit_left":
        
        { verb_id:
        - "walk":
            ~ talk("The path leads to the left.")
        - else: ~ return false
        }

    - else:
        ~ print_error("Unknown thing.")
        ~ return false
    }
    
    ~ return true

=== robot ===

{ visited("robot") == 0:
    Hi there! #player
    
    Hello, fleshperson. #robot
    
    Weird meeting a robot out here in nature. #player
    
    You think so? #robot
    ~ play_anim_char("robot", "front")
    ~ wait(0.3)
    Well doesn’t seem weird to me at all, you know? #robot
    So what do you want, fleshperson? #robot

- else:
    Hello again, fleshperson. #robot
}
-> main

= main

* [Beautiful meadow, isn’t it?]
    What’s next, are you going to ask me about the weather? #robot

    * * [So, how’s the weather?]
        You know, I heard the last person that made aquaintance with my 1000 volt electro tool is still having dreams about the wonderful buzzing sound it makes. #robot
    
* [What’s your name?]
    My name is… err, I have no idea to be honest. No one ever asked me that before. #robot
    
    Wait, you don’t know your own name? #player
    
    Do I need one? #robot

* [What’s a robot like you doing out here?]
    ~ play_anim_char("robot", "idle")

    I’m looking for someone. #robot
    And what are *you* doing out here? #robot

    * * [Just enjoying the nature.]
        Just like a fleshperson would. #robot

* { is_in_inventory("coin") } [Have you ever seen a coin like this?]
    Let me see that. #robot
    ~wait(1)
    That’s a buttcoin. #robot

    A what? #player

    A buttcoin. #robot

    -> buttcoin

+ [Well, I better get going.] -> outro
- -> main

= outro
Take care, fleshperson. #robot -> END
        
-> END

= buttcoin

* [And a buttcoin is what exactly?]
    Well… #robot
    ~ wait(0.5)
    It was one of the failed attempts of the A.R.F. to bring back tangible currency. #robot
    
    What’s the A.R.F.? #player

    Have you been out in space for the past 10 years or something? #robot
    The Analogue Rebellion Front. #robot

* [What can I buy with it?]
    ~ play_anim_char("robot", "shrug")
    Nothing, it’s completely worthless. #robot

* [Why give it a silly name like that?]
    You know the A.R.F., their brand of humor was always a bit…  #robot
    Dumb. #robot

    No wonder it failed. #player

    You could say that. #robot
* -> main
- -> buttcoin