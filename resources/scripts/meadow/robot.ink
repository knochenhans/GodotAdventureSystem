=== robot ===

VAR c = "robot"

{ visited(c) == 0:
    Hi there! #player
    
    Hello, fleshperson. #robot
    
    Weird meeting a robot out here in nature. #player
    
    You think so? #robot
    ~ play_anim_c(c, "front")
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
    ~ play_anim_c(c, "idle")

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
    ~ play_anim_c(c, "shrug")
    Nothing, it’s completely worthless. #robot

* [Why give it a silly name like that?]
    You know the A.R.F., their brand of humor was always a bit…  #robot
    Dumb. #robot

    No wonder it failed. #player

    You could say that. #robot
* -> main
- -> buttcoin