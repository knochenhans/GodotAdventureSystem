using Godot;
using System;

public partial class SpeechBubble : PanelContainer
{
	[Export] public float LifeTimeLengthMultiplier { get; set; } = 0.05f;
	[Export] public float MinimumLifeTime { get; set; } = 1.0f;
	[Export] public Vector2 Offset { get; set; } = new Vector2(0, -5);

	[Signal] public delegate void FinishedEventHandler();

	public Label Label => GetNode<Label>("Label");
	public Timer LifeTimer => GetNode<Timer>("LifeTimer");

	public void Init(string text, Color color, Vector2 offset)
	{
		LifeTimer.WaitTime = Math.Max(text.Length * LifeTimeLengthMultiplier, MinimumLifeTime);
		LifeTimer.Start();

		Label.Text = text;
		Label.AddThemeColorOverride("font_color", color);

		Position -= new Vector2(Size.X / 2, offset.Y + Label.Size.Y - Offset.Y);
	}

	public void _OnLifeTimerTimeout()
	{
		EmitSignal(SignalName.Finished);
		QueueFree();
	}
}
