using Godot;

[Tool]
[Icon("res://addons/GodotAdventureSystem/icons/Entry.svg")]
public partial class Entry : Marker2D
{
	[Export]
	public string ID { get; set; }

	public override void _EnterTree()
	{
		AddToGroup("entry");
	}
}
