using Godot;

public partial class Thing : Area2D
{
    [Export]
    public string ID { get; set; }
    
    [Export]
    public string DisplayedName { get; set; }
}