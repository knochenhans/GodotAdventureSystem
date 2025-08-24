using Godot.Collections;
using Godot;
// using GodotInk;

public partial class Interface : CanvasLayer
{
	GamePanel GamePanel => GetNode<GamePanel>("%GamePanel");
	Control InterfacePanel => GetNode<Control>("%InterfacePanel");
	Control InterfaceContainer => InterfacePanel.GetNode<Control>("InterfaceContainer");
	Control InterfaceContainerDialog => InterfacePanel.GetNode<Control>("InterfaceContainerDialog");
	// Control DialogOptionsContainer => InterfaceContainerDialog.GetNode<Control>("%DialogOptionsContainer");
	GridContainer InventoryGridContainer => InterfaceContainer.GetNode<GridContainer>("%Inventory");
	GridContainer VerbGridContainer => InterfaceContainer.GetNode<GridContainer>("%Verbs");
	Label CommandLabel => InterfaceContainer.GetNode<Label>("%CommandLabel");

	[Signal] public delegate void GamePanelMouseMotionEventHandler();
	[Signal] public delegate void GamePanelMousePressedEventHandler(InputEventMouseButton mouseButtonEvent);
	// [Signal] public delegate void ThingHoveredEventHandler(string thingID);
	// [Signal] public delegate void ThingLeaveEventHandler();
	// [Signal] public delegate void ThingClickedEventHandler(string thingID, Vector2 mousePosition);
	[Signal] public delegate void VerbHoveredEventHandler(string verbID);
	[Signal] public delegate void VerbLeaveEventHandler();
	[Signal] public delegate void VerbClickedEventHandler(string verbID);
	// [Signal] public delegate void DialogOptionClickedEventHandler(InkChoice choice);

	[Export] public PackedScene VerbButtonScene;
	[Export] public PackedScene InventoryButtonScene;

	// // int Zoom = 4;

	enum MessageStateEnum
	{
		Idle,
		Keep
	}

	MessageStateEnum _messageState = MessageStateEnum.Idle;
	private MessageStateEnum MessageState
	{
		get => _messageState;
		set
		{
			if (value == MessageStateEnum.Keep)
				GetNode<Timer>("%MessageTimer").Start();
			_messageState = value;
		}
	}

	public enum ModeEnum
	{
		Normal,
		Dialog
	}

	ModeEnum _mode = ModeEnum.Normal;
	public ModeEnum Mode
	{
		get => _mode;
		set
		{
			ModeChanged(value);
			_mode = value;
		}
	}

	private void ModeChanged(ModeEnum value)
	{
		switch (value)
		{
			case ModeEnum.Normal:
				InterfaceContainer.Visible = true;
				InterfaceContainerDialog.Visible = false;
				break;
			case ModeEnum.Dialog:
				InterfaceContainer.Visible = false;
				InterfaceContainerDialog.Visible = true;
				break;
		}
	}

	public override void _Ready() => GamePanel.GuiInput += OnGamePanelInputEvent;

	public void Init(Dictionary<string, string> verbs)
	{
		var buttonGroup = new ButtonGroup();

		foreach (var verb in verbs)
		{
			var button = VerbButtonScene.Instantiate() as Button;
			button.Text = verb.Value;
			button.MouseEntered += () => EmitSignal(SignalName.VerbHovered, verb.Key);
			button.MouseExited += () => EmitSignal(SignalName.VerbLeave);
			button.Pressed += () => EmitSignal(SignalName.VerbClicked, verb.Key);
			button.SetMeta("verb", verb.Key);
			button.ButtonGroup = buttonGroup;

			VerbButtonsContainer.AddChild(button);
			VerbButtons[verb.Key] = button;
		}

		var inventoryButtonCount = 8;

		for (int i = 0; i < inventoryButtonCount; i++)
		{
			var button = InventoryButtonScene.Instantiate() as InventoryButton;
			button.MouseEntered += () => OnInventoryButtonMouseEntered(button);
			button.MouseExited += OnInventoryButtonMouseExited;
			button.Pressed += () => OnInventoryButtonPressed(button);
			InventoryGridContainer.AddChild(button);
		}
	}

	public void Reset()
	{
		// ResetFocus();
		ClearDialogChoiceLabels();
		ClearInventory();
	}

