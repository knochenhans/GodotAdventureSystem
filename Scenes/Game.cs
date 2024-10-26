using Godot;
using System.Threading.Tasks;
using GodotInk;
using Godot.Collections;
using System;

public partial class Game : Scene
{
	[Export] GameResource GameResource { get; set; }
	[Export] PackedScene PlayerCharacterScene { get; set; }

	public enum CommandStateEnum
	{
		Idle,
		VerbSelected,
		Dialog
	}

	public VariableManager VariableManager { get; set; } = new();

	public Dictionary<string, string> InkStoryStates { get; set; } = new();

	public Interface InterfaceNode => GetNode<Interface>("Interface");
	public Stage CurrentStage { get; set; }
	public string currentVerbID;
	public Camera2D Camera2DNode => GetNode<Camera2D>("Camera2D");

	PackedScene IngameMenuScene => ResourceLoader.Load<PackedScene>("res://addons/GodotAdventureSystem/IngameMenu.tscn");

	CommandStateEnum _currentCommandState = CommandStateEnum.Idle;
	public CommandStateEnum CurrentCommandState
	{
		get => _currentCommandState;
		set
		{
			CommandStateChanged(value);
			_currentCommandState = value;
		}
	}

	public CustomScriptManager ScriptManager { get; set; }
	public DialogManager DialogManager { get; set; }
	public ThingActionCounter ThingActionCounter = new();

	public override void _Ready()
	{
		base._Ready();

		var cursor = ResourceLoader.Load("res://resources/cursor_64.png");
		Input.SetCustomMouseCursor(cursor, Input.CursorShape.Arrow, new Vector2(29, 29));

		SetupInterface();
		SwitchStage("meadow");

		ScriptManager = new CustomScriptManager(this);
		DialogManager = new DialogManager(this);
	}

	private void SetupInterface()
	{
		InterfaceNode.Init(GameResource.Verbs);

		InterfaceNode.GamePanelMouseMotion += OnGamePanelMouseMotion;
		InterfaceNode.GamePanelMousePressed += OnGamePanelMousePressed;

		InterfaceNode.ThingClicked += OnThingClicked;
		InterfaceNode.ThingHovered += OnThingHovered;

		InterfaceNode.VerbClicked += OnVerbClicked;
		InterfaceNode.VerbHovered += OnVerbHovered;
		InterfaceNode.VerbLeave += OnVerbLeave;
	}

	private void CommandStateChanged(CommandStateEnum value)
	{
		if (value == CommandStateEnum.Idle)
		{
			InterfaceNode.ResetCommandLabel();
			currentVerbID = "";
		}
	}

	private void SwitchStage(string stageID, string entryID = "default")
	{
		if (CurrentCommandState == CommandStateEnum.Idle)
		{
			CurrentStage?.QueueFree();

			if (ResourceLoader.Exists($"res://resources/stages/{stageID}.tscn"))
			{
				CurrentStage = ResourceLoader.Load<PackedScene>($"res://resources/stages/{stageID}.tscn").Instantiate() as Stage;
				AddChild(CurrentStage);

				CurrentStage.ThingClicked += OnThingClicked;
				CurrentStage.ThingHovered += OnThingHovered;

				var playerCharacter = PlayerCharacterScene.Instantiate() as PlayerCharacter;
				playerCharacter.SwitchStage += (stageID, entryID) => SwitchStage(stageID, entryID);

				CurrentStage.SetupPlayerCharacter(playerCharacter, entryID);

				if (InkStoryStates.ContainsKey(stageID))
					CurrentStage.InkStory.LoadState(InkStoryStates[stageID]);
			}
			else
			{
				Logger.Log($"Stage file res://resources/stages/{stageID}.tscn does not exist", Logger.LogTypeEnum.Error);
			}
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed)
		{
			switch (keyEvent.Keycode)
			{
				case Key.Escape:
					SceneManagerNode.Quit();
					// SceneManagerNode.ChangeToScene("Menu");
					break;
				case Key.F5:
					var ingameMenu = IngameMenuScene.Instantiate<IngameMenu>();
					AddChild(ingameMenu);

					ingameMenu.SaveButtonPressed += Save;
					ingameMenu.LoadButtonPressed += Load;
					ingameMenu.QuitButtonPressed += Quit;
					break;
			}
		}
	}

