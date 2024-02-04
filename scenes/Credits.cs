using Godot;
using System;

public partial class Credits : Scene
{
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.Pressed)
			SceneManagerNode.Quit();
    }
}