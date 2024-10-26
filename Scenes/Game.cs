using Godot;
using System.Threading.Tasks;
using GodotInk;
using Godot.Collections;

public partial class Game : Scene
{
	[Export] GameResource GameResource { get; set; }
	[Export] public InkStory InkStory { get; set; }
	[Export] PackedScene PlayerCharacterScene { get; set; }

	public enum CommandStateEnum
	{
		Idle,
		VerbSelected,
		Dialog
	}

	public VariableManager VariableManager { get; set; } = new();

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

		ScriptManager = new CustomScriptManager(this);
		DialogManager = new DialogManager(this);

		SwitchStage("meadow");
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
		var tag = InkStory.GetCurrentTags();

		if (tag.Count > 0 && InkStory.CurrentText != "")
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

			ScriptManager.QueueAction(new ScriptActionMessage(actingCharacter, InkStory.CurrentText, targetCharacter));
			ScriptManager.QueueAction(new ScriptActionCharacterWait(actingCharacter, 0.3f));
		}
	}

	private async Task PerformVerbAction(string thingID)
	{
		string performedAction;

		InkStory.Continued += Talk;

		if (CurrentCommandState == CommandStateEnum.VerbSelected)
		{
			// Interact with the object
			if (!InkStory.EvaluateFunction("verb", thingID, currentVerbID).AsBool())
			{
				// No scripted reaction found, use the default one
				ScriptManager.QueueAction(new ScriptActionMessage(CurrentStage.PlayerCharacter, GameResource.DefaultVerbReactions[currentVerbID]));
			}

			await ScriptManager.RunScriptActionQueue();

			performedAction = currentVerbID;
			CurrentCommandState = CommandStateEnum.Idle;
		}
		else
		{
			InkStory.EvaluateFunction("verb", thingID, "walk");
			InterfaceNode.SetCommandLabel(CurrentStage.StageThingManager.GetThingName(thingID));
			await ScriptManager.RunScriptActionQueue();
			performedAction = "walk";
		}

		ThingActionCounter.IncrementActionCounter(thingID, performedAction);

		InkStory.Continued -= Talk;
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
		if (CurrentStage.PlayerCharacter.Position.X > CurrentStage.GetViewportRect().Size.X / 8)
		{
			if (Camera2DNode.Position.X + CurrentStage.GetViewportRect().Size.X / 4 < CurrentStage.GetSize().X)
				Camera2DNode.Position = new Vector2(CurrentStage.PlayerCharacter.Position.X - CurrentStage.GetViewportRect().Size.X / 8, Camera2DNode.Position.Y);
		}
		else if (CurrentStage.PlayerCharacter.Position.X < CurrentStage.GetViewportRect().Size.X / 8)
		{
			if (Camera2DNode.Position.X - CurrentStage.GetViewportRect().Size.X / 4 > 0)
				Camera2DNode.Position = new Vector2(CurrentStage.PlayerCharacter.Position.X - CurrentStage.GetViewportRect().Size.X / 8, Camera2DNode.Position.Y);
		}
	}

	public void Save()
	{
		Dictionary<string, Variant> saveData = new()
		{
			{ "stageID", CurrentStage.ID },
			{ "position", CurrentStage.PlayerCharacter.Position }
		};

		var things = CurrentStage.StageThingManager.GetThings();

		// Write save data to file
		using var saveFile = FileAccess.Open("user://savegame.save", FileAccess.ModeFlags.Write);
		saveFile.StoreVar(saveData);
	}

	public void Load()
	{
		// Read save data from file
		using var saveFile = FileAccess.Open("user://savegame.save", FileAccess.ModeFlags.Read);

		if (saveFile.GetLength() > 0)
		{
			var saveData = (Dictionary<string, Variant>)saveFile.GetVar();

			SwitchStage(saveData["stageID"].ToString());
			CurrentStage.PlayerCharacter.Position = (Vector2)saveData["position"];
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
