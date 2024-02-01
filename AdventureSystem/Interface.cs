using Godot;

public partial class Interface : CanvasLayer
{
	GridContainer VerbGridContainer { get; set; }
	GridContainer InventoryGridContainer { get; set; }
	// InventorySlot[] InventorySlots { get; set; }
	Label CommandLabel { get; set; }
	GamePanel GamePanel { get; set; }

	[Signal]
	public delegate void GamePanelMouseMotionEventHandler();

	[Signal]
	public delegate void GamePanelMousePressedEventHandler(InputEventMouseButton mouseButtonEvent);

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

	public override void _Ready()
	{
		VerbGridContainer = GetNode<GridContainer>("%Verbs");
		InventoryGridContainer = GetNode<GridContainer>("%Inventory");
		// InventorySlots = new InventorySlot[8];
		CommandLabel = GetNode<Label>("%CommandLabel");
		GamePanel = GetNode<GamePanel>("%GamePanel");
		GamePanel.GuiInput += _OnGamePanelInputEvent;
	}

	public void Init(Verb[] verbs)
	{
		foreach (var verb in verbs)
		{
			var button = ResourceLoader.Load<PackedScene>("res://AdventureSystem/VerbButton.tscn").Instantiate() as Button;
			button.Text = verb.Name;
			button.Pressed += () => verb._OnButtonPressed();
			button.SetMeta("verb", verb);

			VerbGridContainer.AddChild(button);
		}

		var inventoryButtonCount = 8;

		for (int i = 0; i < inventoryButtonCount; i++)
		{
			var button = ResourceLoader.Load<PackedScene>("res://AdventureSystem/InventoryButton.tscn").Instantiate() as InventoryButton;
			// var textureRect = button.GetNode<TextureRect>("%TextureRect");
			// var texture = ResourceLoader.Load<Texture2D>("res://Resources/Item.png");
			// var image = texture.GetImage();
			// image.Resize(image.GetWidth() * Zoom, image.GetHeight() * Zoom, Image.Interpolation.Nearest);
			// var newTexture = ImageTexture.CreateFromImage(image);
			// button.GetNode<TextureRect>("%TextureRect").Texture = newTexture;
			button.MouseEntered += () => _OnInventoryButtonMouseEntered(button as InventoryButton);
			button.MouseExited += _OnInventoryButtonMouseExited;
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
			GD.Print($"SetCommandLabel: {text}");

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

	public void ResetCommandLabel() => SetCommandLabel("");

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
			var messages = MessageDataManager.GetMessages(thingID, "name");
			SetCommandLabel(messages[0], true);
		}
	}

	public void _OnInventoryButtonMouseExited()
	{
		ResetCommandLabel();
	}
}
