using System.Threading.Tasks;
using Godot;

public partial class ScriptActionMessage : EntityScriptAction
{
    public string Message { get; set; }
    public Node2D Target { get; set; }

    public ScriptActionMessage(AdventureEntity entity, string message, Node2D target = null) : base(entity) { Message = message; Target = target; }
    public override async Task Execute()
    {
        if (Target == null)
            Entity.TurnTowardsTarget(Entity.GlobalPosition);
        else
            Entity.TurnTowardsTarget(Target.GlobalPosition);
        await Entity.SpeechBubble(Message);
    }
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