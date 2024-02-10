using Godot;
using Godot.Collections;

public partial class InventoryManager : GodotObject
{
    [Signal]
    public delegate void ObjectAddedEventHandler(string objectID, Texture2D objectTexture);

    [Signal]
    public delegate void ObjectRemovedEventHandler(string objectID);

    public static InventoryManager Instance { get; } = new();

    public Dictionary<string, Texture2D> Inventory { get; } = new();

    public void AddObject(string objectID, Texture2D objectTexture)
    {
        Inventory.Add(objectID, objectTexture);
        EmitSignal(SignalName.ObjectAdded, objectID, objectTexture);
    }

    public void RemoveObject(string objectID)
    {
        Inventory.Remove(objectID);
        EmitSignal(SignalName.ObjectRemoved, objectID);
    }

    public bool HasObject(string objectID, bool inventory = false)
    {
        return Inventory.ContainsKey(objectID);
    }
}