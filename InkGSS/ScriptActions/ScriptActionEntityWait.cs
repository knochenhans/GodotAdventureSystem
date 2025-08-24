using System;
using System.Threading.Tasks;

public partial class ScriptActionEntityWait : EntityScriptAction
{
    public float Seconds { get; set; }

    public ScriptActionEntityWait(AdventureEntity entity, float seconds) : base(entity) { Seconds = seconds; }
    public override Task Execute() => Task.Delay(TimeSpan.FromSeconds(Seconds));
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