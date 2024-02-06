using System.Threading.Tasks;
using Godot;

public partial class Character : Thing
{
	[Export]
	public double Speed { get; set; } = 100f;

	[Signal]
	public delegate void CharacterMovedEventHandler();

	[Export]
	public Color SpeechColor { get; set; } = Colors.White;

	[ExportCategory("Sounds")]
	[Export]
	public AudioStream PickupSound { get; set; }

	enum MovementStateEnum
	{
		Idle,
		Moving,
		Talking,
	}

	MovementStateEnum _currentMovementState;
	MovementStateEnum CurrentMovementState
	{
		get => _currentMovementState; set
		{
			switch (value)
			{
				case MovementStateEnum.Idle:
					AnimatedSprite2D.Play("idle");
					StepSoundsNode.Stop();
					break;
				case MovementStateEnum.Moving:
					AnimatedSprite2D.Play("walk");
					StepSoundsNode.Play();
					break;
				case MovementStateEnum.Talking:
					AnimatedSprite2D.Play("talk");
					StepSoundsNode.Stop();
					break;
			}

			_currentMovementState = value;
		}
	}

	enum DirectionEnum
	{
		Left,
		Right,
	}

	DirectionEnum _currentDirection;
	DirectionEnum CurrentDirection
	{
		get => _currentDirection; set
		{
			if (value == DirectionEnum.Left)
				AnimatedSprite2D.FlipH = false;
			else if (value == DirectionEnum.Right)
				AnimatedSprite2D.FlipH = true;

			_currentDirection = value;
		}
	}

	NavigationAgent2D NavigationAgent2D { get; set; }
	AnimatedSprite2D AnimatedSprite2D { get; set; }
	AudioStreamPlayer2D SoundsNode { get; set; }
	AudioStreamPlayer2D StepSoundsNode { get; set; }

	public override void _Ready()
	{
		base._Ready();

		NavigationAgent2D = GetNode<NavigationAgent2D>("NavigationAgent2D");
		AnimatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		SoundsNode = GetNode<AudioStreamPlayer2D>("Sounds");
		StepSoundsNode = GetNode<AudioStreamPlayer2D>("StepSounds");

		CurrentDirection = DirectionEnum.Right;
		CurrentMovementState = MovementStateEnum.Idle;
	}

	public async Task MoveTo(Vector2 position, float desiredDistance = 2f, bool isRelative = false)
	{
		if (CurrentMovementState != MovementStateEnum.Talking)
		{
			if (isRelative)
				NavigationAgent2D.TargetPosition = Position + position;
			else
			{
				NavigationAgent2D.TargetPosition = position;
				// NavigationAgent2D.TargetDesiredDistance = 500f;
			}
			CurrentMovementState = MovementStateEnum.Moving;

			await ToSignal(this, "CharacterMoved");
		}
	}

	public async Task Talk(string message)
	{
		CurrentMovementState = MovementStateEnum.Talking;

		var speechBubble = ResourceLoader.Load<PackedScene>("res://AdventureSystem/SpeechBubble.tscn").Instantiate() as SpeechBubble;
		AddChild(speechBubble);
		speechBubble.Init(message, SpeechColor, new Vector2(0, GetSize().Y));
		// var height = speechBubble.GetNode<RichTextLabel>("Text").GetContentHeight() / 4;
		// speechBubble.Position -= new Vector2(speechBubble.Size.X / 2, GetSize().Y + height);
		// speechBubble.Finished += _OnSpeechBubbleFinished;
		// AnimatedSprite2D.Play("talk");
		await ToSignal(speechBubble, "Finished");
		SetIdle();

		// CurrentMovementState = MovementStateEnum.Idle;
		// return Task.CompletedTask;
	}

	public async Task PlayAnimation(string animationName)
	{
		AnimatedSprite2D.Play(animationName);
		await ToSignal(AnimatedSprite2D, "animation_finished");
		AnimatedSprite2D.Play("idle");
	}

	public void PickUpObject()
	{
		SoundsNode.Stream = PickupSound;
		SoundsNode.Play();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (CurrentMovementState != MovementStateEnum.Talking)
		{
			if (NavigationAgent2D != null)
			{
				if (!NavigationAgent2D.IsNavigationFinished())
				{
					// GD.Print(NavigationAgent2D.TargetDesiredDistance);
					var movementDelta = Speed * delta;
					var nextPathPosition = NavigationAgent2D.GetNextPathPosition();
					var newVelocity = Position.DirectionTo(nextPathPosition) * (float)movementDelta;
					GlobalPosition = GlobalPosition.MoveToward(nextPathPosition + newVelocity, (float)movementDelta);

					if (newVelocity.X < 0)
						CurrentDirection = DirectionEnum.Left;
					else if (newVelocity.X > 0)
						CurrentDirection = DirectionEnum.Right;

					// GD.Print($"Distance to target: {NavigationAgent2D.DistanceToTarget()}");
					// GD.Print($"Distance to next path position: {Position.DistanceTo(nextPathPosition)}");
					// GD.Print($"IsTargetReached: {NavigationAgent2D.IsTargetReached()}");
				}
			}
		}
	}

	public void _OnNavigationFinished()
	{
		CurrentMovementState = MovementStateEnum.Idle;
		EmitSignal(SignalName.CharacterMoved);
	}

	public Vector2 GetSize()
	{
		return AnimatedSprite2D.SpriteFrames.GetFrameTexture(AnimatedSprite2D.Animation, AnimatedSprite2D.Frame).GetSize();
	}

	// public void _OnSpeechBubbleFinished()
	// {
	// 	CurrentMovementState = MovementStateEnum.Idle;
	// }

	public void SetIdle()
	{
		CurrentMovementState = MovementStateEnum.Idle;
	}
}
