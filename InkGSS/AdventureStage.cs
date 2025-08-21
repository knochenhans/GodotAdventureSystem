using GodotInk;
using Godot.Collections;
using System.Linq;

public partial class AdventureStage : Stage
{
    public InkStory InkStory;

    public CustomScriptManager ScriptManager;

    public override void _Ready()
    {
        base._Ready();

        InkStory = (StageResource as AdventureStageResource).InkStory;
    }

    public Array<AdventureEntity> GetAdventureEntities() => [.. GetEntities().Where(e => e is AdventureEntity).Cast<AdventureEntity>()];
    public AdventureEntity GetAdventureEntityByID(string id) => GetAdventureEntities().FirstOrDefault(e => e.ID == id);
}