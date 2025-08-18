using Godot;

public partial class CustomStageManager : StageManager
{
    public Sprite2D GetCurrentStageBackground() => CurrentStageScene.GetNode<Sprite2D>("Background");
}
