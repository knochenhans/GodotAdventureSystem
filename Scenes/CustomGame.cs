using Godot;

public partial class CustomGame : Game
{
    public override void _Ready()
    {
        CurrentGameState = GameState.Loading;

        StageManager.Instance.StageLoaded += InitStageContent;
        StageManager.Instance.EntityExitedStage += OnEntityExitedStage;
        StageManager.Instance.Init(this, "stage1");

        GameInputManager = new CustomGameInputManager(this, Camera, TileMapManager);
        SaveStateManager = new SaveStateManager(this);

        StageManager.Instance.StoreStageStates();
        SceneManager.Instance.OverlayMenuNode.Closed += Resume;
        var initialState = StageManager.Instance.GetSaveData();
        SaveStateManager.SaveGameState(initialState, "init");

        Input.MouseMode = DefaultMouseMode;

        CurrentGameState = GameState.Running;
    }
}
