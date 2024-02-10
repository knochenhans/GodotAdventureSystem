#if TOOLS
using Godot;
using System;

[Tool]
public partial class GodotAdventureSystem : EditorPlugin
{
	public override void _EnterTree()
	{
		var hotspot = GD.Load<Script>("res://addons/GodotAdventureSystem/Hotspot.cs");
		AddCustomType("Hotspot", "Polygon2D", hotspot, null);
	}

	public override void _ExitTree()
	{
		RemoveCustomType("Hotspot");
	}
}
#endif
