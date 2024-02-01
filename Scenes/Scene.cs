using Godot;
using System;
using System.Threading.Tasks;

public partial class Scene : Node
{
	[Signal]
	public delegate void ReadyFinishedEventHandler();

	[Signal]
	public delegate void ExitFinishedEventHandler();

	[Export]
	public float FadeInTime = 1f;

	[Export]
	public float FadeOutTime = 1f;

	PackedScene FadeScene = ResourceLoader.Load<PackedScene>("res://Fade/Fade.tscn");

	public SceneManager SceneManagerNode { get; set; }

	private async Task Fade(global::Fade.FadeDirectionEnum direction, float time)
	{
		var fade = FadeScene.Instantiate<Fade>();
		GetTree().Root.AddChild(fade);
		fade.Run(direction, time);
		await ToSignal(fade, "FadeFinished");
	}
	
	public async override void _Ready()
	{
		SceneManagerNode = GetNode<SceneManager>("/root/SceneManager");

		await Fade(global::Fade.FadeDirectionEnum.In, FadeInTime);

		EmitSignal(SignalName.ReadyFinished);
	}

	public async void Exit()
	{
		await Fade(global::Fade.FadeDirectionEnum.Out, FadeOutTime);

		EmitSignal(SignalName.ExitFinished);
		QueueFree();
	}
}
