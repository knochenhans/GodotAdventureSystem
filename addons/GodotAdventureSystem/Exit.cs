using Godot;

[Tool]
[Icon("res://addons/GodotAdventureSystem/icons/Exit.svg")]
public partial class Exit : Hotspot
{
	[Export] public string Destination { get; set; } = "";
	[Export] public string Entry { get; set; } = "";

	public override void _EnterTree()
	{
        Color = new Color("#f5ef4240");
		AddToGroup("hotspot");
	}
}
