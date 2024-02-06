using System;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using GodotInk;

public partial class ScriptAction : GodotObject
{
	public PlayerCharacter Character { get; set; }

	public ScriptAction(PlayerCharacter character) { Character = character; }
	public virtual Task Execute() { return Task.CompletedTask; }
}

public partial class ScriptActionMessage : ScriptAction
{
	public string Message { get; set; }

	public ScriptActionMessage(PlayerCharacter character, string message) : base(character) { Message = message; }
	public override async Task Execute()
	{
		await Character.Talk(Message);
	}
}

public partial class ScriptActionMove : ScriptAction
{
	public Vector2 Position { get; set; }
	public bool IsRelative { get; set; }

	public ScriptActionMove(PlayerCharacter character, Vector2 position, bool isRelative = false) : base(character) { Position = position; IsRelative = isRelative; }
	public override async Task Execute()
	{
		await Character.MoveTo(Position, 0, IsRelative);
	}
}

public partial class ScriptActionWait : ScriptAction
{
	public float Seconds { get; set; }

	public ScriptActionWait(PlayerCharacter character, float seconds) : base(character) { Seconds = seconds; }
	public override Task Execute()
	{
		return Task.Delay(TimeSpan.FromSeconds(Seconds));
	}
}

public partial class ScriptActionPlayAnimation : ScriptAction
{
	public string AnimationName { get; set; }

	public ScriptActionPlayAnimation(PlayerCharacter character, string animationID) : base(character) { AnimationName = animationID; }
	public override async Task Execute()
	{
		await Character.PlayAnimation(AnimationName);
	}
}

public partial class Game : Scene
{
	enum CommandState
	{
		Idle,
		VerbSelected,
	}

	public VariableManager VariableManager { get; set; } = new();
	public ThingManager ThingManager { get; set; } = new();

	public Dictionary<string, string> Verbs { get; set; }
	public Dictionary<string, string> DefaultVerbReactions { get; set; }

	public Interface InterfaceNode { get; set; }
	public Stage StageNode { get; set; }
	public string currentVerbID { get; set; }
	public Camera2D Camera2DNode { get; set; }

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

	[Export]
	PackedScene PlayerCharacterScene { get; set; }

	public Array<ScriptAction> ActionQueue { get; set; } = new Array<ScriptAction>();

	public async void RunActionQueue()
	{
		foreach (var action in ActionQueue)
		{
			GD.Print($"Running action: {action.GetType()}");
			await action.Execute();
		}
		ActionQueue.Clear();
	}

	// Ink external functions
	Action<string> DisplayBubble;
	Action<string> PrintError;
	Action<string> PickUp;
	Action<string> CreateObject;
	Func<string, Variant> IsInInventory;
	Action<string, bool> SetVariable;
	Func<string, Variant> GetVariable;
	Action<string, string> SetThingName;
	Action<float> Wait;
	Action<int, int> MoveTo;
	Action<int, int> MoveRelative;
	Action<string> PlayAnimation;

	public Game()
	{
		DisplayBubble = (message) => ActionQueue.Add(new ScriptActionMessage(StageNode.PlayerCharacter, message));
		PrintError = (message) => GD.PrintErr(message);
		PickUp = (objectID) =>
		{
			ThingManager.MoveThingToInventory(objectID);
			StageNode.PlayerCharacter.PickUpObject();
		};
		CreateObject = (objectID) =>
		{
			ThingManager.LoadThingToInventory(objectID);
			StageNode.PlayerCharacter.PickUpObject();
		};
		IsInInventory = (thingID) => ThingManager.IsInInventory(thingID);
		SetVariable = (variableID, value) => VariableManager.SetVariable(variableID, value);
		GetVariable = (variableID) => VariableManager.GetVariable(variableID);
		SetThingName = (thingID, name) => ThingManager.UpdateThingName(thingID, name);
		Wait = (seconds) => ActionQueue.Add(new ScriptActionWait(StageNode.PlayerCharacter, seconds));
		MoveTo = (posX, posY) => ActionQueue.Add(new ScriptActionMove(StageNode.PlayerCharacter, new Vector2(posX, posY)));
		MoveRelative = (posX, posY) => ActionQueue.Add(new ScriptActionMove(StageNode.PlayerCharacter, new Vector2(posX, posY), true));
		PlayAnimation = (animationID) => ActionQueue.Add(new ScriptActionPlayAnimation(StageNode.PlayerCharacter, animationID));
	}

