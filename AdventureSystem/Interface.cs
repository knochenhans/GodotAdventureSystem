using Godot;

public partial class Interface : CanvasLayer
{
	GridContainer VerbGridContainer { get; set; }
	GridContainer InventoryGridContainer { get; set; }
	InventorySlot[] InventorySlots { get; set; }
	Label CommandLabel { get; set; }
	GamePanel GamePanel { get; set; }

	[Signal]
	public delegate void GamePanelMouseMotionEventHandler();

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
		InventorySlots = new InventorySlot[8];
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

		foreach (var slot in InventorySlots)
		{
			var button = ResourceLoader.Load<PackedScene>("res://AdventureSystem/InventoryButton.tscn").Instantiate() as InventoryButton;
			var textureRect = button.GetNode<TextureRect>("%TextureRect");
			var texture = ResourceLoader.Load<Texture2D>("res://Resources/Item.png");
			var image = texture.GetImage();
			image.Resize(image.GetWidth() * 4, image.GetHeight() * 4, Image.Interpolation.Nearest);
			var newTexture = ImageTexture.CreateFromImage(image);
			button.GetNode<TextureRect>("%TextureRect").Texture = newTexture;

			InventoryGridContainer.AddChild(button);
		}
	}

	public void _OnGamePanelInputEvent(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotionEvent)
			EmitSignal(SignalName.GamePanelMouseMotion);
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
}
