using Godot;

[GlobalClass]
public partial class AdventureEntityResource : EntityResource
{
    [ExportGroup("Speech")]
    [Export] public PackedScene SpeechBubbleScene { get; set; }
    [Export] public Color SpeechBubbleColor { get; set; }
}
