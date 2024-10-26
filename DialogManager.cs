using System.Threading.Tasks;
using Godot;
using GodotInk;
using Godot.Collections;

public partial class DialogManager : GodotObject
{
	[Signal] public delegate void DialogFinishedEventHandler();

	private Game Game { get; set; }
	public Character CurrentDialogCharacter { get; set; }

	public DialogManager(Game game) => Game = game;

	public async Task StartDialog(string characterID)
	{
		Logger.Log($"Starting dialog with {characterID}", Logger.LogTypeEnum.Script);
		Game.InterfaceNode.Mode = Interface.ModeEnum.Dialog;
		Game.CurrentCommandState = Game.CommandStateEnum.Dialog;

		CurrentDialogCharacter = Game.CurrentStage.StageThingManager.GetThing(characterID) as Character;

		Game.CurrentStage.PlayerCharacter.LookAt(CurrentDialogCharacter.Position);
		Game.CurrentStage.PlayerCharacter.StartDialog();

		CurrentDialogCharacter.LookAt(Game.CurrentStage.PlayerCharacter.Position);
		CurrentDialogCharacter.StartDialog();

		Game.CurrentStage.InkStory.ChoosePathString(characterID);
		Game.CurrentStage.InkStory.Continued += OnDialogContinue;
		DialogFinished += OnDialogFinished;
		Game.InterfaceNode.DialogOptionClicked += OnDialogChoiceMade;
		Game.CurrentStage.InkStory.ContinueMaximally();
		await ToSignal(this, SignalName.DialogFinished);
		Logger.Log($"Finished dialog with {characterID}", Logger.LogTypeEnum.Script);
	}

	private async void OnDialogContinue()
	{
		if (Game.CurrentStage.InkStory.CanContinue)
			Game.CurrentStage.InkStory.Continue();
		else
		{
			await Game.ScriptManager.RunScriptActionQueue();

			if (Game.CurrentStage.InkStory.CurrentChoices.Count > 0)
				Game.InterfaceNode.SetDialogChoiceLabels(new Array<InkChoice>(Game.CurrentStage.InkStory.CurrentChoices));
			else
			{
				Game.InterfaceNode.Mode = Interface.ModeEnum.Normal;
				EmitSignal(SignalName.DialogFinished);
			}
		}
	}

	private async void OnDialogChoiceMade(InkChoice choice)
	{
		Game.InterfaceNode.ClearDialogChoiceLabels();
		Game.ScriptManager.QueueAction(new ScriptActionMessage(Game.CurrentStage.PlayerCharacter, choice.Text, CurrentDialogCharacter));
		Game.ScriptManager.QueueAction(new ScriptActionCharacterWait(Game.CurrentStage.PlayerCharacter, 0.5f));

		await Game.ScriptManager.RunScriptActionQueue();

		Game.CurrentStage.InkStory.ChooseChoiceIndex(choice.Index);
		Game.CurrentStage.InkStory.Continue();
	}

	public void OnDialogFinished()
	{
		Game.CurrentStage.InkStory.CallDeferred("ResetCallstack");
		Game.CurrentStage.InkStory.Continued -= OnDialogContinue;
		DialogFinished -= OnDialogFinished;

		Game.InterfaceNode.DialogOptionClicked -= OnDialogChoiceMade;
		Game.InterfaceNode.ClearDialogChoiceLabels();
		Game.InterfaceNode.Mode = Interface.ModeEnum.Normal;
		Game.CurrentCommandState = Game.CommandStateEnum.Idle;

		Game.CurrentStage.PlayerCharacter.EndDialog();

		CurrentDialogCharacter.EndDialog();
		CurrentDialogCharacter = null;
	}
}
