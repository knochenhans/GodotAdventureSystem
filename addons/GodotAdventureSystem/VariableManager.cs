using Godot;
using Godot.Collections;

public class VariableManager
{
    public static VariableManager Instance { get; } = new VariableManager();

    private Dictionary<string, Variant> Variables { get; } = new();

    public void SetVariable(string thingID, Variant value)
    {
        Variables[thingID] = value;
        Logger.Log($"Set variable {thingID} to {value} with type {value.GetType()}", Logger.LogTypeEnum.Info);
    }

    public Variant GetVariable(string thingID)
    {
        if (!Variables.ContainsKey(thingID))
        {
            Logger.Log($"Variable {thingID} not found, returning false", Logger.LogTypeEnum.Warning);
            return false;
        }
        return Variables[thingID];
    }

    public bool HasVariable(string thingID) => Variables.ContainsKey(thingID);

    public Dictionary<string, Variant> GetVariables() => Variables;
    public void SetVariables(Dictionary<string, Variant> variables)
    {
        Variables.Clear();
        foreach (var kvp in variables)
        {
            Variables[kvp.Key] = kvp.Value;
        }
    }
}