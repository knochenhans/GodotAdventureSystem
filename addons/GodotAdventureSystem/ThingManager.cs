using Godot.Collections;
using Godot;
using System;

public partial class ThingManager : GodotObject
{
    [Signal] public delegate void AddThingToIventoryEventHandler(string thingID, Texture2D thingTexture);

    public Dictionary<string, Thing> Things { get; private set; } = new();

    public void RegisterThing(string thingID, Thing thing)
    {
        Things[thingID] = thing;
        Logger.Log($"ThingManager: Thing {thingID} with name {thing.ThingResource.DisplayedName} registered", Logger.LogTypeEnum.World);

        // if (!thing.Visible)
        //     Logger.Log($"ThingManager: Thing {thingID} is set as invisible", Logger.LogTypeEnum.World);
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
            Logger.Log($"ThingManager: Thing {thingID} removed", Logger.LogTypeEnum.World);
        }
        else
            Logger.Log($"ThingManager: Thing {thingID} not found", Logger.LogTypeEnum.Error);
    }

    public void UpdateThingName(string thingID, string name)
    {
        if (Things.ContainsKey(thingID))
        {
            Things[thingID].ThingResource.DisplayedName = name;
            Logger.Log($"ThingManager: Thing {thingID} name updated to {name}", Logger.LogTypeEnum.World);
        }
        else
            Logger.Log($"ThingManager: Thing {thingID} not found", Logger.LogTypeEnum.Error);
    }

    public string GetThingName(string thingID)
    {
        if (Things.ContainsKey(thingID))
            return Things[thingID].ThingResource.DisplayedName;
        else
        {
            Logger.Log($"ThingManager: Thing {thingID} not found", Logger.LogTypeEnum.Error);
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
            Logger.Log($"ThingManager: Thing {thingID} moved to inventory", Logger.LogTypeEnum.World);
        }
        else
            Logger.Log($"ThingManager: Thing {thingID} not found", Logger.LogTypeEnum.Error);
    }

    public void LoadThingToInventory(string thingID)
    {
        if (!Things.ContainsKey(thingID))
        {
            var thing = GD.Load<PackedScene>($"res://resources/objects/{thingID}.tscn").Instantiate() as Thing;
            RegisterThing(thingID, thing);
            MoveThingToInventory(thingID);

            if (thing == null)
                Logger.Log($"ThingManager: Unable to load thing {thingID} from resources", Logger.LogTypeEnum.Error);
        }
        else
            Logger.Log($"ThingManager: Thing {thingID} already loaded", Logger.LogTypeEnum.Error);
    }

    public void Clear()
    {
        // foreach (var thing in Things)
        // thing.Value.QueueFree();
        Things.Clear();
        Logger.Log("ThingManager: Cleared", Logger.LogTypeEnum.World);
    }
}