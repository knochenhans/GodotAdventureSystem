using System.Threading.Tasks;
using Godot;
using Godot.Collections;

public partial class CustomScriptManager : ScriptManager
{
	public Game Game;

	public CustomScriptManager(Game game) : base(game.InkStory)
	{
		Game = game;

		var dict = new Dictionary<string, string>
			{
				{ "get_var", MethodName.GetVar },
				{ "set_var", MethodName.SetVar },
				{ "visited", MethodName.GetScriptVisits },
				{ "is_in_inventory", MethodName.IsInInventory },
				{ "set_name", MethodName.SetThingName },
				{ "pick_up", MethodName.PickUp },
				{ "create", MethodName.CreateObject },
				{ "start_dialog", MethodName.StartDialog },
				{ "talk", MethodName.DisplayBubble },
				{ "talk_to", MethodName.DisplayBubbleTo },
				{ "wait", MethodName.CharacterWait },
				{ "move_to", MethodName.MoveTo },
				{ "move_rel", MethodName.MoveRelative },
				{ "play_anim", MethodName.PlayPlayerAnimation },
				{ "play_anim_char", MethodName.PlayCharacterAnimation },
				{ "switch_stage", MethodName.InkSwitchStage }
			};

		foreach (var item in dict)
			BindExternalFunction(item.Key, new Callable(this, item.Value));
	}

	public override async Task RunActionQueue()
	{
		// Special case for dialog
		if (ScriptActionQueue.Count == 1)
		{
			if (ScriptActionQueue[0] is ScriptActionStartDialog)
			{
				var action = ScriptActionQueue[0] as ScriptActionStartDialog;
				ScriptActionQueue.Clear();
				await action.Execute();
				return;
			}
		}

		await base.RunActionQueue();
	}

	public Variant GetVar(string varName)
	{
		return Game.VariableManager.GetVariable(varName);
	}

	public void SetVar(string varName, bool value) => Game.VariableManager.SetVariable(varName, value);
	public int GetScriptVisits(string characterID)
	{
		return Game.ThingManager.GetThing(characterID) is Character character ? character.ScriptVisits : 0;
	}
	public Variant IsInInventory(string thingID)
	{
		return Game.ThingManager.IsInInventory(thingID);
	}

	public void SetThingName(string thingID, string name) => Game.ThingManager.UpdateThingName(thingID, name);
	public void PickUp(string objectID)
	{
		Game.ThingManager.MoveThingToInventory(objectID);
		Game.StageNode.PlayerCharacter.PickUpObject();
	}
	public void CreateObject(string objectID)
	{
		Game.ThingManager.LoadThingToInventory(objectID);
		Game.StageNode.PlayerCharacter.PickUpObject();
	}
	public void StartDialog(string characterID) => ScriptActionQueue.Add(new ScriptActionStartDialog(Game.StageNode.PlayerCharacter, Game, characterID));
	public void DisplayBubble(string message) => ScriptActionQueue.Add(new ScriptActionMessage(Game.StageNode.PlayerCharacter, message));
	public void DisplayBubbleTo(string message, string thingID) => ScriptActionQueue.Add(new ScriptActionMessage(Game.StageNode.PlayerCharacter, message, Game.ThingManager.GetThing(thingID)));
	public void CharacterWait(float seconds) => ScriptActionQueue.Add(new ScriptActionCharacterWait(Game.StageNode.PlayerCharacter, seconds));
	public void MoveTo(int posX, int posY) => ScriptActionQueue.Add(new ScriptActionMove(Game.StageNode.PlayerCharacter, new Vector2(posX, posY)));
	public void MoveRelative(int posX, int posY) => ScriptActionQueue.Add(new ScriptActionMove(Game.StageNode.PlayerCharacter, new Vector2(posX, posY), true));
	public void PlayPlayerAnimation(string animationID) => ScriptActionQueue.Add(new ScriptActionPlayAnimation(Game.StageNode.PlayerCharacter, animationID));
	public void PlayCharacterAnimation(string characterID, string animationID)
	{
		var character = Game.ThingManager.GetThing(characterID);

		if (character is Character)
			ScriptActionQueue.Add(new ScriptActionPlayAnimation(character as Character, animationID));
		else
			Logger.Log($"InkPlayCharacterAnimation: Thing {characterID} is not a Character", Logger.LogTypeEnum.Error);
	}
	public void InkSwitchStage(string stageID, string entryID) => ScriptActionQueue.Add(new ScriptActionSwitchStage(Game.StageNode.PlayerCharacter, stageID, entryID));
}