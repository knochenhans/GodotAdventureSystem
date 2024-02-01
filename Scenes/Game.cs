using Godot;
using System;

public partial class Game : Scene
{
	enum CommandState
	{
		Idle,
		VerbSelected,
	}

	public Verb[] Verbs { get; set; }
	public Interface InterfaceNode { get; set; }
	public Stage StageNode { get; set; }
	public VariableManager VariableManager { get; set; } = new();

	public Verb currentVerb { get; set; }

	CommandState _currentCommandState = CommandState.Idle;
	private CommandState CurrentCommandState
	{
		get => _currentCommandState;
		set => _currentCommandState = value;
	}

	public override void _Ready()
	{
		base._Ready();

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
		InterfaceNode.GamePanelActivated += _OnGamePanelActivated;

		StageNode = GetNode<Stage>("Stage");
		StageNode.SetCommandLabel += _OnInterfaceSetCommandLabel;
		StageNode.ActivateHotspot += _OnHotspotAreaActivated;
	}

	public void _OnButtonPressed() => SceneManagerNode.ChangeToScene("Menu");

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
			SceneManagerNode.Quit();
	}

	public void _OnVerbActivated(Verb verb)
	{
		GD.Print($"_OnVerbActivated: Verb: {verb.Name} activated");

		InterfaceNode.SetCommandLabel(verb.Name);
		currentVerb = verb;
		CurrentCommandState = CommandState.VerbSelected;
	}

	public void _OnGamePanelActivated()
	{
		if (CurrentCommandState == CommandState.Idle)
			InterfaceNode.ResetCommandLabel();
	}

	public void _OnHotspotAreaActivated(HotspotArea hotspotArea)
	{
		var message = "That doesn't seem to work.";
		if (CurrentCommandState == CommandState.VerbSelected)
		{
			if (hotspotArea.Actions.ContainsKey(currentVerb.ID))
				if (hotspotArea.Actions[currentVerb.ID] != "")
				{
					GD.Print($"_OnHotspotAreaActivated: Verb: {currentVerb.Name}, Hotspot: {hotspotArea.DisplayedName}, Action: {hotspotArea.Actions[currentVerb.ID]}");
					message = hotspotArea.Actions[currentVerb.ID];
				}

			InterfaceNode.SetCommandLabel(message);

			CurrentCommandState = CommandState.Idle;
		}
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
}
