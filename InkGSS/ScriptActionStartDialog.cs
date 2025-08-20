using System.Threading.Tasks;

public partial class ScriptActionStartDialog : EntityScriptAction
{
    public string KnotName { get; set; }
    public Game Game { get; set; }

    public ScriptActionStartDialog(AdventureEntity entity, Game game, string knotName) : base(entity) { KnotName = knotName; Game = game; }
    // public async override Task Execute() => await Game.StartDialog(KnotName);
}

// public partial class ScriptActionSwitchStage : EntityScriptAction
// {
//     public string StageID { get; set; }
//     public string EntryID { get; set; }

//     public ScriptActionSwitchStage(AdventureEntity entity, string stageID, string entryID = "") : base(entity) { StageID = stageID; EntryID = entryID; }
//     public override Task Execute()
//     {
//         if (Entity is AdventureEntity playerEntity)
//             playerEntity.RequestSwitchStage(StageID, EntryID);
//         return Task.CompletedTask;
//     }
// }

// public partial class ScriptActionPrint : ScriptNodeControllerAction
// {
//     public string Message { get; set; }

//     public ScriptActionPrint(string nodeControllerID, string message) : base(nodeControllerID) => Message = message;

//     public override Task Execute()
//     {
//         Logger.Log($"Printing message: {Message}", Logger.LogTypeEnum.Script);

//         var scriptObjectController = GetScriptObjectController();

//         scriptObjectController?.OnPrint(Message);

//         return Task.CompletedTask;
//     }
// }