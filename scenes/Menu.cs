using Godot;
using System;

public partial class Menu : Scene
{
    public void _OnStartButtonPressed() => SceneManagerNode.ChangeToScene("Game");
    public void _OnOptionsButtonPressed() => SceneManagerNode.ChangeToScene("Options");
    public void _OnExitButtonPressed() => SceneManagerNode.ChangeToScene("Credits");
}
