using Godot;

public partial class ThingResource : Resource
{
    [Export] public string ID { get; set; }
    [Export] public string DisplayedName { get; set; }
}