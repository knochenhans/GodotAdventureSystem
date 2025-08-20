=== robot ===

VAR c = "robot"

{ visited(c) == 0:
    Hallo! #player
    
    Hallo, Fleischperson. #robot
    
    Seltsam, hier draußen in der Natur einen Roboter zu treffen. #player
    
    Findest du? #robot
    ~ play_anim_c(c, "idle_down")
    ~ wait(0.3)
    Nun, mir scheint das überhaupt nicht seltsam, weißt du? #robot
    Also, was willst du, Fleischperson? #robot

- else:
    Hallo nochmal, Fleischperson. #robot
}
-> main

= main

* [Schöne Wiese, nicht wahr?]
    Was kommt als nächstes, fragst du mich nach dem Wetter? #robot

    * * [Also, wie ist das Wetter?]
        Weißt du, ich habe gehört, dass die letzte Person, die Bekanntschaft mit meinem 1000-Volt-Elektrowerkzeug gemacht hat, immer noch von dem wunderbaren Summen träumt, das es macht. #robot
    
* [Wie heißt du?]
    Mein Name ist... äh, ehrlich gesagt, keine Ahnung. Mich hat noch nie jemand danach gefragt. #robot
    
    Warte, du kennst deinen eigenen Namen nicht? #player
    
    Brauche ich einen? #robot

* [Was macht ein Roboter wie du hier draußen?]
    ~ play_anim_c(c, "idle_down")

    Ich suche jemanden. #robot
    Und was machst *du* hier draußen? #robot

    * * [Einfach die Natur genießen.]
        Typisch Fleischperson. #robot

* { is_in_inventory("coin") } [Hast du jemals eine Münze wie diese gesehen?]
    Lass mich das sehen. #robot
    ~wait(1)
    Das ist eine Buttcoin. #robot

    Eine was? #player

    Eine Buttcoin. #robot

    -> buttcoin

+ [Nun, ich sollte besser gehen.] -> outro
- -> main

= outro
Pass auf dich auf, Fleischperson. #robot -> END
        
-> END

= buttcoin

* [Und was genau ist eine Buttcoin?]
    Nun... #robot
    ~ wait(0.5)
    Es war einer der gescheiterten Versuche der A.R.F., greifbare Währung zurückzubringen. #robot
    
    Was ist die A.R.F.? #player

    Warst du die letzten 10 Jahre im Weltraum oder so? #robot
    Die Analoge Rebellion Front. #robot

* [Was kann ich damit kaufen?]
    ~ play_anim_c(c, "shrug")
    Nichts, sie ist völlig wertlos. #robot

* [Warum ihr einen so albernen Namen geben?]
    Du kennst die A.R.F., ihr Humor war immer ein bisschen...  #robot
    Dumm. #robot

    Kein Wunder, dass es gescheitert ist. #player

    Das kann man wohl sagen. #robot
* -> main
- -> buttcoin