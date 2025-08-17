using Godot;

public partial class CustomGame : BaseGame
{
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

        CurrentGameState = GameState.Running;
    }
}
