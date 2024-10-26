using Godot.Collections;
using Godot;
using System;

public partial class StageThingManager : GodotObject
{
    public Dictionary<string, Thing> Things { get; private set; } = new();

    public void RegisterThing(string thingID, Thing thing)
    {
        Things[thingID] = thing;
        Logger.Log($"StageThingManager: Thing {thingID} with name \"{(thing.Resource as ThingResource).DisplayedName}\" registered", Logger.LogTypeEnum.World);
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
            RegisterThing((thing.Resource as ThingResource).ID, thing);
    }

    public Character GetCharacter(string characterID)
    {
        var character = GetThing(characterID);

        if (character is Character)
            return character as Character;
        else
        {
            Logger.Log($"StageThingManager: Thing {characterID} is not a Character", Logger.LogTypeEnum.Error);
            return null;
        }
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

    public ThingResource RemoveThing(string thingID)
    {
        ThingResource thingResource = null;
        if (Things.ContainsKey(thingID))
        {
            var thing = Things[thingID];
            thingResource = thing.Resource as ThingResource;
            Things.Remove(thingID);
            thing.QueueFree();
            Logger.Log($"StageThingManager: Thing {thingID} removed", Logger.LogTypeEnum.World);
        }
        else
            Logger.Log($"StageThingManager: Thing {thingID} not found", Logger.LogTypeEnum.Error);
        return thingResource;
    }

    public void UpdateThingName(string thingID, string name)
    {
        if (Things.ContainsKey(thingID))
        {
            (Things[thingID].Resource as ThingResource).DisplayedName = name;
            Logger.Log($"StageThingManager: Thing {thingID} name updated to {name}", Logger.LogTypeEnum.World);
        }
        else
            Logger.Log($"StageThingManager: Thing {thingID} not found", Logger.LogTypeEnum.Error);
    }

    public string GetThingName(string thingID)
    {
        if (Things.ContainsKey(thingID))
            return (Things[thingID].Resource as ThingResource).DisplayedName;
        else
        {
            Logger.Log($"StageThingManager: Thing {thingID} not found", Logger.LogTypeEnum.Error);
            return "";
        }
    }

    public Array<Thing> GetThings()
    {
        Array<Thing> things = new();

        foreach (var thing in Things)
        {
            if (!IsInInventory(thing.Key))
                things.Add(thing.Value);
        }

        return things;
    }

    public void Clear()
    {
        // foreach (var thing in Things)
        // thing.Value.QueueFree();
        Things.Clear();
        Logger.Log("StageThingManager: Cleared", Logger.LogTypeEnum.World);
    }
}