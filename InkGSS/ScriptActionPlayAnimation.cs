using System.Threading.Tasks;

public partial class ScriptActionPlayAnimation : EntityScriptAction
{
    public string AnimationName { get; set; }

    public ScriptActionPlayAnimation(AdventureEntity entity, string animationID) : base(entity) { AnimationName = animationID; }
    public override async Task Execute() => await Entity.PlayAnimation(AnimationName);
}

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