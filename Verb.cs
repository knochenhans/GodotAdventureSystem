using Godot;

public partial class Verb : GodotObject
{
    public string ID { get; set; }
    public string Name { get; set; }

    [Signal]
    public delegate void VerbActivatedEventHandler(Verb verb);

    public Verb(string id, string name)
    {
        ID = id;
        Name = name;
    }

    public void _OnButtonPressed()
    {
        EmitSignal(SignalName.VerbActivated, this);
    }
}