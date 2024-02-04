using Godot;
using Godot.Collections;

public partial class SceneManager : Node
{
	[Export]
	Array<string> SceneNames;

	[Export]
	string InitialSceneName;

	string CurrentSceneName { get; set; } = "";
	Scene CurrentScene { get; set; }

	public override void _Ready()
	{
		CurrentSceneName = InitialSceneName;

		CallDeferred("ChangeToScene", CurrentSceneName);
	}

	public async void ChangeToScene(string sceneName)
	{
		CurrentSceneName = sceneName;

		if (CurrentScene != null)
		{
			CurrentScene.Exit();
			await ToSignal(CurrentScene, "ExitFinished");
		}

		var newScene = ResourceLoader.Load<PackedScene>($"res://scenes/{CurrentSceneName}.tscn").Instantiate();
		GetTree().Root.AddChild(newScene);
		GetTree().CurrentScene = newScene;
		CurrentScene = newScene as Scene;
	}

	public void RestartScene() => GetTree().ReloadCurrentScene();

	public async void Quit()
	{
		if (CurrentScene != null)
		{
			CurrentScene.Exit();
			await ToSignal(CurrentScene, "ExitFinished");
		}

		GetTree().Quit();
	}
}
