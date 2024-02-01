using Godot;
using System;

public partial class SpeechBubble : PanelContainer
{
	public RichTextLabel RichTextLabel { get; set; }
	public Timer LifeTimer { get; set; }

	[Export]
	public float LifeTimeLengthMultiplier { get; set; } = 0.05f;

	[Signal]
	public delegate void FinishedEventHandler();

	public override void _Ready()
	{
		RichTextLabel = GetNode<RichTextLabel>("Text");
		LifeTimer = GetNode<Timer>("LifeTimer");
	}

	public void Init(string text, Color color)
	{
		RichTextLabel.Text = $"[center]{text}[/center]";
		RichTextLabel.AddThemeColorOverride("font_color", color);
		LifeTimer.WaitTime = text.Length * LifeTimeLengthMultiplier;
		LifeTimer.Start();
	}

	public void _OnLifeTimerTimeout()
	{
		EmitSignal(SignalName.Finished);
		QueueFree();
	}
}
