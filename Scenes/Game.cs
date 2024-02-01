using Godot;
using Godot.Collections;
using System;

public partial class Game : Scene
{
	enum CommandState
	{
		Idle,
		VerbSelected,
	}

	public VariableManager VariableManager { get; set; } = new();
	public InventoryManager InventoryManager { get; set; } = new();

	public Verb[] Verbs { get; set; }
	public Interface InterfaceNode { get; set; }
	public Stage StageNode { get; set; }
	public Verb currentVerb { get; set; }
	public Camera2D Camera2DNode { get; set; }

	public Dictionary<string, string> Things { get; set; } = new();

	PackedScene IngameMenuScene { get; set; }

	CommandState _currentCommandState = CommandState.Idle;
	private CommandState CurrentCommandState
	{
		get => _currentCommandState;
		set => _currentCommandState = value;
	}

	public override void _Ready()
	{
		base._Ready();

		var cursor = ResourceLoader.Load("res://Resources/Cursor_64.png");
		Input.SetCustomMouseCursor(cursor, Input.CursorShape.Arrow, new Vector2(29, 29));

		Verbs = new Verb[]
		{
			new("give", "Give"),
			new("pick_up", "Pick up"),
			new("use", "Use"),
			new("open", "Open"),
			new("look", "Look "),
			new("push", "Push"),
			new("close", "Close"),
			new("talk_to", "Talk to"),
			new("pull", "Pull"),
		};

		foreach (var verb in Verbs)
			verb.VerbActivated += _OnVerbActivated;

		InterfaceNode = GetNode<Interface>("Interface");
		InterfaceNode.Init(Verbs);
		InterfaceNode.GamePanelMouseMotion += _OnGamePanelMouseMotion;
		InterfaceNode.GamePanelMousePressed += _OnGamePanelMousePressed;

		InventoryManager.ObjectAdded += InterfaceNode._OnObjectAddedToInventory;

		StageNode = GetNode<Stage>("Stage");
		StageNode.SetCommandLabel += _OnInterfaceSetCommandLabel;
		StageNode.ActivateThing += _OnThingActivated;
		StageNode.PlayerCharacter.InventoryManager = InventoryManager;

		Camera2DNode = GetNode<Camera2D>("Camera2D");

		MessageDataManager.LoadMessages("res://messages.json");

		IngameMenuScene = ResourceLoader.Load<PackedScene>("res://AdventureSystem/IngameMenu.tscn");
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

	public void _OnVerbActivated(Verb verb)
	{
		GD.Print($"_OnVerbActivated: Verb: {verb.Name} activated");

		InterfaceNode.SetCommandLabel(verb.Name);
		currentVerb = verb;
		CurrentCommandState = CommandState.VerbSelected;
	}

	public void _OnGamePanelMouseMotion()
	{
		if (CurrentCommandState == CommandState.Idle)
			InterfaceNode.ResetCommandLabel();
		else if (CurrentCommandState == CommandState.VerbSelected)
			InterfaceNode.SetCommandLabel($"{currentVerb.Name}");
	}

	public void _OnGamePanelMousePressed(InputEventMouseButton mouseButtonEvent)
	{
		GD.Print($"_OnGamePanelMousePressed: Mouse button pressed: {mouseButtonEvent.ButtonIndex} at {mouseButtonEvent.Position}");

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

	public async void _OnThingActivated(Thing thing)
	{
		Vector2 position = Vector2.Zero;
		if (thing is Object object_)
			position = object_.Position;
		else if (thing is HotspotArea hotspotArea)
			position = hotspotArea.CalculateCenter();
		else
			GD.PrintErr($"_OnAreaActivated: Area {thing.ID} is not an Object or a HotspotArea");

		StageNode.PlayerCharacter.MoveTo(position);

		await ToSignal(StageNode.PlayerCharacter, "CharacterMoved");

		var message = MessageDataManager.GetMessages("default", currentVerb.ID);
		if (CurrentCommandState == CommandState.VerbSelected)
		{
			GD.Print($"_OnObjectActivated: Verb: {currentVerb.Name}, Object: {MessageDataManager.GetMessages(thing.ID, "name")[0]}");
			var thingMessages = MessageDataManager.GetMessages(thing.ID, currentVerb.ID);

			if (thingMessages.Count == 0)
				thingMessages = MessageDataManager.GetMessages(thing.ID, "default");

			for (int i = 0; i < thingMessages.Count; i++)
			{
				if (!ParseMessage(thing, thingMessages[i]))
					StageNode.PlayerCharacter.Talk(new Array<string> { thingMessages[i] });
			}

			CurrentCommandState = CommandState.Idle;
			return;
		}

		GD.Print($"_OnObjectActivated: Object: {thing.DisplayedName} activated");

		InterfaceNode.SetCommandLabel(thing.DisplayedName);
		CurrentCommandState = CommandState.VerbSelected;
	}

	private bool ParseMessage(Thing thing, string message)
	{
		message = message.StripEdges();

		if (message.StartsWith("$"))
		{
			switch (message)
			{
				case "$pick_up":
					StageNode.PlayerCharacter.PickUp(thing as Object);
					break;
			}
			return true;
		}
		return false;
	}

	public void _OnInterfaceSetCommandLabel(string commandLabel)
	{
		if (commandLabel != "")
			GD.Print($"_OnInterfaceSetCommandLabel: Command label: {commandLabel}");

		if (CurrentCommandState == CommandState.Idle)
			InterfaceNode.SetCommandLabel(commandLabel);
		else if (CurrentCommandState == CommandState.VerbSelected)
			InterfaceNode.SetCommandLabel($"{currentVerb.Name} {commandLabel}");
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