using Godot.Collections;
using Godot;
using GodotInk;

public partial class Interface : CanvasLayer
{
	GridContainer VerbGridContainer { get; set; }
	GridContainer InventoryGridContainer { get; set; }
	Label CommandLabel { get; set; }
	GamePanel GamePanel { get; set; }
	Control InterfacePanel { get; set; }
	Control InterfaceContainer { get; set; }
	Control InterfaceContainerDialog { get; set; }

	[Signal]
	public delegate void GamePanelMouseMotionEventHandler();

	[Signal]
	public delegate void GamePanelMousePressedEventHandler(InputEventMouseButton mouseButtonEvent);

	[Signal]
	public delegate void ThingHoveredEventHandler(string thingID);

	[Signal]
	public delegate void ThingLeaveEventHandler();

	[Signal]
	public delegate void ThingClickedEventHandler(string thingID);

	[Signal]
	public delegate void VerbHoveredEventHandler(string verbID);

	[Signal]
	public delegate void VerbLeaveEventHandler();

	[Signal]
	public delegate void VerbClickedEventHandler(string verbID);

	[Signal]
	public delegate void DialogOptionClickedEventHandler(InkChoice choice);

	int Zoom = 4;

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

			_mode = value;
		}
	}

	public override void _Ready()
	{
		GamePanel = GetNode<GamePanel>("%GamePanel");
		GamePanel.GuiInput += _OnGamePanelInputEvent;

		InterfacePanel = GetNode<Control>("%InterfacePanel");

		InterfaceContainer = InterfacePanel.GetNode<Control>("InterfaceContainer");
		InterfaceContainerDialog = InterfacePanel.GetNode<Control>("InterfaceContainerDialog");
		VerbGridContainer = InterfaceContainer.GetNode<GridContainer>("%Verbs");
		InventoryGridContainer = InterfaceContainer.GetNode<GridContainer>("%Inventory");
		CommandLabel = InterfaceContainer.GetNode<Label>("%CommandLabel");
	}

	public void Init(Dictionary<string, string> verbs)
	{
		foreach (var verb in verbs)
		{
			var button = ResourceLoader.Load<PackedScene>("res://AdventureSystem/VerbButton.tscn").Instantiate() as Button;
			button.Text = verb.Value;
			button.MouseEntered += () => EmitSignal(SignalName.VerbHovered, verb.Key);
			button.MouseExited += () => EmitSignal(SignalName.VerbLeave);
			button.Pressed += () => EmitSignal(SignalName.VerbClicked, verb.Key);
			button.SetMeta("verb", verb.Key);

			VerbGridContainer.AddChild(button);
		}

		var inventoryButtonCount = 8;

		for (int i = 0; i < inventoryButtonCount; i++)
		{
			var button = ResourceLoader.Load<PackedScene>("res://AdventureSystem/InventoryButton.tscn").Instantiate() as InventoryButton;
			button.MouseEntered += () => _OnInventoryButtonMouseEntered(button);
			button.MouseExited += _OnInventoryButtonMouseExited;
			button.Pressed += () => _OnInventoryButtonPressed(button);
			InventoryGridContainer.AddChild(button);
		}
	}

	public void _OnGamePanelInputEvent(InputEvent @event)
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

	public void _OnMessageTimerTimeout()
	{
		SetCommandLabel("");
		MessageState = MessageStateEnum.Idle;
	}

	public void ResetCommandLabel()
	{
		SetCommandLabel("");
		ResetFocus();
	}

	public void _OnObjectAddedToInventory(string thingID, Texture2D texture)
	{
		foreach (var inventoryButton in InventoryGridContainer.GetChildren())
		{
			var button = inventoryButton as InventoryButton;
			if (button.GetMeta("thingID").AsString() == "")
			{
				button.SetThing(thingID, texture);
				break;
			}
		}
	}

	public void _OnInventoryButtonMouseEntered(InventoryButton inventoryButton)
	{
		var thingID = inventoryButton.GetMeta("thingID").AsString();

		if (thingID != "")
		{
			// EmitSignal(SignalName.ThingClicked, thingID);
			EmitSignal(SignalName.ThingHovered, thingID);
		}
	}

	public void _OnInventoryButtonMouseExited()
	{
		// ResetCommandLabel();
		EmitSignal(SignalName.ThingLeave);
	}

	public void _OnInventoryButtonPressed(InventoryButton inventoryButton)
	{
		var thingID = inventoryButton.GetMeta("thingID").AsString();

		if (thingID != "")
		{
			EmitSignal(SignalName.ThingClicked, thingID);
		}
	}

	public void ResetFocus()
	{
		foreach (var verbButton in VerbGridContainer.GetChildren())
		{
			var button = verbButton as Button;
			button.ReleaseFocus();
		}
	}

	public void SetDialogChoiceLabels(Array<InkChoice> choices)
	{
		var dialogOptionsContainer = InterfaceContainerDialog.GetNode<Control>("%DialogOptionsContainer");

		foreach (var choice in choices)
		{
			var choiceLabel = ResourceLoader.Load<PackedScene>("res://AdventureSystem/DialogOptionLabel.tscn").Instantiate() as DialogOptionLabel;
			choiceLabel.Text = choice.Text;
			// choiceLabel.MouseEntered += () => EmitSignal(SignalName.VerbHovered, choice.Text);
			// choiceLabel.MouseExited += () => EmitSignal(SignalName.VerbLeave);
			choiceLabel.GuiInput += (InputEvent @event) =>
			{
				if (@event is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.Pressed)
					EmitSignal(SignalName.DialogOptionClicked, choice);
			};
			choiceLabel.SetMeta("choice", choice);

			dialogOptionsContainer.AddChild(choiceLabel);
		}
	}
}
