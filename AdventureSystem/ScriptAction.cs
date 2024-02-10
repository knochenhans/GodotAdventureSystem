using Godot;
using System;
using System.Threading.Tasks;

public partial class ScriptAction : GodotObject
{
	public Character Character { get; set; }

	public ScriptAction(Character character) { Character = character; }
	public virtual Task Execute() { return Task.CompletedTask; }
}

public partial class ScriptActionMessage : ScriptAction
{
	public string Message { get; set; }

	public ScriptActionMessage(Character character, string message) : base(character) { Message = message; }
	public override async Task Execute()
	{
		await Character.SpeechBubble(Message);
	}
}

public partial class ScriptActionMove : ScriptAction
{
	public Vector2 Position { get; set; }
	public bool IsRelative { get; set; }

	public ScriptActionMove(PlayerCharacter character, Vector2 position, bool isRelative = false) : base(character) { Position = position; IsRelative = isRelative; }
	public override async Task Execute()
	{
		await Character.MoveTo(Position, 1, IsRelative);
	}
}

public partial class ScriptActionWait : ScriptAction
{
	public float Seconds { get; set; }

	public ScriptActionWait(Character character, float seconds) : base(character) { Seconds = seconds; }
	public override Task Execute()
	{
		GD.Print($"Waiting for {Seconds} seconds");
		return Task.Delay(TimeSpan.FromSeconds(Seconds));
	}
}

public partial class ScriptActionPlayAnimation : ScriptAction
{
	public string AnimationName { get; set; }

	public ScriptActionPlayAnimation(Character character, string animationID) : base(character) { AnimationName = animationID; }
	public override async Task Execute()
	{
		await Character.PlayAnimation(AnimationName, false);
	}
}

public partial class ScriptActionStartDialog : ScriptAction
{
	public string KnotName { get; set; }
	public Game Game { get; set; }

	public ScriptActionStartDialog(PlayerCharacter character, Game game, string knotName) : base(character) { KnotName = knotName; Game = game; }
	public async override Task Execute()
	{
		await Game.StartDialog(KnotName);
	}
}