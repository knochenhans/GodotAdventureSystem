using Godot;
using System;
using System.Threading.Tasks;

public partial class AbstractScriptAction : GodotObject
{
    public Character Character { get; set; }

    public AbstractScriptAction(Character character) { Character = character; }
    public virtual Task Execute() { return Task.CompletedTask; }
}

public partial class ScriptActionMessage : AbstractScriptAction
{
    public string Message { get; set; }
    public Thing Target { get; set; }

    public ScriptActionMessage(Character character, string message, Thing target = null) : base(character) { Message = message; Target = target; }
    public override async Task Execute()
    {
        if (Target == null)
            Character.LookTo(Character.GlobalPosition);
        else
            Character.LookTo(Target.GlobalPosition);
        await Character.SpeechBubble(Message);
    }
}

public partial class ScriptActionMove : AbstractScriptAction
{
    public Vector2 Position { get; set; }
    public bool IsRelative { get; set; }

    public ScriptActionMove(PlayerCharacter character, Vector2 position, bool isRelative = false) : base(character) { Position = position; IsRelative = isRelative; }
    public override async Task Execute()
    {
        await Character.MoveTo(Position, 1, IsRelative);
    }
}

public partial class ScriptActionWait : AbstractScriptAction
{
    public float Seconds { get; set; }

    public ScriptActionWait(Character character, float seconds) : base(character) { Seconds = seconds; }
    public override Task Execute()
    {
        GD.Print($"Waiting for {Seconds} seconds");
        return Task.Delay(TimeSpan.FromSeconds(Seconds));
    }
}

public partial class ScriptActionPlayAnimation : AbstractScriptAction
{
    public string AnimationName { get; set; }

    public ScriptActionPlayAnimation(Character character, string animationID) : base(character) { AnimationName = animationID; }
    public override async Task Execute()
    {
        await Character.PlayAnimation(AnimationName, false);
    }
}

public partial class ScriptActionStartDialog : AbstractScriptAction
{
    public string KnotName { get; set; }
    public Game Game { get; set; }

    public ScriptActionStartDialog(PlayerCharacter character, Game game, string knotName) : base(character) { KnotName = knotName; Game = game; }
    public async override Task Execute()
    {
        await Game.StartDialog(KnotName);
    }
}

public partial class ScriptActionSwitchStage : AbstractScriptAction
{
    public string StageID { get; set; }
    public string EntryID { get; set; }

    public ScriptActionSwitchStage(PlayerCharacter playerCharacter, string stageID, string entryID = "default") : base(playerCharacter) { StageID = stageID; EntryID = entryID; }
    public override Task Execute()
    {
        if(Character is PlayerCharacter playerCharacter)
            playerCharacter.RequestSwitchStage(StageID, EntryID);
        return Task.CompletedTask;
    }
}