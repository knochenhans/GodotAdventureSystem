using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using GodotInk;

public partial class CustomGame : BaseGame
{
    public Rect2 stageLimits;

    public Interface InterfaceNode => GetNode<Interface>("%Interface");

    public VariableManager VariableManager;
    AdventureEntity PlayerEntity;

    Dictionary<string, string> Verbs = new()
    {
        { "close", "Close" },
        { "give", "Give" },
        { "look", "Look" },
        { "open", "Open" },
        { "pick_up", "Pick up" },
        { "pull", "Pull" },
        { "push", "Push" },
        { "talk_to", "Talk to" },
        { "use", "Use" }
    };

    public enum CommandStateEnum
    {
        Idle,
        VerbSelected,
        Dialog
    }

    public string currentVerbID;

    public Dictionary<string, string> InkStoryStates { get; set; } = [];

    CommandStateEnum _currentCommandState = CommandStateEnum.Idle;
    public CommandStateEnum CurrentCommandState
    {
        get => _currentCommandState;
        set
        {
            CommandStateChanged(value);
            _currentCommandState = value;
            Logger.Log($"Command state changed to: {value}", Logger.LogTypeEnum.Script);
        }
    }

    public override void _Ready()
    {
        CurrentGameState = GameState.Loading;

        Input.SetCustomMouseCursor(DefaultCursorTexture, Input.CursorShape.Arrow, DefaultCursorHotspot);

        GameInputManager = new CustomGameInputManager(this, Camera);

        StageManager.Instance.StageLoaded += InitStageContent;
        StageManager.Instance.EntityExitedStage += OnEntityExitedStage;
        StageManager.Instance.Init(this);

        SetupInterface();

        SaveStateManager = new SaveStateManager(this);

        StageManager.Instance.StoreStageStates();
        SceneManager.Instance.OverlayMenuClosed += Resume;
        var initialState = StageManager.Instance.GetSaveData();
        SaveStateManager.SaveGameState(initialState, "init");

        Input.MouseMode = DefaultMouseMode;

        VariableManager = new VariableManager();
        PlayerEntity = StageManager.Instance.CurrentStageScene.GetEntityByID(PlayerEntityID) as AdventureEntity;

        CurrentGameState = GameState.Running;

        CallDeferred(MethodName.SetStageLimits);
    }

    private void SetupInterface()
    {
        InterfaceNode.Init(Verbs);

        InterfaceNode.GamePanelMouseMotion += OnGamePanelMouseMotion;
        InterfaceNode.GamePanelMousePressed += OnGamePanelMousePressed;

        // InterfaceNode.ThingClicked += OnThingClicked;
        // InterfaceNode.ThingHovered += OnThingHovered;

        InterfaceNode.VerbClicked += OnVerbClicked;
        InterfaceNode.VerbHovered += OnVerbHovered;
        InterfaceNode.VerbLeave += OnVerbLeave;
    }

    public void OnVerbHovered(string verbID)
    {
        if (CurrentCommandState == CommandStateEnum.Idle)
            InterfaceNode.SetCommandLabel(Verbs[verbID]);
    }

    public void OnVerbLeave()
    {
        if (CurrentCommandState == CommandStateEnum.Idle)
            InterfaceNode.ResetCommandLabel();
    }

    public void OnVerbClicked(string verbID)
    {
        // Logger.Log($"_OnVerbActivated: Verb: {verbID} activated", Logger.LogTypeEnum.Script);

        InterfaceNode.SetCommandLabel(Verbs[verbID]);
        currentVerbID = verbID;
        CurrentCommandState = CommandStateEnum.VerbSelected;
    }

    public void OnGamePanelMouseMotion()
    {
        // if (CurrentCommandState == CommandStateEnum.Idle)
        //     InterfaceNode.ResetCommandLabel();
        // else
        if (CurrentCommandState == CommandStateEnum.VerbSelected)
            InterfaceNode.SetCommandLabel(Verbs[currentVerbID]);
    }

