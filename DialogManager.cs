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

		CurrentDialogCharacter = Game.ThingManager.GetThing(characterID) as Character;

		Game.StageNode.PlayerCharacter.LookAt(CurrentDialogCharacter.Position);
		Game.StageNode.PlayerCharacter.StartDialog();

		CurrentDialogCharacter.LookAt(Game.StageNode.PlayerCharacter.Position);
		CurrentDialogCharacter.StartDialog();

		Game.InkStory.ChoosePathString(characterID);
		Game.InkStory.Continued += OnDialogContinue;
		Game.InterfaceNode.DialogOptionClicked += OnDialogChoiceMade;
		Game.InkStory.ContinueMaximally();
		await ToSignal(this, SignalName.DialogFinished);
		Logger.Log($"Finished dialog with {characterID}", Logger.LogTypeEnum.Script);
	}

	private async void OnDialogContinue()
	{
		if (Game.InkStory.CanContinue)
			Game.InkStory.Continue();
		else
		{
			await Game.ScriptManager.RunScriptActionQueue();

			if (Game.InkStory.CurrentChoices.Count > 0)
				Game.InterfaceNode.SetDialogChoiceLabels(new Array<InkChoice>(Game.InkStory.CurrentChoices));
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
		Game.ScriptManager.QueueAction(new ScriptActionMessage(Game.StageNode.PlayerCharacter, choice.Text, CurrentDialogCharacter));
		Game.ScriptManager.QueueAction(new ScriptActionCharacterWait(Game.StageNode.PlayerCharacter, 0.5f));

		await Game.ScriptManager.RunScriptActionQueue();

		Game.InkStory.ChooseChoiceIndex(choice.Index);
		Game.InkStory.Continue();
	}

	public void OnFinishDialog()
	{
		Game.InkStory.CallDeferred("ResetCallstack");
		Game.InkStory.Continued -= OnDialogContinue;

		Game.InterfaceNode.DialogOptionClicked -= OnDialogChoiceMade;
		Game.InterfaceNode.ClearDialogChoiceLabels();
		Game.InterfaceNode.Mode = Interface.ModeEnum.Normal;
		Game.CurrentCommandState = Game.CommandStateEnum.Idle;

		Game.StageNode.PlayerCharacter.EndDialog();

		CurrentDialogCharacter.EndDialog();
		CurrentDialogCharacter.ScriptVisits++;
		CurrentDialogCharacter = null;
	}
}
