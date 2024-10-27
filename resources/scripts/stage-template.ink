INCLUDE ../includes.ink

=== function verb(thing_id, verb_id) ===
    VAR action_count = 0
    ~ action_count = get_action_count(thing_id, verb_id)
    
    { thing_id:
    - "thing":
        { verb_id:
        - "look":
            This is a test! #player
        - "talk_to":
            This cloud’s not talking back to me. What a shame! #player
        - "give":
            ~ play_anim("left_hand_up")
        - else: ~ return false
        }
    
    ~ return true