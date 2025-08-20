using System.Threading.Tasks;
using Godot;
using Godot.Collections;

public partial class CustomScriptManager : ScriptManager
{
	public CustomGame Game { get; set; }

	public CustomScriptManager(CustomGame game, SceneTree sceneTree) : base(game.GetCurrentInkStory(), sceneTree)
	{
		Game = game;

		var functionMappings = new Dictionary<string, string>
			{
				// { "get_var", MethodName.GetVar },
				// { "set_var", MethodName.SetVar },
				// { "visited", MethodName.GetScriptVisits },
				// { "is_in_inventory", MethodName.IsInInventory },
				// { "set_name", MethodName.SetThingName },
				// { "pick_up", MethodName.PickUp },
				// { "create", MethodName.CreateThingInInventory },
				// { "start_dialog", MethodName.StartDialog },
				// { "talk", MethodName.DisplayBubble },
				// { "talk_to", MethodName.DisplayBubbleTo },
				// { "wait", MethodName.CharacterWait },
				// { "look_at", MethodName.LookAt },
				// { "look_at_c", MethodName.CharacterLookAt },
				{ "move_to", MethodName.MoveTo },
				// { "move_rel", MethodName.MoveRelative },
				// { "play_anim", MethodName.PlayAnimation },
				// { "play_anim_c", MethodName.CharacterPlayAnimation },
				// { "switch_stage", MethodName.InkSwitchStage },
				// { "get_action_count", MethodName.GetActionCounter },
			};

		foreach (var item in functionMappings)
			BindExternalFunction(item.Key, new Callable(this, item.Value));
	}

	public override async Task RunScriptActionQueue()
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

		await base.RunScriptActionQueue();
	}

	public Variant GetVar(string varName) => Game.VariableManager.GetVariable(varName);
	public void SetVar(string varName, bool value) => Game.VariableManager.SetVariable(varName, value);
    // public int GetScriptVisits(string characterID)
    // {
    //     return GetActionCounter(characterID, "talk_to");
    // }

    // public Variant IsInInventory(string thingID) => Game.GetCurrentStage().PlayerCharacter.Inventory.FindThing(thingID) is not null;

	// public void SetThingName(string thingID, string name)
	// {
	// 	if (Game.GetCurrentStage().StageThingManager.GetThing(thingID) is not null)
	// 		Game.GetCurrentStage().StageThingManager.UpdateThingName(thingID, name);
	// 	else
	// 	{
	// 		var thingResource = Game.GetCurrentStage().PlayerCharacter.Inventory.FindThing(thingID);
	// 		if (thingResource is not null)
	// 			thingResource.DisplayedName = name;
	// 	}
	// }

	// public void PickUp(string thingID)
	// {
	// 	var thingResource = Game.GetCurrentStage().StageThingManager.GetThing(thingID).Resource as ThingResource;
	// 	Game.GetCurrentStage().PlayerCharacter.PickUpThing(thingResource);
	// }

	// public void CreateThingInInventory(string thingID)
	// {
	// 	var thing = GD.Load<PackedScene>($"res://resources/objects/{thingID}.tscn").Instantiate() as Thing;
	// 	Game.GetCurrentStage().PlayerCharacter.PickUpThing(thing.Resource as ThingResource);
	// 	thing.QueueFree();
	// }

	// public void StartDialog(string characterID) => QueueAction(new ScriptActionStartDialog(Game.GetCurrentStage().PlayerCharacter, Game, characterID));
	// public void DisplayBubble(string message) => QueueAction(new ScriptActionMessage(Game.GetCurrentStage().PlayerCharacter, message));
	// public void DisplayBubbleTo(string message, string thingID) => QueueAction(new ScriptActionMessage(Game.GetCurrentStage().PlayerCharacter, message, Game.GetCurrentStage().StageThingManager.GetThing(thingID)));
	// public void CharacterWait(float seconds) => QueueAction(new ScriptActionCharacterWait(Game.GetCurrentStage().PlayerCharacter, seconds));
	public void MoveTo(int posX, int posY) => QueueAction(new ScriptActionMove(Game.GetEntity("player"), new Vector2(posX, posY)));
	// public void CharacterMoveTo(string characterID, int posX, int posY) => QueueAction(new ScriptActionMove(Game.GetCurrentStage().StageThingManager.GetCharacter(characterID), new Vector2(posX, posY)));
	// public void MoveRelative(int posX, int posY) => QueueAction(new ScriptActionMove(Game.GetCurrentStage().PlayerCharacter, new Vector2(posX, posY), true));
	// public void CharacterMoveRelative(string characterID, int posX, int posY) => QueueAction(new ScriptActionMove(Game.GetCurrentStage().StageThingManager.GetCharacter(characterID), new Vector2(posX, posY), true));
	// public void LookAt(string thingID) => QueueAction(new ScriptActionLookAt(Game.GetCurrentStage().PlayerCharacter, Game.GetCurrentStage().StageThingManager.GetThing(thingID)));
	// public void CharacterLookAt(string characterID, string thingID) => QueueAction(new ScriptActionLookAt(Game.GetCurrentStage().StageThingManager.GetCharacter(characterID), Game.GetCurrentStage().StageThingManager.GetThing(thingID)));
	// public void PlayAnimation(string animationID) => QueueAction(new ScriptActionPlayAnimation(Game.GetCurrentStage().PlayerCharacter, animationID));
	// public void CharacterPlayAnimation(string characterID, string animationID) => QueueAction(new ScriptActionPlayAnimation(Game.GetCurrentStage().StageThingManager.GetCharacter(characterID), animationID));
	// public void InkSwitchStage(string stageID, string entryID) => QueueAction(new ScriptActionSwitchStage(Game.GetCurrentStage().PlayerCharacter, stageID, entryID));
	// public int GetActionCounter(string thingID, string actionID) => Game.ThingActionCounter.GetActionCounter(thingID, actionID);
}