	public void OnVerbHovered(string verbID)
	{
		if (CurrentCommandState == CommandStateEnum.Idle)
			InterfaceNode.SetCommandLabel(GameResource.Verbs[verbID]);
	}

	public void OnVerbLeave()
	{
		if (CurrentCommandState == CommandStateEnum.Idle)
			InterfaceNode.ResetCommandLabel();
	}

	public void OnVerbClicked(string verbID)
	{
		// Logger.Log($"_OnVerbActivated: Verb: {verbID} activated", Logger.LogTypeEnum.Script);

		InterfaceNode.SetCommandLabel(GameResource.Verbs[verbID]);
		currentVerbID = verbID;
		CurrentCommandState = CommandStateEnum.VerbSelected;
	}

	public void OnGamePanelMouseMotion()
	{
		if (CurrentCommandState == CommandStateEnum.Idle)
			InterfaceNode.ResetCommandLabel();
		else if (CurrentCommandState == CommandStateEnum.VerbSelected)
			InterfaceNode.SetCommandLabel(GameResource.Verbs[currentVerbID]);
	}

	public async void OnGamePanelMousePressed(InputEventMouseButton mouseButtonEvent)
	{
		if (CurrentCommandState == CommandStateEnum.Idle)
		{
			await CurrentStage.PlayerCharacter.MoveTo(mouseButtonEvent.Position / Camera2DNode.Zoom + Camera2DNode.Position, 1);
		}
		else if (CurrentCommandState == CommandStateEnum.VerbSelected)
		{
			if (mouseButtonEvent.ButtonIndex == MouseButton.Right)
			{
				CurrentCommandState = CommandStateEnum.Idle;
				InterfaceNode.ResetCommandLabel();
			}
		}
	}

	public void OnThingHovered(string thingID)
	{
		if (CurrentCommandState != CommandStateEnum.Dialog)
		{
			var thing = CurrentStage.StageThingManager.GetThing(thingID);
			ThingResource thingResource;

			if (thing == null)
			{
				thingResource = CurrentStage.PlayerCharacter.Inventory.FindThing(thingID);
			}
			else
			{
				thingResource = thing.Resource as ThingResource;
			}

			var thingName = thingResource.DisplayedName;

			if (CurrentCommandState == CommandStateEnum.Idle)
				InterfaceNode.SetCommandLabel(thingName);
			else if (CurrentCommandState == CommandStateEnum.VerbSelected)
				InterfaceNode.SetCommandLabel($"{GameResource.Verbs[currentVerbID]} {thingName}");
		}
	}

	public async void OnThingClicked(string thingID)
	{
		if (CurrentCommandState != CommandStateEnum.Dialog)
		{
			var thing = CurrentStage.StageThingManager.GetThing(thingID);
			ThingResource thingResource;

			if (thing == null)
			{
				thingResource = CurrentStage.PlayerCharacter.Inventory.FindThing(thingID);
			}
			else
			{
				thingResource = thing.Resource as ThingResource;
				await MovePlayerToThing(thing);
			}

			await PerformVerbAction(thingID);

			// Logger.Log($"_OnObjectActivated: Object: {thing.DisplayedName} activated", Logger.LogTypeEnum.Script);

			// CurrentCommandState = CommandState.VerbSelected;
		}
	}

