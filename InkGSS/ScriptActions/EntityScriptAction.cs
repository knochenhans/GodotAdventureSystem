public partial class EntityScriptAction : AbstractScriptAction
{
    public AdventureEntity Entity { get; set; }

    public EntityScriptAction(AdventureEntity entity) { Entity = entity; }
}
