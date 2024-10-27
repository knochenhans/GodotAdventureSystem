using System;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

public partial class CharacterScriptAction : AbstractScriptAction
{
    public Character Character { get; set; }

    public CharacterScriptAction(Character character) { Character = character; }
}

public partial class ScriptActionCharacterWait : CharacterScriptAction
{
    public float Seconds { get; set; }

    public ScriptActionCharacterWait(Character character, float seconds) : base(character) { Seconds = seconds; }
    public override Task Execute() => Task.Delay(TimeSpan.FromSeconds(Seconds));
}

public partial class ScriptActionMessage : CharacterScriptAction
{
    public string Message { get; set; }
    public Thing Target { get; set; }

    public ScriptActionMessage(Character character, string message, Thing target = null) : base(character) { Message = message; Target = target; }
    public override async Task Execute()
    {
        if (Target == null)
            Character.LookAt(Character.GlobalPosition);
        else
            Character.LookAt(Target.GlobalPosition);
        await Character.SpeechBubble(Message);
    }
}

public partial class ScriptActionLookAt : CharacterScriptAction
{
    public Thing Target { get; set; }

    public ScriptActionLookAt(Character character, Thing target) : base(character) { Target = target; }
    public override Task Execute()
    {
        Character.LookAt(Target.GlobalPosition);

        return Task.CompletedTask;
    }
}

public partial class ScriptActionMove : CharacterScriptAction
{
    public Vector2 Position { get; set; }
    public bool IsRelative { get; set; }

    public ScriptActionMove(Character character, Vector2 position, bool isRelative = false) : base(character) { Position = position; IsRelative = isRelative; }
    public override async Task Execute() => await Character.MoveTo(Position, 1, IsRelative);
}

public partial class ScriptActionPlayAnimation : CharacterScriptAction
{
    public string AnimationName { get; set; }

    public ScriptActionPlayAnimation(Character character, string animationID) : base(character) { AnimationName = animationID; }
    public override async Task Execute() => await Character.PlayAnimation(AnimationName);
}

public partial class ScriptActionStartDialog : CharacterScriptAction
{
    public string KnotName { get; set; }
    public Game Game { get; set; }

    public ScriptActionStartDialog(PlayerCharacter character, Game game, string knotName) : base(character) { KnotName = knotName; Game = game; }
    public async override Task Execute() => await Game.StartDialog(KnotName);
}

public partial class ScriptActionSwitchStage : CharacterScriptAction
{
    public string StageID { get; set; }
    public string EntryID { get; set; }

    public ScriptActionSwitchStage(PlayerCharacter playerCharacter, string stageID, string entryID = "") : base(playerCharacter) { StageID = stageID; EntryID = entryID; }
    public override Task Execute()
    {
        if (Character is PlayerCharacter playerCharacter)
            playerCharacter.RequestSwitchStage(StageID, EntryID);
        return Task.CompletedTask;
    }
}

public partial class ScriptActionPrint : ScriptObjectControllerAction
{
    public string Message { get; set; }

    public ScriptActionPrint(Array<ScriptObjectController> scriptObjectControllers, string objectControllerID, string message) : base(scriptObjectControllers, objectControllerID) => Message = message;

    public override Task Execute()
    {
        Logger.Log($"Printing message: {Message}", Logger.LogTypeEnum.Script);

        var scriptObjectController = GetScriptObjectController();

        scriptObjectController?.OnPrint(Message);

        return Task.CompletedTask;
    }
}