	private void Talk()
	{
		var tag = CurrentStage.InkStory.GetCurrentTags();

		if (tag.Count > 0 && CurrentStage.InkStory.CurrentText != "")
		{
			Character actingCharacter = CurrentStage.PlayerCharacter;
			Character targetCharacter = null;

			if (_currentCommandState == CommandStateEnum.Dialog)
			{
				if (tag[0] == "player")
					targetCharacter = DialogManager.CurrentDialogCharacter;
				else
					targetCharacter = CurrentStage.PlayerCharacter;
			}
			if (tag[0] != "player")
			{
				actingCharacter = CurrentStage.StageThingManager.GetThing(tag[0]) as Character;
			}

			ScriptManager.QueueAction(new ScriptActionMessage(actingCharacter, CurrentStage.InkStory.CurrentText, targetCharacter));
			ScriptManager.QueueAction(new ScriptActionCharacterWait(actingCharacter, 0.3f));
		}
	}

	private async Task PerformVerbAction(string thingID)
	{
		string performedAction;

		CurrentStage.InkStory.Continued += Talk;

		if (CurrentCommandState == CommandStateEnum.VerbSelected)
		{
			performedAction = currentVerbID;

			// Interact with the object
			if (!CurrentStage.InkStory.EvaluateFunction("verb", thingID, currentVerbID).AsBool())
			{
				// No scripted reaction found, use the default one
				ScriptManager.QueueAction(new ScriptActionMessage(CurrentStage.PlayerCharacter, GameResource.DefaultVerbReactions[currentVerbID]));
			}

			await ScriptManager.RunScriptActionQueue();

			CurrentCommandState = CommandStateEnum.Idle;
		}
		else
		{
			CurrentStage.InkStory.EvaluateFunction("verb", thingID, "walk");
			InterfaceNode.SetCommandLabel(CurrentStage.StageThingManager.GetThingName(thingID));
			await ScriptManager.RunScriptActionQueue();
			performedAction = "walk";
		}

		ThingActionCounter.IncrementActionCounter(thingID, performedAction);

		CurrentStage.InkStory.Continued -= Talk;
	}

	private async Task MovePlayerToThing(Thing thing)
	{
		Vector2 position = Vector2.Zero;
		if (thing is Object object_)
			position = object_.Position;
		else if (thing is Character character)
			position = character.Position;
		else if (thing is HotspotArea hotspotArea)
			position = hotspotArea.GetClosestPoint(CurrentStage.PlayerCharacter.Position) + hotspotArea.Position;
		else
			Logger.Log($"OnAreaActivated: Area {(thing.Resource as ThingResource).ID} is not an Object or a HotspotArea", Logger.LogTypeEnum.Error);

		// if (position.DistanceTo(StageNode.PlayerCharacter.Position) > 20)
		await CurrentStage.PlayerCharacter.MoveTo(position, 20);
	}

	public async Task StartDialog(string characterID) => await DialogManager.StartDialog(characterID);

	public override void _Process(double delta)
	{
		var viewportRect = CurrentStage.GetViewportRect();
		var cameraPositionX = Mathf.Clamp(CurrentStage.PlayerCharacter.Position.X - viewportRect.Size.X / (2 * Camera2DNode.Zoom.X), 0, viewportRect.Size.X / Camera2DNode.Zoom.X);
		Camera2DNode.Position = new Vector2(cameraPositionX, Camera2DNode.Position.Y);
	}

