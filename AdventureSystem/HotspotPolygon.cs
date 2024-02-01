using Godot;
using System;

public partial class HotspotPolygon : Polygon2D
{
	[Export]
	public string ID { get; set; }

	[Export]
	public string DisplayedName { get; set; }

	[Export]
	public Godot.Collections.Dictionary<string, string> Actions { get; set; } = new Godot.Collections.Dictionary<string, string>
	{
		["give"] = "",
		["pick_up"] = "",
		["use"] = "",
		["open"] = "",
		["look"] = "",
		["push"] = "",
		["close"] = "",
		["talk_to"] = "",
		["pull"] = "",
	};
}
