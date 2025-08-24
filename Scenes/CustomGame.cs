using System.Threading.Tasks;
using Godot;
using Godot.Collections;

public partial class CustomGame : BaseGame
{
    [Export] public Dictionary<string, string> DefaultVerbReactions { get; set; } = [];

    public Rect2 stageLimits;

    public Interface InterfaceNode => GetNode<Interface>("%Interface");

    public VariableManager VariableManager;
    public AdventureEntity PlayerEntity;

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

    public StageNodeActionCounter StageNodeActionCounter = new();

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

        StageManager.Instance.StageLoaded += OnStageLoaded;
        StageManager.Instance.StageUnloaded += OnStageUnloaded;
        StageManager.Instance.StageNodeExitedStage += OnStageNodeExitedStage;
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

    public async Task PerformVerbAction(string stageNodeID)
    {
        string performedAction;

        var currentStage = StageManager.Instance.CurrentStageScene as AdventureStage;

        var inkStory = currentStage.InkStory;

        inkStory.Continued += Talk;

        if (CurrentCommandState == CommandStateEnum.VerbSelected)
        {
            performedAction = currentVerbID;
            var useDefaultReaction = false;

            // Check if this is an inventory item
            if (PlayerEntity.Inventory.HasItemMoreThan(stageNodeID, 1))
            {
                if (!inkStory.EvaluateFunction("interact_inventory", stageNodeID, currentVerbID).AsBool())
                    useDefaultReaction = true;
            }
            else
            {
                if (currentVerbID == "pick_up")
                {
                    var stageNode = currentStage.GetStageNodeByID(stageNodeID);

                    if (stageNode is AdventureObject adventureObject)
                    {
                        if (adventureObject.canBePickedUp)
                            currentStage.MoveStageNodeToInventory(stageNodeID, PlayerEntity.ID);
                        else
                            useDefaultReaction = true;
                    }
                }
                else
                {
                    if (!inkStory.EvaluateFunction("interact_stage", stageNodeID, currentVerbID).AsBool())
                        useDefaultReaction = true;
                }
            }

            if (useDefaultReaction)
            {
                // No scripted reaction found, check the node's default reactions
                var thing = currentStage.GetStageNodeByID(stageNodeID);

                string reaction = "";

                if (thing is AdventureEntity adventureEntity)
                    reaction = adventureEntity.DefaultVerbReactions.TryGetValue(currentVerbID, out var entityReaction) ? entityReaction : "";

                if (thing is AdventureObject adventureObject)
                    reaction = adventureObject.DefaultVerbReactions.TryGetValue(currentVerbID, out var objectReaction) ? objectReaction : "";

                if (reaction == "")
                    // No default reaction on the node found, use the game's default verb reactions
                    reaction = DefaultVerbReactions.TryGetValue(currentVerbID, out var defaultReaction) ? defaultReaction : "";

                currentStage.ScriptManager.QueueAction(new ScriptActionMessage(PlayerEntity, reaction));
            }

            await currentStage.ScriptManager.RunScriptActionQueue();

            CurrentCommandState = CommandStateEnum.Idle;
        }
        else
        {
            InterfaceNode.SetCommandLabel(currentStage.GetStageNodeByID(stageNodeID).DisplayName);

            // Check for exit scripts
            if (!inkStory.EvaluateFunction("interact_stage", stageNodeID, "walk").AsBool())
            {
                Logger.Log($"No exit script found for {stageNodeID}", Logger.LogTypeEnum.Script);

                // var thing = currentStage.StageThingManager.GetThing(thingID);

                // if (thing.Resource is ExitResource exitResource)
                // {
                //     Logger.Log($"Exit destination found in node: {exitResource.TargetStageID}", Logger.LogTypeEnum.Script);
                //     StageManager.SwitchStage(exitResource.TargetStageID, exitResource.TargetEntryID);
                // }
            }
            await currentStage.ScriptManager.RunScriptActionQueue();
            performedAction = "walk";
        }

        StageNodeActionCounter.IncrementActionCounter(stageNodeID, performedAction);

        currentStage.InkStory.Continued -= Talk;
    }

    public async Task MovePlayerToStageNode(StageNode stageNode)
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

    private void Talk()
    {
        var currentStage = StageManager.Instance.CurrentStageScene as AdventureStage;
        var inkStory = currentStage.InkStory;
        var tag = inkStory.GetCurrentTags();

        if (tag.Count > 0 && inkStory.CurrentText != "")
        {
            AdventureEntity actingCharacter = PlayerEntity;
            AdventureEntity targetCharacter = null;

            if (_currentCommandState == CommandStateEnum.Dialog)
            {
                // if (tag[0] == "player")
                //     targetCharacter = DialogManager.CurrentDialogCharacter;
                // else
                //     targetCharacter = PlayerEntity;
            }
            if (tag[0] != "player")
            {
                actingCharacter = currentStage.GetStageNodeByID(tag[0]) as AdventureEntity;
            }

            currentStage.ScriptManager.QueueAction(new ScriptActionMessage(actingCharacter, inkStory.CurrentText, targetCharacter));
            currentStage.ScriptManager.QueueAction(new ScriptActionCharacterWait(actingCharacter, 0.3f));
        }
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
            InterfaceNode.UnpressVerbButton(currentVerbID);
            currentVerbID = "";
        }
    }

    public AdventureEntity GetEntity(string id) => GetCurrentAdventureStage().GetEntityByID(id) as AdventureEntity;

    public async Task StartDialog(string knotName)
    {
        // Test
    }

    protected override void OnStageLoaded(bool storedStateFound)
    {
        base.OnStageLoaded(storedStateFound);

        var adventureStage = StageManager.Instance.CurrentStageScene as AdventureStage;
        var inkStory = adventureStage.InkStory;
        adventureStage.ScriptManager = new CustomScriptManager(this, inkStory, GetTree());
    }

    protected override void OnStageUnloaded()
    {
        var adventureStage = StageManager.Instance.CurrentStageScene as AdventureStage;
        adventureStage.InkStory.ResetState();
        adventureStage.ScriptManager.Cleanup();
        adventureStage.ScriptManager = null;
    }

    public static AdventureStage GetCurrentAdventureStage() => StageManager.Instance.CurrentStageScene as AdventureStage;
}