    public async void OnGamePanelMousePressed(InputEventMouseButton mouseButtonEvent)
    {
        if (CurrentCommandState == CommandStateEnum.Idle)
        {
            // await CurrentStage.PlayerCharacter.MoveTo(mouseButtonEvent.Position / Camera2DNode.Zoom + Camera2DNode.Position, 1);
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

    public void OnStageNodeHovered(StageNode stageNode)
    {
        if (stageNode == null)
            return;

        if (CurrentCommandState != CommandStateEnum.Dialog)
        {
            // var thing = CurrentStage.StageThingManager.GetThing(thingID);
            // ThingResource thingResource;

            // if (thing == null)
            //     thingResource = CurrentStage.PlayerCharacter.Inventory.FindThing(thingID);
            // else
            //     thingResource = thing.Resource as ThingResource;

            var stageNodeName = stageNode.DisplayName;

            if (CurrentCommandState == CommandStateEnum.Idle)
                InterfaceNode.SetCommandLabel(stageNodeName);
            else if (CurrentCommandState == CommandStateEnum.VerbSelected)
                InterfaceNode.SetCommandLabel($"{Verbs[currentVerbID]} {stageNodeName}");
        }
    }

    public async void OnStageNodeClicked(StageNode stageNode, Vector2 mousePosition)
    {
        if (CurrentCommandState != CommandStateEnum.Dialog)
        {
            // var thing = CurrentStage.StageThingManager.GetThing(thingID);
            // ThingResource thingResource;

            // if (thing == null)
            // {
            //     thingResource = CurrentStage.PlayerCharacter.Inventory.FindThing(thingID);
            // }
            // else
            // {
            //     if (thing is HotspotArea hotspotArea)
            //     {
            //         await CurrentStage.PlayerCharacter.MoveTo(mousePosition / Camera2DNode.Zoom + Camera2DNode.Position);
            //     }
            //     else
            //     {
            //         thingResource = thing.Resource as ThingResource;
            if (stageNode != null && stageNode != PlayerEntity)
                await MovePlayerToStageNode(stageNode);
            //     }
            // }

            await PerformVerbAction(stageNode.ID);
        }
    }

    private async Task PerformVerbAction(string thingID)
    {
        GD.Print($"Performing verb action: {currentVerbID} on {thingID}");
    }

    private async Task MovePlayerToStageNode(StageNode stageNode)
	{
		Vector2 position = Vector2.Zero;
		if (stageNode is Object obj)
			position = obj.Position;
		else if (stageNode is AdventureEntity character)
			position = character.Position;
		// else if (stageNode is HotspotArea hotspotArea)
		// 	position = hotspotArea.GetClosestPoint(CurrentStage.PlayerCharacter.Position) + hotspotArea.Position;
		else
			Logger.LogError($"OnAreaActivated: Area {stageNode.ID} is not an Object or an AdventureEntity", Logger.LogTypeEnum.Script);

		// if (position.DistanceTo(StageNode.PlayerCharacter.Position) > 20)
		await PlayerEntity.MoveTo(position, 20);
	}

    protected void SetStageLimits()
    {
        var GamePanelRect = InterfaceNode.GetNode<Control>("%GamePanel").GetRect();
        var backgroundRect = ((CustomStageManager)StageManager.Instance).GetCurrentStageBackground().GetRect();
        var stageBackgroundSize = new Rect2(
            backgroundRect.Position,
            backgroundRect.Size * Camera.Zoom
        );

        stageLimits = new Rect2(
            GamePanelRect.Position,
            GamePanelRect.Size
        ).Intersection(stageBackgroundSize);
    }

    private void CommandStateChanged(CommandStateEnum value)
    {
        if (value == CommandStateEnum.Idle)
        {
            InterfaceNode.ResetCommandLabel();
            currentVerbID = "";
        }
    }

    public AdventureEntity GetEntity(string id) => GetCurrentStage().GetEntityByID(id) as AdventureEntity;

    public async Task StartDialog(string knotName)
    {
        // Test
    }

    public Stage GetCurrentStage() => StageManager.Instance.CurrentStageScene;

    public InkStory GetCurrentInkStory() => (StageManager.Instance.CurrentStageScene.StageResource as AdventureStageResource).InkStory;
}