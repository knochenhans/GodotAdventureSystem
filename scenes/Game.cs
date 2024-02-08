using System;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using GodotInk;

public partial class ScriptAction : GodotObject
{
	public Character Character { get; set; }

	public ScriptAction(Character character) { Character = character; }
	public virtual Task Execute() { return Task.CompletedTask; }
}

public partial class ScriptActionMessage : ScriptAction
{
	public string Message { get; set; }

	public ScriptActionMessage(Character character, string message) : base(character) { Message = message; }
	public override async Task Execute()
	{
		await Character.SpeechBubble(Message);
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

	public ScriptActionWait(Character character, float seconds) : base(character) { Seconds = seconds; }
	public override Task Execute()
	{
		GD.Print($"Waiting for {Seconds} seconds");
		return Task.Delay(TimeSpan.FromSeconds(Seconds));
	}
}

public partial class ScriptActionPlayAnimation : ScriptAction
{
	public string AnimationName { get; set; }

	public ScriptActionPlayAnimation(Character character, string animationID) : base(character) { AnimationName = animationID; }
	public override async Task Execute()
	{
		await Character.PlayAnimation(AnimationName, false);
	}
}

public partial class ScriptActionStartDialog : ScriptAction
{
	public string KnotName { get; set; }
	public Game Game { get; set; }

	public ScriptActionStartDialog(PlayerCharacter character, Game game, string knotName) : base(character) { KnotName = knotName; Game = game; }
	public override Task Execute()
	{
		Game.StartDialog(KnotName);
		return Task.CompletedTask;
	}
}

public partial class Game : Scene
{
	[Signal]
	public delegate void DialogFinishedEventHandler();

	enum CommandState
	{
		Idle,
		VerbSelected,
		Dialog
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

	public async Task RunActionQueue()
	{
		// Special case for dialog
		if (ActionQueue.Count == 1)
		{
			if (ActionQueue[0] is ScriptActionStartDialog)
			{
				var action = ActionQueue[0] as ScriptActionStartDialog;
				ActionQueue.Clear();
				await action.Execute();
				return;
			}
		}

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
	Func<string, Variant> InkIsInInventory;
	Action<string, bool> InkSetVariable;
	Func<string, Variant> InkGetVariable;
	Action<string, string> InkSetThingName;
	Action<float> InkWait;
	Action<int, int> InkMoveTo;
	Action<int, int> InkMoveRelative;
	Action<string> InkPlayPlayerAnimation;
	Action<string, string> InkPlayCharacterAnimation;
	Action<string> InkStartDialog;

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
		InkIsInInventory = (thingID) => ThingManager.IsInInventory(thingID);
		InkSetVariable = (variableID, value) => VariableManager.SetVariable(variableID, value);
		InkGetVariable = (variableID) => VariableManager.GetVariable(variableID);
		InkSetThingName = (thingID, name) => ThingManager.UpdateThingName(thingID, name);
		InkWait = (seconds) => ActionQueue.Add(new ScriptActionWait(StageNode.PlayerCharacter, seconds));
		InkMoveTo = (posX, posY) => ActionQueue.Add(new ScriptActionMove(StageNode.PlayerCharacter, new Vector2(posX, posY)));
		InkMoveRelative = (posX, posY) => ActionQueue.Add(new ScriptActionMove(StageNode.PlayerCharacter, new Vector2(posX, posY), true));
		InkPlayPlayerAnimation = (animationID) => ActionQueue.Add(new ScriptActionPlayAnimation(StageNode.PlayerCharacter, animationID));
		InkPlayCharacterAnimation = (string characterID, string animationID) =>
		{
			var character = ThingManager.GetThing(characterID);

			if (character is Character)
				ActionQueue.Add(new ScriptActionPlayAnimation(character as Character, animationID));
			else
				GD.PrintErr($"InkPlayCharacterAnimation: Thing {characterID} is not a Character");
		};
		InkStartDialog = (characterID) => ActionQueue.Add(new ScriptActionStartDialog(StageNode.PlayerCharacter, this, characterID));
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
		// playerCharacter.CharacterStartDialog += _OnDialogStarted();

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
		InkStory.BindExternalFunction("is_in_inventory", InkIsInInventory);
		InkStory.BindExternalFunction("set_var", InkSetVariable);
		InkStory.BindExternalFunction("get_var", InkGetVariable);
		InkStory.BindExternalFunction("set_name", InkSetThingName);
		InkStory.BindExternalFunction("wait", InkWait);
		InkStory.BindExternalFunction("move_to", InkMoveTo);
		InkStory.BindExternalFunction("move_rel", InkMoveRelative);
		InkStory.BindExternalFunction("play_anim", InkPlayPlayerAnimation);
		InkStory.BindExternalFunction("play_anim_char", InkPlayCharacterAnimation);
		InkStory.BindExternalFunction("dialog", InkStartDialog);
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
			await RunActionQueue();

			CurrentCommandState = CommandState.Idle;
			return;
		}

		// GD.Print($"_OnObjectActivated: Object: {thing.DisplayedName} activated");

		InterfaceNode.SetCommandLabel(ThingManager.GetThingName(thingID));
		// CurrentCommandState = CommandState.VerbSelected;
	}

	public void StartDialog(string characterID)
	{
		GD.Print($"StartDialog: {characterID}");

		InterfaceNode.Mode = Interface.ModeEnum.Dialog;
		var character = ThingManager.GetThing(characterID) as Character;

		StageNode.PlayerCharacter.LookTo(character.Position);
		StageNode.PlayerCharacter.StartDialog();

		character.LookTo(StageNode.PlayerCharacter.Position);
		character.StartDialog();

		InkStory.ChoosePathString(characterID);
		InkStory.Continued += _OnDialogContinue;
		// InkStory.MadeChoice += _OnDialogChoiceMade;
		InterfaceNode.DialogOptionClicked += _OnDialogChoiceMade;
		InkStory.Continue();
	}

	public async void _OnDialogContinue()
	{
		if (InkStory.CurrentText.StripEdges() != "")
		{
			// Only check first tag for now
			var tag = InkStory.GetCurrentTags();

			Character actingCharacter = StageNode.PlayerCharacter;

			if (tag.Count > 0)
				if (tag[0] != "player")
					actingCharacter = ThingManager.GetThing(tag[0]) as Character;

			ActionQueue.Add(new ScriptActionMessage(actingCharacter, InkStory.CurrentText));
			ActionQueue.Add(new ScriptActionWait(actingCharacter, 0.3f));

			await RunActionQueue();
		}

		if (InkStory.CanContinue)
			InkStory.Continue();
		else
		{
			if (InkStory.CurrentChoices.Count > 0)
				InterfaceNode.SetDialogChoiceLabels(new Array<InkChoice>(InkStory.CurrentChoices.ToArray()));
			else
				{
					// Story has finished
					InterfaceNode.Mode = Interface.ModeEnum.Normal;
					// InkStory.ResetState();
					// InkStory.ResetCallstack();
				}
		}
		// CurrentCommandState = CommandState.Idle;
	}

	public async void _OnDialogChoiceMade(InkChoice choice)
	{
		InterfaceNode.ClearDialogChoiceLabels();
		ActionQueue.Add(new ScriptActionMessage(StageNode.PlayerCharacter, choice.Text));
		ActionQueue.Add(new ScriptActionWait(StageNode.PlayerCharacter, 0.5f));

		await RunActionQueue();

		InkStory.ChooseChoiceIndex(choice.Index);
		InkStory.Continue();
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