using System;
using System.Threading.Tasks;
using Godot;
using static Logger;

public partial class AdventureEntity : Entity
{
    PlayerInputControllerNavigation InputController => GetNode<PlayerInputControllerNavigation>("PlayerInputControllerNavigation");

    public async Task SpeechBubble(string message)
    {
        GD.Print($"Entity {Name} says: {message}");
        await Task.Delay(1000); // Simulate delay
    }

    public async Task MoveTo(Vector2 position, int v, bool isRelative = false)
    {
        if (isRelative)
            position += GlobalPosition;

        InputController.SetMovementTarget(position);

        await ToSignal(Moveable as MoveableNavigation, MoveableNavigation.SignalName.TargetPositionReached);

        Log($"Entity {Name} moved to {position}", LogTypeEnum.Script);
    }

    public async Task PlayAnimation(string animationName)
    {
        GD.Print($"Entity {Name} plays animation: {animationName}");
        await Task.Delay(1000); // Simulate delay
    }
}
