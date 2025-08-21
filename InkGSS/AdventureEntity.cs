using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using static Logger;

public partial class AdventureEntity : Entity
{
    public Dictionary<string, string> DefaultVerbReactions { get; set; } = [];

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
        var animatedSpriteManager = SpriteManager as AnimatedSprite2DManager;

        Moveable?.StopMovement();
        animatedSpriteManager.PlayAnimation(animationName);
        await ToSignal(animatedSpriteManager.AnimatedSprite, AnimatedSprite2D.SignalName.AnimationFinished);

        CurrentDirection = new Vector2(0, 1);
        UpdateSpriteOrientation();
        animatedSpriteManager.PlayAnimation();
    }
}
