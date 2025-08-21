public partial class ScriptActionCharacterWait : AbstractScriptAction
{
    private AdventureEntity adventureEntity;
    private float seconds;

    public ScriptActionCharacterWait(AdventureEntity adventureEntity, float seconds)
    {
        this.adventureEntity = adventureEntity;
        this.seconds = seconds;
    }
}