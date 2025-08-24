using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using static Logger;

public partial class AdventureEntity : Entity
{
    PlayerInputControllerNavigation InputController => GetNode<PlayerInputControllerNavigation>("PlayerInputControllerNavigation");

    enum TalkingStateEnum
    {
        Idle,
        Talk
    }

    TalkingStateEnum _currentTalkingState = TalkingStateEnum.Idle;
    TalkingStateEnum CurrentTalkingState
    {
        get => _currentTalkingState;
        set
        {
            if (_currentTalkingState == value)
                return;

            PlayAnimation(value.ToString().ToLower());
            _currentTalkingState = value;
        }
    }

    public async Task SpeechBubble(string message)
    {
        if (StageNodeResource is AdventureEntityResource adventureEntityResource)
        {
            if (message == string.Empty)
            {
                LogWarning("Speech bubble message is empty.", "SpeechBubble", LogTypeEnum.Script);
                return;
            }

            CurrentTalkingState = TalkingStateEnum.Talk;
            var speechBubble = adventureEntityResource.SpeechBubbleScene.Instantiate() as SpeechBubble;
            AddChild(speechBubble);
            speechBubble.Init(message, adventureEntityResource.SpeechBubbleColor, new Vector2(0, GetGlobalRect().Size.Y));

            await ToSignal(speechBubble, global::SpeechBubble.SignalName.Finished);
            CurrentTalkingState = TalkingStateEnum.Idle;
        }
    }

    public async Task MoveTo(Vector2 position, int desiredDistance = 2, bool isRelative = false)
    {
        if (CurrentTalkingState == TalkingStateEnum.Talk)
            return;

        if (isRelative)
            position += GlobalPosition;

        InputController.SetMovementTarget(position);

        await ToSignal(Moveable as MoveableNavigation, MoveableNavigation.SignalName.TargetPositionReached);

        Log($"Entity {Name} moved to {position}", LogTypeEnum.Script);
    }

    public void PlayAnimation(string animationName)
    {
        var animatedSpriteManager = SpriteManager as AnimatedSprite2DManager;

        Moveable?.StopMovement();
        animatedSpriteManager.PlayAnimation(animationName);
    }

    public async Task PlayAnimationAndWait(string animationName)
    {
        var animatedSpriteManager = SpriteManager as AnimatedSprite2DManager;

        Moveable?.StopMovement();
        await animatedSpriteManager.PlayAnimationAndWait(animationName);

        CurrentDirection = new Vector2(0, 1);
        UpdateSpriteOrientation();
        animatedSpriteManager.PlayAnimation();
    }
}
