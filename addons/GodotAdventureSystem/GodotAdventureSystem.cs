#if TOOLS
using Godot;
using System;

[Tool]
public partial class GodotAdventureSystem : EditorPlugin
{
	public override void _EnterTree()
	{
		var hotspotScript = GD.Load<Script>("res://addons/GodotAdventureSystem/Hotspot.cs");
		AddCustomType("Hotspot", "Polygon2D", hotspotScript, null);

		var entryScript = GD.Load<Script>("res://addons/GodotAdventureSystem/Entry.cs");
		AddCustomType("Entry", "Marker2D", entryScript, null);
	}

	public override void _ExitTree()
	{
		RemoveCustomType("Entry");
		RemoveCustomType("Hotspot");
	}
}
#endif
