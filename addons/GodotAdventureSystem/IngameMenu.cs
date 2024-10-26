using Godot;
using System;

public partial class IngameMenu : CanvasLayer
{
	[Signal] public delegate void QuitButtonPressedEventHandler();
	[Signal] public delegate void SaveButtonPressedEventHandler();
	[Signal] public delegate void LoadButtonPressedEventHandler();
	[Signal] public delegate void OptionsButtonPressedEventHandler();

	public void OnSaveButtonPressed()
	{
		EmitSignal(SignalName.SaveButtonPressed);
		QueueFree();
	}

	public void OnLoadButtonPressed()
	{
		EmitSignal(SignalName.LoadButtonPressed);
		QueueFree();
	}

	public void OnContinueButtonPressed()
	{
		QueueFree();
	}

	public void OnOptionsButtonPressed()
	{
		EmitSignal(SignalName.OptionsButtonPressed);
	}

	public void OnQuitButtonPressed()
	{
		EmitSignal(SignalName.QuitButtonPressed);
	}
}
