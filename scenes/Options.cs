using Godot;
using System;

public partial class Options : Scene
{
	public void _OnButtonPressed() => SceneManagerNode.ChangeToScene("Menu");
}
