using Godot;
using System;

public partial class Character : Node2D
{
	[Export]
	public double Speed { get; set; } = 100f;

	[Signal]
	public delegate void CharacterMovedEventHandler();

	enum MovementStateEnum
	{
		Idle,
		Moving,
	}

	MovementStateEnum _currentMovementState;
	MovementStateEnum CurrentMovementState
	{
		get => _currentMovementState; set
		{
			if (value == MovementStateEnum.Idle)
				AnimatedSprite2D.Play("idle");
			else if (value == MovementStateEnum.Moving)
				AnimatedSprite2D.Play("walk");

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

	public override void _Ready()
	{
		NavigationAgent2D = GetNode<NavigationAgent2D>("NavigationAgent2D");
		AnimatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		CurrentDirection = DirectionEnum.Right;
	}

	public void MoveTo(Vector2 position)
	{
		NavigationAgent2D.TargetPosition = position / 4;
		CurrentMovementState = MovementStateEnum.Moving;
	}

	public override void _PhysicsProcess(double delta)
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
