using Godot;
using System;

public partial class PlayerCharacter : Character
{
	[Signal] public delegate void SwitchStageEventHandler(string stageID, string entryID);

	public override void _Ready()
	{
		base._Ready();
	}

	public void RequestSwitchStage(string stageID, string entryID)
	{
		EmitSignal(SignalName.SwitchStage, stageID, entryID);
	}
}
