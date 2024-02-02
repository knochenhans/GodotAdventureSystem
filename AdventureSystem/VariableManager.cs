using Godot;
using Godot.Collections;

public class VariableManager
{
    public static VariableManager Instance { get; } = new VariableManager();

    public Dictionary<string, Variant> Variables { get; } = new();

    public void SetVariable(string thingID, Variant value)
    {
        Variables[thingID] = value;
    }

    public object GetVariable(string thingID)
    {
        return Variables[thingID];
    }

    public bool HasVariable(string thingID)
    {
        return Variables.ContainsKey(thingID);
    }

    public void SetVerbState(string thingID, string verb, bool state)
    {
        if (!Variables.ContainsKey(thingID))
        {
            Variables[thingID] = new Dictionary<string, bool>();
        }
    }
}