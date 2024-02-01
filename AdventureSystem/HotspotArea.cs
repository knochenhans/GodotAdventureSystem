using Godot;
using System;

public partial class HotspotArea : Area2D
{
	public string ID { get; set; }
	public string DisplayedName { get; set; }
	public Godot.Collections.Dictionary<string, string> Actions { get; set; }
}