	public void ClearInventory()
	{
		foreach (var inventoryButton in InventoryGridContainer.GetChildren())
		{
			var button = inventoryButton as InventoryButton;
			// button.SetThing(null);
		}
	}

	public void OnGamePanelInputEvent(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotionEvent)
			EmitSignal(SignalName.GamePanelMouseMotion);
		else if (@event is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.Pressed)
			EmitSignal(SignalName.GamePanelMousePressed, mouseButtonEvent);
	}

	public void SetCommandLabel(string text, bool keep = false)
	{
		if (MessageState == MessageStateEnum.Idle)
		{
			CommandLabel.Text = text;
			// GD.Print($"SetCommandLabel: {text}");

			if (keep)
				MessageState = MessageStateEnum.Keep;
			// else
			// 	MessageState = MessageStateEnum.Idle;
		}
	}

	public void OnMessageTimerTimeout()
	{
		SetCommandLabel("");
		MessageState = MessageStateEnum.Idle;
	}

	public void ResetCommandLabel()
	{
		SetCommandLabel("");
		// ResetFocus();
	}

	// public void OnPlayerObjectAddedToInventory(ThingResource thingResource)
	// {
	// 	foreach (var inventoryButton in InventoryGridContainer.GetChildren())
	// 	{
	// 		var button = inventoryButton as InventoryButton;
	// 		if (button.GetMeta("thingID").AsString() == "")
	// 		{
	// 			button.SetThing(thingResource);
	// 			break;
	// 		}
	// 	}
	// }

	// public void OnPlayerObjectRemovedFromInventory(string thingID)
	// {
	// 	foreach (var inventoryButton in InventoryGridContainer.GetChildren())
	// 	{
	// 		var button = inventoryButton as InventoryButton;
	// 		if (button.GetMeta("thingID").AsString() == thingID)
	// 		{
	// 			button.SetThing(null);
	// 			break;
	// 		}
	// 	}
	// }

	public void OnInventoryButtonMouseEntered(InventoryButton inventoryButton)
	{
		var thingID = inventoryButton.GetMeta("thingID").AsString();

		if (thingID != "")
		{
			// EmitSignal(SignalName.ThingHovered, thingID);
		}
	}

	public void OnInventoryButtonMouseExited()
	{
		// EmitSignal(SignalName.ThingLeave);
	}

	public void OnInventoryButtonPressed(InventoryButton inventoryButton)
	{
		var thingID = inventoryButton.GetMeta("thingID").AsString();

		if (thingID != "")
		{
			// EmitSignal(SignalName.ThingClicked, thingID);
		}
	}

	// public void ResetFocus()
	// {
	// 	foreach (var verbButton in VerbGridContainer.GetChildren())
	// 	{
	// 		var button = verbButton as Button;
	// 		button.ReleaseFocus();
	// 	}
	// }

	// public void SetDialogChoiceLabels(Array<InkChoice> choices)
	// {
	// 	foreach (var choice in choices)
	// 	{
	// 		var choiceLabel = ResourceLoader.Load<PackedScene>("res://addons/GodotAdventureSystem/DialogOptionLabel.tscn").Instantiate() as DialogOptionLabel;
	// 		choiceLabel.Text = choice.Text;
	// 		// choiceLabel.MouseEntered += () => EmitSignal(SignalName.VerbHovered, choice.Text);
	// 		// choiceLabel.MouseExited += () => EmitSignal(SignalName.VerbLeave);
	// 		choiceLabel.GuiInput += (InputEvent @event) =>
	// 		{
	// 			if (@event is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.Pressed)
	// 				EmitSignal(SignalName.DialogOptionClicked, choice);
	// 		};
	// 		choiceLabel.SetMeta("choice", choice);

	// 		DialogOptionsContainer.AddChild(choiceLabel);
	// 	}
	// }

	public void ClearDialogChoiceLabels()
	{
		// foreach (var child in DialogOptionsContainer.GetChildren())
		// 	child.QueueFree();
	}

	public void UnpressVerbButton(string verbID)
	{
		if (VerbButtons.TryGetValue(verbID, out var button))
			button.ButtonPressed = false;
	}
}