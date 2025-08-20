=== function interact_inventory(thing_id, verb_id) ===
    ~ action_count = get_action_count(thing_id, verb_id)

    { thing_id:
    - "note":
        { verb_id:
        - "look":
            It’s a paper note. It reads: “Why are you reading this?” #player
            { action_count == 0:
                ~ set_name("note", "Paper note with a pointless question")
            }
        - else: ~ return false
        }
    }

    ~ return true