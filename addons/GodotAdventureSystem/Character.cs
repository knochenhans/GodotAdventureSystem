using System.Threading.Tasks;
using Godot;

[Icon("res://addons/GodotAdventureSystem/icons/Character.svg")]
public partial class Character : Thing
{
	[Signal]
	public delegate void CharacterMovedEventHandler();

	[Export]
	public double Speed { get; set; } = 100f;

	[Export]
	public Color SpeechColor { get; set; } = Colors.White;

	[ExportCategory("Sounds")]
	[Export]
	public AudioStream PickupSound { get; set; }

	enum MovementStateEnum
	{
		Idle,
		Moving,
		SpeechBubble,
		Dialog
	}

	MovementStateEnum _currentMovementState;
	MovementStateEnum CurrentMovementState
	{
		get => _currentMovementState; set
		{
			switch (value)
			{
				case MovementStateEnum.Idle:
					if (CurrentDirection == DirectionEnum.Front)
						AnimatedSprite2D.Play("idle");
					else
						AnimatedSprite2D.Play("idle_side");
					StepSoundsNode.Stop();
					break;
				case MovementStateEnum.Moving:
					AnimatedSprite2D.Play("walk");
					StepSoundsNode.Play();
					break;
				case MovementStateEnum.SpeechBubble:
					if (CurrentDirection == DirectionEnum.Front)
						AnimatedSprite2D.Play("talk");
					else
						AnimatedSprite2D.Play("talk_side");
					StepSoundsNode.Stop();
					break;
				case MovementStateEnum.Dialog:
					AnimatedSprite2D.Play("idle_side");
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
		Front
	}

	DirectionEnum _currentDirection;
	DirectionEnum CurrentDirection
	{
		get => _currentDirection; set
		{
			switch (value)
			{
				case DirectionEnum.Left:
					AnimatedSprite2D.FlipH = false;
					break;
				case DirectionEnum.Right:
					AnimatedSprite2D.FlipH = true;
					break;
				case DirectionEnum.Front:
					AnimatedSprite2D.FlipH = false;
					AnimatedSprite2D.Play("idle");
					break;
			}

			_currentDirection = value;
		}
	}

	NavigationAgent2D NavigationAgent2D { get; set; }
	AnimatedSprite2D AnimatedSprite2D { get; set; }
	AudioStreamPlayer2D SoundsNode { get; set; }
	AudioStreamPlayer2D StepSoundsNode { get; set; }

	private int _scriptVisits = 0; // How many times this character's story script has been visited by the player
	public int ScriptVisits { get; set; }

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

	public async Task MoveTo(Vector2 position, int desiredDistance = 10, bool isRelative = false)
	{
		if (CurrentMovementState != MovementStateEnum.SpeechBubble)
		{
			if (isRelative)
				NavigationAgent2D.TargetPosition = Position + position;
			else
			{
				NavigationAgent2D.TargetPosition = position;
				// NavigationAgent2D.TargetDesiredDistance = 500f;
			}
			NavigationAgent2D.PathDesiredDistance = desiredDistance;
			CurrentMovementState = MovementStateEnum.Moving;

			await ToSignal(this, "CharacterMoved");
		}
	}

	public async Task SpeechBubble(string message)
	{
		CurrentMovementState = MovementStateEnum.SpeechBubble;

		var speechBubble = ResourceLoader.Load<PackedScene>("res://addons/GodotAdventureSystem/SpeechBubble.tscn").Instantiate() as SpeechBubble;
		AddChild(speechBubble);
		speechBubble.Init(message, SpeechColor, new Vector2(0, GetSize().Y));

		await ToSignal(speechBubble, "Finished");
		SetIdle();
	}

	public async Task PlayAnimation(string animationName, bool loop = false)
	{
		AnimatedSprite2D.Play(animationName);
		AnimatedSprite2D.SpriteFrames.SetAnimationLoop(animationName, loop);
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
		if (CurrentMovementState != MovementStateEnum.SpeechBubble)
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

	public void SetIdle()
	{
		CurrentMovementState = MovementStateEnum.Idle;
	}

	public void StartDialog()
	{
		CurrentMovementState = MovementStateEnum.Dialog;
	}

	public void EndDialog()
	{
		CurrentMovementState = MovementStateEnum.Idle;
	}

	public void LookTo(Vector2 position)
	{
		if (position == Position)
			CurrentDirection = DirectionEnum.Front;
		else
		{
			if (position.X < Position.X)
				CurrentDirection = DirectionEnum.Left;
			else if (position.X > Position.X)
				CurrentDirection = DirectionEnum.Right;
		}
	}
}
