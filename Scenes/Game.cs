using Godot;
using System.Threading.Tasks;
using GodotInk;

public partial class Game : Scene
{
	[Signal] public delegate void DialogFinishedEventHandler();

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
	public ThingManager ThingManager { get; set; } = new();

	public Interface InterfaceNode => GetNode<Interface>("Interface");
	public Stage StageNode { get; set; }
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

	public ScriptManager ScriptManager { get; set; }
	public DialogManager DialogManager { get; set; }
	public ThingActionCounter ThingActionCounter = new();

	public override void _Ready()
	{
		base._Ready();

		var cursor = ResourceLoader.Load("res://resources/cursor_64.png");
		Input.SetCustomMouseCursor(cursor, Input.CursorShape.Arrow, new Vector2(29, 29));

		SetupInterface();

		SwitchStage("Meadow");

		ThingManager.AddThingToIventory += InterfaceNode.OnObjectAddedToInventory;

		ScriptManager = new CustomScriptManager(this);

		DialogManager = new DialogManager(this);
		DialogFinished += DialogManager.OnFinishDialog;
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
			StageNode?.QueueFree();

			StageNode = ResourceLoader.Load<PackedScene>($"res://resources/{stageID}.tscn").Instantiate() as Stage;
			AddChild(StageNode);

			StageNode.ThingClicked += OnThingClicked;
			StageNode.ThingHovered += OnThingHovered;

			var playerCharacter = PlayerCharacterScene.Instantiate() as PlayerCharacter;
			playerCharacter.SwitchStage += (stageID, entryID) => SwitchStage(stageID, entryID);

			StageNode.SetupPlayerCharacter(playerCharacter, entryID);

			ThingManager.Clear();
			ThingManager.RegisterThings(StageNode.CollectThings());
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
			await StageNode.PlayerCharacter.MoveTo(mouseButtonEvent.Position / Camera2DNode.Zoom + Camera2DNode.Position, 1);
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
			var thing = ThingManager.GetThing(thingID);

			if (thing == null)
			{
				Logger.Log($"OnThingHovered: Thing {thingID} not registered in ThingManager", Logger.LogTypeEnum.Error);
			}
			else
			{
				// Hovered inventory item
				if (CurrentCommandState == CommandStateEnum.Idle)
					InterfaceNode.SetCommandLabel(ThingManager.GetThingName(thingID));
				else if (CurrentCommandState == CommandStateEnum.VerbSelected)
					InterfaceNode.SetCommandLabel($"{GameResource.Verbs[currentVerbID]} {ThingManager.GetThingName(thingID)}");
			}
		}
	}

	public async void OnThingClicked(string thingID)
	{
		if (CurrentCommandState != CommandStateEnum.Dialog)
        {
            var thing = ThingManager.GetThing(thingID);

            if (thing != null)
            {
                // For objects (that are not in the inventory) and hotspots, move the player character to the object
                if (!ThingManager.IsInInventory(thingID))
                    await MovePlayerToThing(thing);
            }

            await PerformVerbAction(thingID);

            // Logger.Log($"_OnObjectActivated: Object: {thing.DisplayedName} activated", Logger.LogTypeEnum.Script);

            // CurrentCommandState = CommandState.VerbSelected;
        }
    }

    private async Task PerformVerbAction(string thingID)
    {
        string performedAction;

        if (CurrentCommandState == CommandStateEnum.VerbSelected)
        {
            // Interact with the object

            // var result = InkStory.EvaluateFunction("verb", thingID, currentVerbID);

            if (!InkStory.EvaluateFunction("verb", thingID, currentVerbID).AsBool())
            {
                // No scripted reaction found, use the default one
                ScriptManager.ScriptActionQueue.Add(new ScriptActionMessage(StageNode.PlayerCharacter, GameResource.DefaultVerbReactions[currentVerbID]));
            }
            await ScriptManager.RunActionQueue();

			performedAction = currentVerbID;
            CurrentCommandState = CommandStateEnum.Idle;
        }
        else
        {
            InkStory.EvaluateFunction("verb", thingID, "walk");
			InterfaceNode.SetCommandLabel(ThingManager.GetThingName(thingID));
            await ScriptManager.RunActionQueue();
			performedAction = "walk";
        }

		ThingActionCounter.IncrementActionCounter(thingID, performedAction);
    }

	private async Task MovePlayerToThing(Thing thing)
	{
		Vector2 position = Vector2.Zero;
		if (thing is Object object_)
			position = object_.Position;
		else if (thing is Character character)
			position = character.Position;
		else if (thing is HotspotArea hotspotArea)
			position = hotspotArea.GetClosestPoint(StageNode.PlayerCharacter.Position) + hotspotArea.Position;
		else
			Logger.Log($"OnAreaActivated: Area {thing.ThingResource.ID} is not an Object or a HotspotArea", Logger.LogTypeEnum.Error);

		// if (position.DistanceTo(StageNode.PlayerCharacter.Position) > 20)
		await StageNode.PlayerCharacter.MoveTo(position, 20);
	}

	public async Task StartDialog(string characterID) => await DialogManager.StartDialog(characterID);

	public override void _Process(double delta)
	{
		if (StageNode.PlayerCharacter.Position.X > StageNode.GetViewportRect().Size.X / 8)
		{
			if (Camera2DNode.Position.X + StageNode.GetViewportRect().Size.X / 4 < StageNode.GetSize().X)
				Camera2DNode.Position = new Vector2(StageNode.PlayerCharacter.Position.X - StageNode.GetViewportRect().Size.X / 8, Camera2DNode.Position.Y);
		}
		else if (StageNode.PlayerCharacter.Position.X < StageNode.GetViewportRect().Size.X / 8)
		{
			if (Camera2DNode.Position.X - StageNode.GetViewportRect().Size.X / 4 > 0)
				Camera2DNode.Position = new Vector2(StageNode.PlayerCharacter.Position.X - StageNode.GetViewportRect().Size.X / 8, Camera2DNode.Position.Y);
		}
	}
}
