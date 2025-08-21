using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using GodotInk;

public partial class CustomScriptManager : ScriptManager
{
	public CustomGame Game { get; set; }

	public CustomScriptManager(CustomGame game, InkStory inkStory, SceneTree sceneTree) : base(inkStory, sceneTree)
	{
		Game = game;

		var functionMappings = new Dictionary<string, string>
			{
				{ "get_var", MethodName.GetVar },
				{ "set_var", MethodName.SetVar },
				{ "visited", MethodName.GetScriptVisits },
				{ "is_in_inventory", MethodName.IsInInventory },
				{ "set_name", MethodName.SetThingName },
				{ "pick_up", MethodName.PickUp },
				{ "create", MethodName.CreateThingInInventory },
				{ "start_dialog", MethodName.StartDialog },
				{ "talk", MethodName.DisplayBubble },
				{ "talk_to", MethodName.DisplayBubbleTo },
				{ "wait", MethodName.CharacterWait },
				{ "look_at", MethodName.LookAt },
				{ "look_at_c", MethodName.CharacterLookAt },
				{ "move_to", MethodName.MoveTo },
				{ "move_rel", MethodName.MoveRelative },
				{ "play_anim", MethodName.PlayAnimation },
				{ "play_anim_c", MethodName.CharacterPlayAnimation },
				{ "switch_stage", MethodName.InkSwitchStage },
				{ "get_action_count", MethodName.GetActionCounter },
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
	public int GetScriptVisits(string characterID)
	{
		// return GetActionCounter(characterID, "talk_to");
		return 0;
	}

	public Variant IsInInventory(string stageNodeID) => Game.GetEntity("player").Inventory.GetItemCount(stageNodeID) > 0;

	public void SetThingName(string stageNodeID, string name)
	{
		var node = CustomGame.GetCurrentAdventureStage().GetStageNodeByID(stageNodeID);
		if (node is not null)
		{
			node.DisplayName = name;
		}
		// else
		// {
		// 	var thingResource = Game.GetEntity("player").Inventory.FindThing(stageNodeID);
		// 	if (thingResource is not null)
		// 		thingResource.DisplayedName = name;
		// }
	}

	public void PickUp(string stageNodeID)
	{
		var node = CustomGame.GetCurrentAdventureStage().GetStageNodeByID(stageNodeID);
		if (node is not null)
		{
			CustomGame.GetCurrentAdventureStage().MoveObjectToEntityInventory(node.ID, "player");
		}
	}

	public void CreateThingInInventory(string stageNodeID)
	{
		Game.GetEntity("player").Inventory.AddItem(stageNodeID, 1);
	}

	public void StartDialog(string characterID) => QueueAction(new ScriptActionStartDialog(Game.GetEntity("player"), Game, characterID));
	public void DisplayBubble(string message) => QueueAction(new ScriptActionMessage(Game.GetEntity("player"), message));
	public void DisplayBubbleTo(string message, string stageNodeID) => QueueAction(new ScriptActionMessage(Game.GetEntity("player"), message, CustomGame.GetCurrentAdventureStage().GetStageNodeByID(stageNodeID)));
	public void CharacterWait(float seconds) => QueueAction(new ScriptActionCharacterWait(Game.GetEntity("player"), seconds));
	public void MoveTo(int posX, int posY) => QueueAction(new ScriptActionMove(Game.GetEntity("player"), new Vector2(posX, posY)));
	public void CharacterMoveTo(string characterID, int posX, int posY) => QueueAction(new ScriptActionMove(CustomGame.GetCurrentAdventureStage().GetAdventureEntityByID(characterID), new Vector2(posX, posY)));
	public void MoveRelative(int posX, int posY) => QueueAction(new ScriptActionMove(Game.GetEntity("player"), new Vector2(posX, posY), true));
	public void CharacterMoveRelative(string characterID, int posX, int posY) => QueueAction(new ScriptActionMove(CustomGame.GetCurrentAdventureStage().GetAdventureEntityByID(characterID), new Vector2(posX, posY), true));
	public void LookAt(string stageNodeID) => QueueAction(new ScriptActionLookAt(Game.GetEntity("player"), CustomGame.GetCurrentAdventureStage().GetStageNodeByID(stageNodeID)));
	public void CharacterLookAt(string characterID, string stageNodeID) => QueueAction(new ScriptActionLookAt(CustomGame.GetCurrentAdventureStage().GetAdventureEntityByID(characterID), CustomGame.GetCurrentAdventureStage().GetStageNodeByID(stageNodeID)));
	public void PlayAnimation(string animationID) => QueueAction(new ScriptActionPlayAnimation(Game.GetEntity("player"), animationID));
	public void CharacterPlayAnimation(string characterID, string animationID) => QueueAction(new ScriptActionPlayAnimation(CustomGame.GetCurrentAdventureStage().GetAdventureEntityByID(characterID), animationID));
	public void InkSwitchStage(string stageID, string entryID) => QueueAction(new ScriptActionSwitchStage(Game.GetEntity("player"), stageID, entryID));
	public int GetActionCounter(string stageNodeID, string actionID) => Game.StageNodeActionCounter.GetActionCounter(stageNodeID, actionID);
}
