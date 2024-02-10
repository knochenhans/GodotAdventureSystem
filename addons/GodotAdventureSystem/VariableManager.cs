using Godot;
using Godot.Collections;

public class VariableManager
{
    public static VariableManager Instance { get; } = new VariableManager();

    public Dictionary<string, bool> Variables { get; } = new();

    public void SetVariable(string thingID, bool value)
    {
        Variables[thingID] = value;
        GD.Print($"Set variable {thingID} to {value} with type {value.GetType()}");
    }

    public bool GetVariable(string thingID)
    {
        if (!Variables.ContainsKey(thingID))
        {
            GD.Print($"Variable {thingID} not found, returning false");
            return false;
        }
        return Variables[thingID];
    }

    public bool HasVariable(string thingID)
    {
        return Variables.ContainsKey(thingID);
    }
}