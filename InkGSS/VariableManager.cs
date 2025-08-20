using Godot;
using Godot.Collections;

public class VariableManager
{
    public static VariableManager Instance { get; } = new VariableManager();

    private Dictionary<string, Variant> Variables { get; } = [];

    public void SetVariable(string id, Variant value)
    {
        Variables[id] = value;
        Logger.Log($"Setting variable {id} to {value} with type {value.GetType()}", "InkGSS", Logger.LogTypeEnum.Script);
    }

    public Variant GetVariable(string id)
    {
        if (!Variables.TryGetValue(id, out Variant value))
        {
            Logger.LogWarning($"Variable {id} not found, returning false", "InkGSS", Logger.LogTypeEnum.Script);
            return false;
        }
        return value;
    }

    public bool HasVariable(string id) => Variables.ContainsKey(id);

    public Dictionary<string, Variant> GetVariables() => Variables;
    public void SetVariables(Dictionary<string, Variant> variables)
    {
        Variables.Clear();
        foreach (var kvp in variables)
            Variables[kvp.Key] = kvp.Value;
    }
}