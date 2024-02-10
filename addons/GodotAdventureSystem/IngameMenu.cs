using Godot;
using System;

public partial class IngameMenu : CanvasLayer
{
	[Signal]
	public delegate void QuitButtonPressedEventHandler();

	public void _OnSaveButtonPressed()
	{

	}

	public void _OnLoadButtonPressed()
	{

	}

	public void _OnContinueButtonPressed()
	{
		QueueFree();
	}

	public void _OnOptionsButtonPressed()
	{

	}

	public void _OnQuitButtonPressed()
	{
		EmitSignal(SignalName.QuitButtonPressed);
	}
}
