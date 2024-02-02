using Godot.Collections;
using Godot;

public class ThingManager
{
    public Dictionary<string, Thing> Things { get; private set; } = new();

    public void AddThing(string thingID, Thing thing)
    {
        Things[thingID] = thing;
    }

    public Thing GetThing(string thingID)
    {
        if (Things.ContainsKey(thingID))
            return Things[thingID];
        else
            return null;
    }

    public void AddThings(Array<Thing> things)
    {
        foreach (var thing in things)
            AddThing(thing.ID, thing);
    }

    public void RemoveThing(string thingID)
    {
        if (Things.ContainsKey(thingID))
            Things.Remove(thingID);
        else
            GD.PrintErr($"ThingManager: Thing {thingID} not found");
    }
}