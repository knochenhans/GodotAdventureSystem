INCLUDE ../includes.ink
INCLUDE ../items.ink


=== function interact_stage(thing_id, verb_id) ===
    ~ action_count = get_action_count(thing_id, verb_id)
    
    { thing_id:
    - "door":
        { verb_id:
        - "open":
            It’s locked. #player
            ~ set_name("door", "Locked door")
        - else: ~ return false
        }
    - else: ~ return false
    }
    ~ return true