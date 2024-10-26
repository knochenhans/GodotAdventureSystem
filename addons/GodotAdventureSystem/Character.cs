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

	MovementStateEnum _movementState;
	MovementStateEnum MovementState
	{
		get => _movementState; set
		{
			MovementStateChanged(value);
			_movementState = value;
		}
	}

	enum OrientationEnum
	{
		Left,
		Right,
		Up,
		Down
	}

	OrientationEnum _orientation;
	OrientationEnum Orientation
	{
		get => _orientation; set
		{
			OrientationChanged(value);
			_orientation = value;
		}
	}

	NavigationAgent2D NavigationAgent2D => GetNode<NavigationAgent2D>("NavigationAgent2D");
	AnimatedSprite2D AnimatedSprite2D => GetNode<AnimatedSprite2D>("AnimatedSprite2D");
	AudioStreamPlayer2D SoundPlayer => GetNode<AudioStreamPlayer2D>("Sounds");
	AudioStreamPlayer2D StepSoundPlayer => GetNode<AudioStreamPlayer2D>("StepSounds");

	private int _scriptVisits = 0; // How many times this character's story script has been visited by the player
	public int ScriptVisits { get; set; }

	private string defaultAnimation;

	public override void _Ready()
	{
		base._Ready();

		Orientation = OrientationEnum.Right;
		MovementState = MovementStateEnum.Idle;

		AnimatedSprite2D.SpriteFrames = (Resource as CharacterResource).SpriteFrames;
		defaultAnimation = (Resource as CharacterResource).DefaultAnimation;
		if (!string.IsNullOrEmpty(defaultAnimation))
			AnimatedSprite2D.Play(defaultAnimation);
	}

	private void MovementStateChanged(MovementStateEnum value)
	{
		switch (value)
		{
			case MovementStateEnum.Idle:
				AnimatedSprite2D.Play("idle_" + Orientation.ToString().ToLower());
				StepSoundPlayer.Stop();
				break;
			case MovementStateEnum.Moving:
				AnimatedSprite2D.Play("walk_" + Orientation.ToString().ToLower());
				StepSoundPlayer.Play();
				break;
			case MovementStateEnum.SpeechBubble:
				AnimatedSprite2D.Play("talk_" + Orientation.ToString().ToLower());
				StepSoundPlayer.Stop();
				break;
			case MovementStateEnum.Dialog:
				AnimatedSprite2D.Play("idle_" + Orientation.ToString().ToLower());
				StepSoundPlayer.Stop();
				break;
		}
	}

    private void OrientationChanged(OrientationEnum value) => AnimatedSprite2D.Play("idle_" + value.ToString().ToLower());

    public async Task MoveTo(Vector2 position, int desiredDistance = 10, bool isRelative = false)
	{
		if (MovementState != MovementStateEnum.SpeechBubble)
		{
			NavigationAgent2D.TargetDesiredDistance = desiredDistance;

			if (isRelative)
				NavigationAgent2D.TargetPosition = Position + position;
			else
				NavigationAgent2D.TargetPosition = position;

			// Check angle to determine if to use left/right or up/down animation
			Vector2 targetPosition = isRelative ? Position + position : position;
			Vector2 direction = targetPosition - Position;

			if (Mathf.Abs(direction.X) > Mathf.Abs(direction.Y))
			{
				if (direction.X < 0)
					Orientation = OrientationEnum.Left;
				else
					Orientation = OrientationEnum.Right;
			}
			else
			{
				if (direction.Y < 0)
					Orientation = OrientationEnum.Up;
				else
					Orientation = OrientationEnum.Down;
			}

			MovementState = MovementStateEnum.Moving;

			await ToSignal(this, SignalName.MovementFinished);
		}
	}

	public async Task SpeechBubble(string message)
	{
		MovementState = MovementStateEnum.SpeechBubble;

		var speechBubble = ResourceLoader.Load<PackedScene>("res://addons/GodotAdventureSystem/SpeechBubble.tscn").Instantiate() as SpeechBubble;
		AddChild(speechBubble);
		speechBubble.Init(message, (Resource as CharacterResource).SpeechColor, new Vector2(0, GetSize().Y));

		await ToSignal(speechBubble, global::SpeechBubble.SignalName.Finished);
		SetIdle();
	}

	public async Task PlayAnimation(string animationName)
	{
		if (AnimatedSprite2D.SpriteFrames.HasAnimation(animationName))
		{
			AnimatedSprite2D.Play(animationName);
			AnimatedSprite2D.SpriteFrames.SetAnimationLoop(animationName, false);
			await ToSignal(AnimatedSprite2D, AnimatedSprite2D.SignalName.AnimationFinished);
			// AnimatedSprite2D.Play("idle_" + Orientation.ToString().ToLower());
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
		if (MovementState != MovementStateEnum.SpeechBubble)
		{
			if (NavigationAgent2D != null)
			{
				if (!NavigationAgent2D.IsNavigationFinished())
				{
					var movementDelta = (Resource as CharacterResource).MovementSpeed * delta;
					var nextPathPosition = NavigationAgent2D.GetNextPathPosition();
					var newVelocity = Position.DirectionTo(nextPathPosition) * (float)movementDelta;
					GlobalPosition = GlobalPosition.MoveToward(nextPathPosition + newVelocity, (float)movementDelta);
				}
			}
		}
	}

	public void OnNavigationFinished()
	{
		MovementState = MovementStateEnum.Idle;
		EmitSignal(SignalName.MovementFinished);
	}

	public Vector2 GetSize() => AnimatedSprite2D.SpriteFrames.GetFrameTexture(AnimatedSprite2D.Animation, AnimatedSprite2D.Frame).GetSize();

    public void SetIdle() => MovementState = MovementStateEnum.Idle;

    public void StartDialog() => MovementState = MovementStateEnum.Dialog;

    public void EndDialog()
	{
		MovementState = MovementStateEnum.Idle;
		AnimatedSprite2D.Play(defaultAnimation);
	}

	public new void LookAt(Vector2 position)
	{
		if (position == Position)
			Orientation = OrientationEnum.Down;
		else
		{
			if (position.X < Position.X)
				Orientation = OrientationEnum.Left;
			else if (position.X > Position.X)
				Orientation = OrientationEnum.Right;
		}
	}
}
