using Godot;

public partial class CustomGame : BaseGame
{
    public Entity playerEntity;
    public Rect2 StageLimits;

    public override void _Ready()
    {
        CurrentGameState = GameState.Loading;

        GameInputManager = new CustomGameInputManager(this, Camera);

        StageManager.Instance.StageLoaded += InitStageContent;
        StageManager.Instance.EntityExitedStage += OnEntityExitedStage;
        StageManager.Instance.Init(this, "stage1");
        SaveStateManager = new SaveStateManager(this);

        StageManager.Instance.StoreStageStates();
        SceneManager.Instance.OverlayMenuNode.Closed += Resume;
        var initialState = StageManager.Instance.GetSaveData();
        SaveStateManager.SaveGameState(initialState, "init");

        Input.MouseMode = DefaultMouseMode;

        playerEntity = StageManager.Instance.CurrentStageScene.GetEntityByID("player");

        CurrentGameState = GameState.Running;

        CallDeferred(MethodName.SetStageLimits);
    }

    protected void SetStageLimits()
    {
        var GamePanelRect = GetNode<Interface>("%Interface").GetNode<Control>("%GamePanel").GetRect();
        var backgroundRect = ((CustomStageManager)StageManager.Instance).GetCurrentStageBackground().GetRect();
        var stageBackgroundSize = new Rect2(
            backgroundRect.Position,
            backgroundRect.Size * Camera.Zoom
        );

        StageLimits = new Rect2(
            GamePanelRect.Position,
            GamePanelRect.Size
        ).Intersection(stageBackgroundSize);
    }
}