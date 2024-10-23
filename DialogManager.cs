using System.Threading.Tasks;
using Godot;
using GodotInk;
using Godot.Collections;

public partial class DialogManager : GodotObject
{
	[Signal] public delegate void DialogFinishedEventHandler();

	private Game Game { get; set; }
	private Character CurrentDialogCharacter { get; set; }

	public DialogManager(Game game) => Game = game;

	public async Task StartDialog(string characterID)
	{
		Logger.Log($"Starting dialog with {characterID}", Logger.LogTypeEnum.Script);
		Game.InterfaceNode.Mode = Interface.ModeEnum.Dialog;
		Game.CurrentCommandState = Game.CommandStateEnum.Dialog;

		CurrentDialogCharacter = Game.ThingManager.GetThing(characterID) as Character;

		Game.StageNode.PlayerCharacter.LookTo(CurrentDialogCharacter.Position);
		Game.StageNode.PlayerCharacter.StartDialog();

		CurrentDialogCharacter.LookTo(Game.StageNode.PlayerCharacter.Position);
		CurrentDialogCharacter.StartDialog();

		Game.InkStory.ChoosePathString(characterID);
		Game.InkStory.Continued += _OnDialogContinue;
		Game.InterfaceNode.DialogOptionClicked += _OnDialogChoiceMade;
		Game.InkStory.ContinueMaximally();
		await ToSignal(this, SignalName.DialogFinished);
		Logger.Log($"Finished dialog with {characterID}", Logger.LogTypeEnum.Script);
	}

	private async void _OnDialogContinue()
	{
		Logger.Log($"_OnDialogContinue: {Game.InkStory.CurrentText}", Logger.LogTypeEnum.Script);
		if (Game.InkStory.CurrentText.StripEdges() != "")
		{
			var tag = Game.InkStory.GetCurrentTags();

			Character actingCharacter = Game.StageNode.PlayerCharacter;
			Character targetCharacter = CurrentDialogCharacter;

			if (tag.Count > 0 && tag[0] != "player")
			{
				actingCharacter = Game.ThingManager.GetThing(tag[0]) as Character;
				targetCharacter = Game.StageNode.PlayerCharacter;
			}

			Game.ScriptManager.ScriptActionQueue.Add(new ScriptActionMessage(actingCharacter, Game.InkStory.CurrentText, targetCharacter));
			Game.ScriptManager.ScriptActionQueue.Add(new ScriptActionCharacterWait(actingCharacter, 0.3f));
		}

		if (Game.InkStory.CanContinue)
			Game.InkStory.Continue();
		else
		{
			await Game.ScriptManager.RunActionQueue();

			if (Game.InkStory.CurrentChoices.Count > 0)
				Game.InterfaceNode.SetDialogChoiceLabels(new Array<InkChoice>(Game.InkStory.CurrentChoices));
			else
			{
				Game.InterfaceNode.Mode = Interface.ModeEnum.Normal;
				EmitSignal(SignalName.DialogFinished);
			}
		}
	}

	private async void _OnDialogChoiceMade(InkChoice choice)
	{
		Game.InterfaceNode.ClearDialogChoiceLabels();
		Game.ScriptManager.ScriptActionQueue.Add(new ScriptActionMessage(Game.StageNode.PlayerCharacter, choice.Text, CurrentDialogCharacter));
		Game.ScriptManager.ScriptActionQueue.Add(new ScriptActionCharacterWait(Game.StageNode.PlayerCharacter, 0.5f));

		await Game.ScriptManager.RunActionQueue();

		Game.InkStory.ChooseChoiceIndex(choice.Index);
		Game.InkStory.Continue();
	}

	public void OnFinishDialog()
	{
		Game.InkStory.CallDeferred("ResetCallstack");
		Game.InkStory.Continued -= _OnDialogContinue;

		Game.InterfaceNode.DialogOptionClicked -= _OnDialogChoiceMade;
		Game.InterfaceNode.ClearDialogChoiceLabels();
		Game.InterfaceNode.Mode = Interface.ModeEnum.Normal;
		Game.CurrentCommandState = Game.CommandStateEnum.Idle;

		Game.StageNode.PlayerCharacter.EndDialog();

		CurrentDialogCharacter.EndDialog();
		CurrentDialogCharacter.ScriptVisits++;
		CurrentDialogCharacter = null;
	}
}
