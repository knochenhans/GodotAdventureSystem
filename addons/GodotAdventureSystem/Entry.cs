using Godot;

[Tool]
[Icon("res://addons/GodotAdventureSystem/icons/Entry.svg")]
public partial class Entry : Marker2D
{
	[Export] public string ID { get; set; }
	[Export] public Character.OrientationEnum Orientation { get; set; } = Character.OrientationEnum.Down;

	public override void _EnterTree()
	{
		AddToGroup("entry");
	}
}
