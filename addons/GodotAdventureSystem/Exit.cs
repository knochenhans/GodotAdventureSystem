using Godot;

[Tool]
[Icon("res://addons/GodotAdventureSystem/icons/Exit.svg")]
public partial class Exit : Polygon2D
{
	[Export] public string ID { get; set; }

	public override void _EnterTree()
	{
        Color = new Color("#f5ef4240");
		AddToGroup("exit");
	}
}
