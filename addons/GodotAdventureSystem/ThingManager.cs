using Godot.Collections;
using Godot;
using System;

public partial class ThingManager : GodotObject
{
    [Signal]
    public delegate void AddThingToIventoryEventHandler(string thingID, Texture2D thingTexture);

    public Dictionary<string, Thing> Things { get; private set; } = new();

    public void RegisterThing(string thingID, Thing thing)
    {
        Things[thingID] = thing;
        GD.Print($"ThingManager: Thing {thingID} with name {thing.ThingResource.DisplayedName} registered");

        // if (!thing.Visible)
        //     GD.Print($"ThingManager: Thing {thingID} is set as invisible");
    }

    public Thing GetThing(string thingID)
    {
        if (Things.ContainsKey(thingID))
            return Things[thingID];
        else
            return null;
    }

    public void RegisterThings(Array<Thing> things)
    {
        foreach (var thing in things)
            RegisterThing(thing.ThingResource.ID, thing);
    }

    public bool IsInInventory(string thingID)
    {
        if (Things.ContainsKey(thingID))
        {
            var thing = Things[thingID];
            if (thing is Object object_)
            {
                if (!object_.Visible)
                    return true;
            }
        }
        return false;
    }

    public void RemoveThing(string thingID)
    {
        if (Things.ContainsKey(thingID))
        {
            var thing = Things[thingID];
            Things.Remove(thingID);
            thing.QueueFree();
            GD.Print($"ThingManager: Thing {thingID} removed");
        }
        else
            GD.PrintErr($"ThingManager: Thing {thingID} not found");
    }

    public void UpdateThingName(string thingID, string name)
    {
        if (Things.ContainsKey(thingID))
        {
            Things[thingID].ThingResource.DisplayedName = name;
            GD.Print($"ThingManager: Thing {thingID} name updated to {name}");
        }
        else
            GD.PrintErr($"ThingManager: Thing {thingID} not found");
    }

    public string GetThingName(string thingID)
    {
        if (Things.ContainsKey(thingID))
            return Things[thingID].ThingResource.DisplayedName;
        else
        {
            GD.PrintErr($"ThingManager: Thing {thingID} not found");
            return "";
        }
    }

    public void MoveThingToInventory(string thingID)
    {
        if (Things.ContainsKey(thingID))
        {
            var thing = Things[thingID];
            if (thing is Object object_)
            {
                object_.Visible = false;
                EmitSignal(SignalName.AddThingToIventory, thingID, object_.GetTexture());
            }
            GD.Print($"ThingManager: Thing {thingID} moved to inventory");
        }
        else
            GD.PrintErr($"ThingManager: Thing {thingID} not found");
    }

    public void LoadThingToInventory(string thingID)
    {
        if (!Things.ContainsKey(thingID))
        {
            var thing = GD.Load<PackedScene>($"res://resources/objects/{thingID}.tscn").Instantiate() as Thing;
            RegisterThing(thingID, thing);
            MoveThingToInventory(thingID);

            if (thing == null)
                GD.PrintErr($"ThingManager: Unable to load thing {thingID} from resouces");
        }
        else
            GD.PrintErr($"ThingManager: Thing {thingID} already loaded");
    }

    public void Clear()
    {
        // foreach (var thing in Things)
            // thing.Value.QueueFree();
        Things.Clear();
        GD.Print("ThingManager: Cleared");
    }
}