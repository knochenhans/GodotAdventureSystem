using Godot;

public partial class ExitResource : ThingResource
{
    [Export] public string Destination { get; set; }
    [Export] public string Entry { get; set; }
}