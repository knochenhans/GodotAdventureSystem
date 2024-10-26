using Godot;

public partial class ThingResource : Resource
{
    [Export] public string ID { get; set; }
    [Export] public string DisplayedName { get; set; }
    [Export] public Texture2D Texture { get; set; }
}