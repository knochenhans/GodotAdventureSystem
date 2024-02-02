using System;
using Godot;
using Godot.Collections;
using GodotInk;

public partial class Game : Scene
{
	enum CommandState
	{
		Idle,
		VerbSelected,
	}

	public VariableManager VariableManager { get; set; } = new();
	public InventoryManager InventoryManager { get; set; } = new();
	public ThingManager ThingManager { get; set; } = new();

	public Dictionary<string, string> Verbs { get; set; }
	public Interface InterfaceNode { get; set; }
	public Stage StageNode { get; set; }
	public string currentVerbID { get; set; }
	public Camera2D Camera2DNode { get; set; }

	public Dictionary<string, string> Things { get; set; } = new();

	PackedScene IngameMenuScene { get; set; }

	CommandState _currentCommandState = CommandState.Idle;
	private CommandState CurrentCommandState
	{
		get => _currentCommandState;
		set
		{
			if (value == CommandState.Idle)
			{
				InterfaceNode.ResetCommandLabel();
				currentVerbID = "";
			}
			_currentCommandState = value;
		}
	}

	[Export]
	InkStory InkStory { get; set; }

	// Ink external functions
	Action<string> DisplayMessage;
	Action<string> PrintError;
	Action<string> PickUp;
	Func<string, Variant> IsInInventory;

	public Game()
	{
		DisplayMessage = (parameter) =>
		{
			GD.Print($"Calling external Ink function: Display: {parameter}");
			StageNode.PlayerCharacter.Talk(new Array<string> { parameter });
		};
		PrintError = (parameter) =>
		{
			GD.PrintErr($"Calling external Ink function: Error: {parameter}");
		};
		PickUp = (parameter) =>
		{
			GD.Print($"Calling external Ink function: PickUp: {parameter}");
			StageNode.PlayerCharacter.PickUp(StageNode.FindObject(parameter));
			ThingManager.RemoveThing(parameter);
		};
		IsInInventory = (parameter) =>
		{
			GD.Print($"Calling external Ink function: IsInInventory: {parameter}");
			return InventoryManager.HasObject(parameter);
		};
	}

