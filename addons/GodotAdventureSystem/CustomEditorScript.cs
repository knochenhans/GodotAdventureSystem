using Godot;

[Tool]
public partial class CustomEditorScript : EditorScript
{
    public override void _Run()
    {
        GD.Print("Hello from C#");
    }
}
