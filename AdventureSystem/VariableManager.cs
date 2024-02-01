using Godot.Collections;

public class VariableManager
{
    public static VariableManager Instance { get; } = new VariableManager();

    public Dictionary<string, Object> Variables { get; } = new();

    public void SetVariable(string name, Object value)
    {
        Variables[name] = value;
    }

    public object GetVariable(string name)
    {
        return Variables[name];
    }

    public bool HasVariable(string name)
    {
        return Variables.ContainsKey(name);
    }
}