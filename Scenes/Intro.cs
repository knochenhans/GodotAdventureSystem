using Godot;

public partial class Intro : Scene
{
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.Pressed)
            SceneManagerNode.ChangeToScene("Menu");
    }
}
