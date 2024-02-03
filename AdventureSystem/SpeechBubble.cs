using Godot;
using System;

public partial class SpeechBubble : PanelContainer
{
	public RichTextLabel RichTextLabel { get; set; }
	public Timer LifeTimer { get; set; }

	[Export]
	public float LifeTimeLengthMultiplier { get; set; } = 0.05f;

	[Export]
	public float MinimumLifeTime { get; set; } = 1.0f;

	[Export]
	public Vector2 Offset { get; set; } = new Vector2(0, -5);

	[Signal]
	public delegate void FinishedEventHandler();

	public override void _Ready()
	{
		RichTextLabel = GetNode<RichTextLabel>("Text");
		LifeTimer = GetNode<Timer>("LifeTimer");
	}

	public void Init(string text, Color color, Vector2 offset)
	{
		RichTextLabel.Text = $"[center]{text}[/center]";
		RichTextLabel.AddThemeColorOverride("font_color", color);
		LifeTimer.WaitTime = Math.Max(text.Length * LifeTimeLengthMultiplier, MinimumLifeTime);
		LifeTimer.Start();

		var height = RichTextLabel.GetContentHeight() / GetViewport().GetCamera2D().Zoom.Y;
		Position -= new Vector2(Size.X / 2, offset.Y + height - Offset.Y);
	}

	public void _OnLifeTimerTimeout()
	{
		EmitSignal(SignalName.Finished);
		QueueFree();
	}
}
