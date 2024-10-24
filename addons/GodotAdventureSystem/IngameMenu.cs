using Godot;
using System;

public partial class IngameMenu : CanvasLayer
{
	[Signal]
	public delegate void QuitButtonPressedEventHandler();

	public void OnSaveButtonPressed()
	{

	}

	public void OnLoadButtonPressed()
	{

	}

	public void OnContinueButtonPressed()
	{
		QueueFree();
	}

	public void OnOptionsButtonPressed()
	{

	}

	public void OnQuitButtonPressed()
	{
		EmitSignal(SignalName.QuitButtonPressed);
	}
}
