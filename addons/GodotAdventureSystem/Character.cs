using System.Threading.Tasks;
using Godot;

[Icon("res://addons/GodotAdventureSystem/icons/Character.svg")]
public partial class Character : Thing
{
	[Signal] public delegate void MovementFinishedEventHandler();

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
			MovementStateChanged(value);

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

	NavigationAgent2D NavigationAgent2D => GetNode<NavigationAgent2D>("NavigationAgent2D");
	AnimatedSprite2D AnimatedSprite2D => GetNode<AnimatedSprite2D>("AnimatedSprite2D");
	AudioStreamPlayer2D SoundPlayer => GetNode<AudioStreamPlayer2D>("Sounds");
	AudioStreamPlayer2D StepSoundPlayer => GetNode<AudioStreamPlayer2D>("StepSounds");

	private int _scriptVisits = 0; // How many times this character's story script has been visited by the player
	public int ScriptVisits { get; set; }

	public override void _Ready()
	{
		base._Ready();

		CurrentDirection = DirectionEnum.Right;
		CurrentMovementState = MovementStateEnum.Idle;

		AnimatedSprite2D.SpriteFrames = (Resource as CharacterResource).SpriteFrames;
	}

	private void MovementStateChanged(MovementStateEnum value)
	{
		switch (value)
		{
			case MovementStateEnum.Idle:
				if (CurrentDirection == DirectionEnum.Front)
					AnimatedSprite2D.Play("idle");
				else
					AnimatedSprite2D.Play("idle_side");
				StepSoundPlayer.Stop();
				break;
			case MovementStateEnum.Moving:
				AnimatedSprite2D.Play("walk");
				StepSoundPlayer.Play();
				break;
			case MovementStateEnum.SpeechBubble:
				if (CurrentDirection == DirectionEnum.Front)
					AnimatedSprite2D.Play("talk");
				else
					AnimatedSprite2D.Play("talk_side");
				StepSoundPlayer.Stop();
				break;
			case MovementStateEnum.Dialog:
				AnimatedSprite2D.Play("idle_side");
				StepSoundPlayer.Stop();
				break;
		}
	}

	public async Task MoveTo(Vector2 position, int desiredDistance = 10, bool isRelative = false)
	{
		if (CurrentMovementState != MovementStateEnum.SpeechBubble)
		{
			NavigationAgent2D.TargetDesiredDistance = desiredDistance;

			if (isRelative)
				NavigationAgent2D.TargetPosition = Position + position;
			else
				NavigationAgent2D.TargetPosition = position;

			CurrentMovementState = MovementStateEnum.Moving;

			await ToSignal(this, SignalName.MovementFinished);
		}
	}

	public async Task SpeechBubble(string message)
	{
		CurrentMovementState = MovementStateEnum.SpeechBubble;

		var speechBubble = ResourceLoader.Load<PackedScene>("res://addons/GodotAdventureSystem/SpeechBubble.tscn").Instantiate() as SpeechBubble;
		AddChild(speechBubble);
		speechBubble.Init(message, (Resource as CharacterResource).SpeechColor, new Vector2(0, GetSize().Y));

		await ToSignal(speechBubble, global::SpeechBubble.SignalName.Finished);
		SetIdle();
	}

	public async Task PlayAnimation(string animationName, bool loop = false)
	{
		if (AnimatedSprite2D.SpriteFrames.HasAnimation(animationName))
		{
			AnimatedSprite2D.Play(animationName);
			AnimatedSprite2D.SpriteFrames.SetAnimationLoop(animationName, loop);
			await ToSignal(AnimatedSprite2D, AnimatedSprite2D.SignalName.AnimationFinished);
			AnimatedSprite2D.Play("idle");
		}
		else
		{
			Logger.Log($"Animation \"{animationName}\" not found in {(Resource as CharacterResource).ID}", Logger.LogTypeEnum.Error);
		}
	}

	public void PickUpObject()
	{
		SoundPlayer.Stream = (Resource as CharacterResource).PickupSound;
		SoundPlayer.Play();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (CurrentMovementState != MovementStateEnum.SpeechBubble)
		{
			if (NavigationAgent2D != null)
			{
				if (!NavigationAgent2D.IsNavigationFinished())
				{
					var movementDelta = (Resource as CharacterResource).MovementSpeed * delta;
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

	public void OnNavigationFinished()
	{
		CurrentMovementState = MovementStateEnum.Idle;
		EmitSignal(SignalName.MovementFinished);
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

	public new void LookAt(Vector2 position)
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
