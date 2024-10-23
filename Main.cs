using Godot;
using Godot.Collections;

public partial class Main : Node
{
	public SceneManagerNode SceneManagerNode => GetNode<SceneManagerNode>("/root/SceneManagerNode");

	public override void _Ready()
	{
		var scenes = new Array<string> { "Intro", "Menu", "Game", "Options", "Credits" };

		SceneManagerNode.Init(scenes, "Game");
	}
}
