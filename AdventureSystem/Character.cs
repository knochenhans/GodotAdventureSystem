using Godot;
using Godot.Collections;

public partial class Character : Node2D
{
	[Export]
	public double Speed { get; set; } = 100f;

	[Signal]
	public delegate void CharacterMovedEventHandler();

	[ExportCategory("Character")]
	[Export]
	public string CharacterName { get; set; }

	[Export]
	public Color SpeechColor { get; set; } = Colors.White;

	[ExportCategory("Sounds")]
	[Export]
	public AudioStream PickupSound { get; set; }

	public InventoryManager InventoryManager { get; set; }

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
		NavigationAgent2D = GetNode<NavigationAgent2D>("NavigationAgent2D");
		AnimatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		SoundsNode = GetNode<AudioStreamPlayer2D>("Sounds");
		StepSoundsNode = GetNode<AudioStreamPlayer2D>("StepSounds");

		CurrentDirection = DirectionEnum.Right;
	}

	public void MoveTo(Vector2 position)
	{
		if (CurrentMovementState != MovementStateEnum.Talking)
		{
			NavigationAgent2D.TargetPosition = position;
			CurrentMovementState = MovementStateEnum.Moving;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (CurrentMovementState != MovementStateEnum.Talking)
		{
			if (NavigationAgent2D.IsNavigationFinished())
			{
				CurrentMovementState = MovementStateEnum.Idle;
				EmitSignal(SignalName.CharacterMoved);
			}
			else
			{
				var movementDelta = Speed * delta;
				var nextPathPosition = NavigationAgent2D.GetNextPathPosition();
				var newVelocity = Position.DirectionTo(nextPathPosition) * (float)movementDelta;
				Position = Position.MoveToward(nextPathPosition + newVelocity, (float)movementDelta);

				if (newVelocity.X < 0)
					CurrentDirection = DirectionEnum.Left;
				else if (newVelocity.X > 0)
					CurrentDirection = DirectionEnum.Right;
				// GD.Print($"New position: {Position}, nextPathPosition: {nextPathPosition}, newVelocity: {newVelocity}");
			}
		}
	}

	public void Talk(Array<string> messageLines)
	{
		CurrentMovementState = MovementStateEnum.Talking;

		foreach (string messageLine in messageLines)
		{
			var speechBubble = ResourceLoader.Load<PackedScene>("res://AdventureSystem/SpeechBubble.tscn").Instantiate() as SpeechBubble;
			AddChild(speechBubble);
			speechBubble.Init(messageLines[0], SpeechColor);
			speechBubble.Position -= new Vector2(speechBubble.Size.X / 2, GetSize().Y + 25);
			speechBubble.Finished += _OnSpeechBubbleFinished;
			// AnimatedSprite2D.Play("talk");
			// await ToSignal(speechBubble, "SpeechBubbleFinished");
		}

		// CurrentMovementState = MovementStateEnum.Idle;
	}

	public void PickUp(Object object_)
	{
		// AnimatedSprite2D.Play("pick_up");
		InventoryManager.AddObject(object_.ID, object_.GetTexture());
		SoundsNode.Stream = PickupSound;
		SoundsNode.Play();
		object_.QueueFree();
	}

	public Vector2 GetSize()
	{
		return AnimatedSprite2D.SpriteFrames.GetFrameTexture(AnimatedSprite2D.Animation, AnimatedSprite2D.Frame).GetSize();
	}

	public void _OnSpeechBubbleFinished()
	{
		CurrentMovementState = MovementStateEnum.Idle;
	}
}
