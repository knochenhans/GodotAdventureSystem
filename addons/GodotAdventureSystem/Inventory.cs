using Godot;
using Godot.Collections;

public partial class Inventory : GodotObject
{
    [Signal] public delegate void AddedThingEventHandler(ThingResource thingResource);
    [Signal] public delegate void RemovedThingEventHandler(string thingID);

    private Array<ThingResource> ThingResources { get; set; } = new();

    public void AddThing(ThingResource thingResource)
    {
        ThingResources.Add(thingResource);
        EmitSignal(SignalName.AddedThing, thingResource);
    }

    public ThingResource FindThing(string thingID)
    {
        foreach (var thingResource in ThingResources)
            if (thingResource.ID == thingID)
                return thingResource;
        return null;
    }

    public Array<ThingResource> GetThings() => ThingResources;

    public void AddThings(Array<ThingResource> thingResources)
    {
        foreach (var thingResource in thingResources)
            AddThing(thingResource);
    }

    public void RemoveThing(ThingResource thingResource)
    {
        ThingResources.Remove(thingResource);
        EmitSignal(SignalName.RemovedThing, thingResource.ID);
    }

    public void Clear()
    {
        foreach (var thingResource in ThingResources)
            RemoveThing(thingResource);
    }
}