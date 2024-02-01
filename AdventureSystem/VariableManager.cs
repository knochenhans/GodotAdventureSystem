using System.Collections.Generic;

public class VariableManager
{
    public static VariableManager Instance { get; } = new VariableManager();

    public Dictionary<string, object> Variables { get; } = new();

    public void SetVariable(string name, object value)
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