	public void Save()
	{
		// Save general data, variables, and ink story state
		Dictionary<string, Variant> saveData = new()
		{
			{ "stageID", CurrentStage.ID },
			{ "position", CurrentStage.PlayerCharacter.Position },
			{ "orientation", CurrentStage.PlayerCharacter.Orientation.ToString() },
			{ "cameraPosition", Camera2DNode.Position },
			{ "variables", VariableManager.GetVariables() },
		};

		// Save things in stage
		var thingsStage = CurrentStage.StageThingManager.GetThings();

		Dictionary<string, Dictionary<string, Variant>> thingsStageData = new()
		{
			[CurrentStage.ID] = new()
		};

		foreach (var thing in thingsStage)
		{
			ThingResource thingResource = thing.Resource as ThingResource;

			Dictionary<string, Variant> thingData = new()
			{
				{ "displayedName", thingResource.DisplayedName },
				{ "position", thing.Position },
				{ "actionCounter", ThingActionCounter.GetActionCounters(thingResource.ID) }
			};
			thingsStageData[CurrentStage.ID].Add(thingResource.ID, thingData);
		}

		saveData.Add("things", thingsStageData);

		// Save things in inventory
		var thingsInventory = CurrentStage.PlayerCharacter.Inventory.GetThings();

		Array<Dictionary<string, Variant>> thingsInventoryData = new();

		foreach (var thing in thingsInventory)
		{
			ThingResource thingResource = thing;

			Dictionary<string, Variant> thingData = new()
			{
				{ "ID", thingResource.ID },
				{ "displayedName", thingResource.DisplayedName },
				{ "actionCounter", ThingActionCounter.GetActionCounters(thingResource.ID) }
			};
			thingsInventoryData.Add(thingData);
		}

		saveData.Add("inventory", thingsInventoryData);

		// Save ink story states per stage
		Dictionary<string, string> inkStoryStates = new()
		{
			[CurrentStage.ID] = CurrentStage.InkStory.SaveState()
		};

		saveData.Add("inkStoryStates", inkStoryStates);

		// Write save data to file
		using var saveFile = FileAccess.Open("user://savegame.save", FileAccess.ModeFlags.Write);
		saveFile.StoreVar(saveData);
	}

	public void Load()
	{
		InterfaceNode.Reset();

		// Read save data from file
		using var saveFile = FileAccess.Open("user://savegame.save", FileAccess.ModeFlags.Read);

		if (saveFile.GetLength() > 0)
		{
			// Load general data
			var saveData = (Dictionary<string, Variant>)saveFile.GetVar();

			// Load ink story states per stage
			InkStoryStates = (Dictionary<string, string>)saveData["inkStoryStates"];

			SwitchStage(saveData["stageID"].ToString());
			CurrentStage.PlayerCharacter.Position = (Vector2)saveData["position"];
			CurrentStage.PlayerCharacter.Orientation = Enum.Parse<Character.OrientationEnum>(saveData["orientation"].ToString());
			Camera2DNode.Position = (Vector2)saveData["cameraPosition"];

			// Load variables
			VariableManager.SetVariables((Dictionary<string, Variant>)saveData["variables"]);

			// Load things in stage
			var thingsStageData = (Dictionary<string, Dictionary<string, Variant>>)saveData["things"];

			Array<Thing> list = CurrentStage.StageThingManager.GetThings();
			for (int i = list.Count - 1; i >= 0; i--)
			{
				Thing thing = list[i];
				var thingID = (thing.Resource as ThingResource).ID;

				if (thingsStageData[CurrentStage.ID].ContainsKey(thingID))
				{
					var thingData = (Dictionary<string, Variant>)thingsStageData[CurrentStage.ID][thingID];

					var thingResource = thing.Resource as ThingResource;

					thingResource.DisplayedName = thingData["displayedName"].ToString();
					thing.Position = (Vector2)thingData["position"];
					ThingActionCounter.SetActionCounters(thingResource.ID, (Dictionary<string, int>)thingData["actionCounter"]);
					// list.RemoveAt(i);
				}
				else
					CurrentStage.StageThingManager.RemoveThing(thingID);
			}

			// Load things in inventory
			var thingsInventoryData = (Array<Dictionary<string, Variant>>)saveData["inventory"];

			CurrentStage.PlayerCharacter.Inventory.Clear();

			foreach (var thingData in thingsInventoryData)
			{
				var thing = GD.Load<PackedScene>($"res://resources/objects/{thingData["ID"]}.tscn").Instantiate() as Thing;
				var thingResource = thing.Resource as ThingResource;
				thing.QueueFree();

				thingResource.DisplayedName = thingData["displayedName"].ToString();
				ThingActionCounter.SetActionCounters(thingResource.ID, (Dictionary<string, int>)thingData["actionCounter"]);

				CurrentStage.PlayerCharacter.AddThingToInventory(thingResource);
			}
		}
		else
		{
			Logger.Log("No save data found", Logger.LogTypeEnum.Error);
		}
	}

	public void Quit()
	{
		GetTree().Quit();
	}
}
