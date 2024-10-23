using Godot;
using Godot.Collections;

public partial class GameResource : Resource
{
	[Export] public Dictionary<string, string> Verbs = new();
	[Export] public Dictionary<string, string> DefaultVerbReactions = new();
}