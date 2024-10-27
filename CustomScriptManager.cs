using System.Threading.Tasks;
using Godot;
using Godot.Collections;

public partial class CustomScriptManager : ScriptManager
{
	public Game Game { get; set; }

	public CustomScriptManager(Game game) : base(game.CurrentStage.InkStory)
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
        return GetActionCounter(characterID, "talk_to");
    }

    public Variant IsInInventory(string thingID) => Game.CurrentStage.PlayerCharacter.Inventory.FindThing(thingID) is not null;
    public void SetThingName(string thingID, string name)
	{
		if (Game.CurrentStage.StageThingManager.GetThing(thingID) is not null)
			Game.CurrentStage.StageThingManager.UpdateThingName(thingID, name);
		else
		{
			var thingResource = Game.CurrentStage.PlayerCharacter.Inventory.FindThing(thingID);
			if (thingResource is not null)
				thingResource.DisplayedName = name;
		}
	}

	public void PickUp(string thingID)
	{
		var thingResource = Game.CurrentStage.StageThingManager.RemoveThing(thingID);
		Game.CurrentStage.PlayerCharacter.PickUpThing(thingResource);
	}
	public void CreateThingInInventory(string thingID)
	{
		var thing = GD.Load<PackedScene>($"res://resources/objects/{thingID}.tscn").Instantiate() as Thing;
		Game.CurrentStage.PlayerCharacter.PickUpThing(thing.Resource as ThingResource);
		thing.QueueFree();
	}

	public void StartDialog(string characterID) => QueueAction(new ScriptActionStartDialog(Game.CurrentStage.PlayerCharacter, Game, characterID));
	public void DisplayBubble(string message) => QueueAction(new ScriptActionMessage(Game.CurrentStage.PlayerCharacter, message));
	public void DisplayBubbleTo(string message, string thingID) => QueueAction(new ScriptActionMessage(Game.CurrentStage.PlayerCharacter, message, Game.CurrentStage.StageThingManager.GetThing(thingID)));
	public void CharacterWait(float seconds) => QueueAction(new ScriptActionCharacterWait(Game.CurrentStage.PlayerCharacter, seconds));
	public void MoveTo(int posX, int posY) => QueueAction(new ScriptActionMove(Game.CurrentStage.PlayerCharacter, new Vector2(posX, posY)));
	public void CharacterMoveTo(string characterID, int posX, int posY) => QueueAction(new ScriptActionMove(Game.CurrentStage.StageThingManager.GetCharacter(characterID), new Vector2(posX, posY)));
	public void MoveRelative(int posX, int posY) => QueueAction(new ScriptActionMove(Game.CurrentStage.PlayerCharacter, new Vector2(posX, posY), true));
	public void CharacterMoveRelative(string characterID, int posX, int posY) => QueueAction(new ScriptActionMove(Game.CurrentStage.StageThingManager.GetCharacter(characterID), new Vector2(posX, posY), true));
	public void LookAt(string thingID) => QueueAction(new ScriptActionLookAt(Game.CurrentStage.PlayerCharacter, Game.CurrentStage.StageThingManager.GetThing(thingID)));
	public void CharacterLookAt(string characterID, string thingID) => QueueAction(new ScriptActionLookAt(Game.CurrentStage.StageThingManager.GetCharacter(characterID), Game.CurrentStage.StageThingManager.GetThing(thingID)));
	public void PlayAnimation(string animationID) => QueueAction(new ScriptActionPlayAnimation(Game.CurrentStage.PlayerCharacter, animationID));
	public void CharacterPlayAnimation(string characterID, string animationID) => QueueAction(new ScriptActionPlayAnimation(Game.CurrentStage.StageThingManager.GetCharacter(characterID), animationID));
	public void InkSwitchStage(string stageID, string entryID) => QueueAction(new ScriptActionSwitchStage(Game.CurrentStage.PlayerCharacter, stageID, entryID));
	public int GetActionCounter(string thingID, string actionID) => Game.ThingActionCounter.GetActionCounter(thingID, actionID);
}