	public override void _Ready()
	{
		base._Ready();

		var cursor = ResourceLoader.Load("res://Resources/Cursor_64.png");
		Input.SetCustomMouseCursor(cursor, Input.CursorShape.Arrow, new Vector2(29, 29));

		Verbs = new Dictionary<string, string>
		{
			{ "give", "Give" },
			{ "pick_up", "Pick up" },
			{ "use", "Use" },
			{ "open", "Open" },
			{ "look", "Look" },
			{ "push", "Push" },
			{ "close", "Close" },
			{ "talk_to", "Talk to" },
			{ "pull", "Pull" }
		};

		InterfaceNode = GetNode<Interface>("Interface");
		InterfaceNode.Init(Verbs);
		InterfaceNode.InventoryManager = InventoryManager;

		InterfaceNode.GamePanelMouseMotion += _OnGamePanelMouseMotion;
		InterfaceNode.GamePanelMousePressed += _OnGamePanelMousePressed;

		InterfaceNode.ThingClicked += _OnThingClicked;
		InterfaceNode.ThingHovered += _OnThingHovered;

		InterfaceNode.VerbClicked += _OnVerbClicked;
		InterfaceNode.VerbHovered += _OnVerbHovered;
		InterfaceNode.VerbLeave += _OnVerbLeave;

		InventoryManager.ObjectAdded += InterfaceNode._OnObjectAddedToInventory;

		StageNode = GetNode<Stage>("Stage");

		StageNode.ThingClicked += _OnThingClicked;
		StageNode.ThingHovered += _OnThingHovered;

		StageNode.PlayerCharacter.InventoryManager = InventoryManager;

		ThingManager.AddThings(StageNode.CollectThings());

		Camera2DNode = GetNode<Camera2D>("Camera2D");

		MessageDataManager.LoadMessages("res://messages.json");

		IngameMenuScene = ResourceLoader.Load<PackedScene>("res://AdventureSystem/IngameMenu.tscn");

		InkStory.BindExternalFunction("display_message", DisplayMessage, true);
		InkStory.BindExternalFunction("print_error", PrintError, true);
		InkStory.BindExternalFunction("pick_up", PickUp, true);
		InkStory.BindExternalFunction("is_in_inventory", IsInInventory, true);
		InkStory.Continue();
		// InkStory.EvaluateFunction("verb", "parameter");
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

	public void _OnVerbHovered(string verbID)
	{
		if (CurrentCommandState == CommandState.Idle)
			InterfaceNode.SetCommandLabel(Verbs[verbID]);
	}

	public void _OnVerbLeave()
	{
		if (CurrentCommandState == CommandState.Idle)
			InterfaceNode.ResetCommandLabel();
	}

	public void _OnVerbClicked(string verbID)
	{
		// GD.Print($"_OnVerbActivated: Verb: {verbID} activated");

		InterfaceNode.SetCommandLabel(Verbs[verbID]);
		currentVerbID = verbID;
		CurrentCommandState = CommandState.VerbSelected;
	}

	public void _OnGamePanelMouseMotion()
	{
		if (CurrentCommandState == CommandState.Idle)
			InterfaceNode.ResetCommandLabel();
		else if (CurrentCommandState == CommandState.VerbSelected)
			InterfaceNode.SetCommandLabel(Verbs[currentVerbID]);
	}

	public void _OnGamePanelMousePressed(InputEventMouseButton mouseButtonEvent)
	{
		// GD.Print($"_OnGamePanelMousePressed: Mouse button pressed: {mouseButtonEvent.ButtonIndex} at {mouseButtonEvent.Position}");

		if (CurrentCommandState == CommandState.Idle)
		{
			StageNode.PlayerCharacter.MoveTo(mouseButtonEvent.Position / Camera2DNode.Zoom + Camera2DNode.Position);
		}
		else if (CurrentCommandState == CommandState.VerbSelected)
		{
			if (mouseButtonEvent.ButtonIndex == MouseButton.Right)
			{
				CurrentCommandState = CommandState.Idle;
				InterfaceNode.ResetCommandLabel();
			}
		}
	}

	public void _OnThingHovered(string thingID)
	{
		var thing = ThingManager.GetThing(thingID);

		if (thing != null)
		{
			if (CurrentCommandState == CommandState.Idle)
				InterfaceNode.SetCommandLabel(MessageDataManager.GetMessages(thingID, "name"));
			else if (CurrentCommandState == CommandState.VerbSelected)
				InterfaceNode.SetCommandLabel($"{Verbs[currentVerbID]} {MessageDataManager.GetMessages(thingID, "name")}");
		}
	}

	public async void _OnThingClicked(string thingID)
	{
		var thing = ThingManager.GetThing(thingID);

		if (thing != null)
		{
			// For objects (that are not in the inventory) and hotspots, move the player character to the object
			Vector2 position = Vector2.Zero;
			if (thing is Object object_)
				position = object_.Position;
			else if (thing is HotspotArea hotspotArea)
				position = hotspotArea.CalculateCenter();
			else
				GD.PrintErr($"_OnAreaActivated: Area {thing.ID} is not an Object or a HotspotArea");

			StageNode.PlayerCharacter.MoveTo(position);

			await ToSignal(StageNode.PlayerCharacter, "CharacterMoved");
		}

		if (CurrentCommandState == CommandState.VerbSelected)
		{
			// Interact with the object
			if (!InkStory.EvaluateFunction("verb", thingID, currentVerbID).AsBool())
				StageNode.PlayerCharacter.Talk(new Array<string> { MessageDataManager.GetMessages("default", currentVerbID) });

			CurrentCommandState = CommandState.Idle;
			return;
		}

		// GD.Print($"_OnObjectActivated: Object: {thing.DisplayedName} activated");

		InterfaceNode.SetCommandLabel(MessageDataManager.GetMessages(thingID, "name"));
		// CurrentCommandState = CommandState.VerbSelected;
	}

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