	public override void _Ready()
	{
		base._Ready();

		var cursor = ResourceLoader.Load("res://resources/cursor_64.png");
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

		DefaultVerbReactions = new Dictionary<string, string>()
		{
			{ "give", "There’s no one to give anything to." },
			{ "pick_up", "I can’t pick that up." },
			{ "use", "I can’t use that." },
			{ "open", "I can’t open that." },
			{ "look", "I see nothing special." },
			{ "push", "I can’t push that." },
			{ "close", "I can’t close that." },
			{ "talk_to", "There’s no one to talk to." },
			{ "pull", "I can’t pull that." }
		};

		InterfaceNode = GetNode<Interface>("Interface");
		InterfaceNode.Init(Verbs);

		InterfaceNode.GamePanelMouseMotion += _OnGamePanelMouseMotion;
		InterfaceNode.GamePanelMousePressed += _OnGamePanelMousePressed;

		InterfaceNode.ThingClicked += _OnThingClicked;
		InterfaceNode.ThingHovered += _OnThingHovered;

		InterfaceNode.VerbClicked += _OnVerbClicked;
		InterfaceNode.VerbHovered += _OnVerbHovered;
		InterfaceNode.VerbLeave += _OnVerbLeave;

		var playerCharacter = PlayerCharacterScene.Instantiate() as PlayerCharacter;

		StageNode = GetNode<Stage>("Stage");

		StageNode.ThingClicked += _OnThingClicked;
		StageNode.ThingHovered += _OnThingHovered;

		StageNode.InitPlayerCharacter(playerCharacter);

		ThingManager.RegisterThings(StageNode.CollectThings());
		ThingManager.AddThingToIventory += InterfaceNode._OnObjectAddedToInventory;

		Camera2DNode = GetNode<Camera2D>("Camera2D");

		IngameMenuScene = ResourceLoader.Load<PackedScene>("res://AdventureSystem/IngameMenu.tscn");

		// Bind Ink functions
		InkStory.BindExternalFunction("bubble", DisplayBubble);
		InkStory.BindExternalFunction("print_error", PrintError);
		InkStory.BindExternalFunction("pick_up", PickUp);
		InkStory.BindExternalFunction("create", CreateObject);
		InkStory.BindExternalFunction("is_in_inventory", IsInInventory);
		InkStory.BindExternalFunction("set_var", SetVariable);
		InkStory.BindExternalFunction("get_var", GetVariable);
		InkStory.BindExternalFunction("set_name", SetThingName);
		InkStory.BindExternalFunction("wait", Wait);
		InkStory.BindExternalFunction("move_to", MoveTo);
		InkStory.BindExternalFunction("move_rel", MoveRelative);
		InkStory.BindExternalFunction("play_anim", PlayAnimation);

		// InkStory.Continue();
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

	public async void _OnGamePanelMousePressed(InputEventMouseButton mouseButtonEvent)
	{
		if (CurrentCommandState == CommandState.Idle)
		{
			await StageNode.PlayerCharacter.MoveTo(mouseButtonEvent.Position / Camera2DNode.Zoom + Camera2DNode.Position);
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

		if (thing == null)
		{ 
			GD.PrintErr($"_OnThingHovered: Thing {thingID} not registered in ThingManager");
		}
		else
		{
			// Hovered inventory item
			if (CurrentCommandState == CommandState.Idle)
				InterfaceNode.SetCommandLabel(ThingManager.GetThingName(thingID));
			else if (CurrentCommandState == CommandState.VerbSelected)
				InterfaceNode.SetCommandLabel($"{Verbs[currentVerbID]} {ThingManager.GetThingName(thingID)}");
		}
	}

	public async void _OnThingClicked(string thingID)
	{
		var thing = ThingManager.GetThing(thingID);

		if (thing != null)
		{
			// For objects (that are not in the inventory) and hotspots, move the player character to the object
			if (!ThingManager.IsInInventory(thingID))
			{
				Vector2 position = Vector2.Zero;
				if (thing is Object object_)
					position = object_.Position;
				else if (thing is Character character)
					position = character.Position;
				else if (thing is HotspotArea hotspotArea)
					position = hotspotArea.CalculateCenter();
				else
					GD.PrintErr($"_OnAreaActivated: Area {thing.ID} is not an Object or a HotspotArea");

				await StageNode.PlayerCharacter.MoveTo(position, 50);

				// await ToSignal(StageNode.PlayerCharacter, "CharacterMoved");
			}
		}

		if (CurrentCommandState == CommandState.VerbSelected)
		{
			// Interact with the object
			if (!InkStory.EvaluateFunction("verb", thingID, currentVerbID).AsBool())
			{
				// No scripted reaction found, use the default one
				ActionQueue.Add(new ScriptActionMessage(StageNode.PlayerCharacter, DefaultVerbReactions[currentVerbID]));
			}
			RunActionQueue();

			CurrentCommandState = CommandState.Idle;
			return;
		}

		// GD.Print($"_OnObjectActivated: Object: {thing.DisplayedName} activated");

		InterfaceNode.SetCommandLabel(ThingManager.GetThingName(thingID));
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