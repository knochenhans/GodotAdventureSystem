using Godot;
using Godot.Collections;

public partial class CharacterResource : ThingResource
{
    public enum OrientationEnum
    {
        Idle,
        Up,
        Down,
        Left,
        Right
    }

    [Export] public OrientationEnum InitialOrientation { get; set; } = OrientationEnum.Down;
    [Export] public float MovementSpeed { get; set; }
    [Export] public Color SpeechColor { get; set; } = Colors.White;

    [ExportGroup("Sprite")]
    [Export] public SpriteFrames SpriteFrames { get; set; } = new();
    [Export] public Dictionary<string, string> AnimationPrefixMap { get; set; } = new Dictionary<string, string>
    {
        { "idle", "idle" },
        { "move", "move" },
        { "talk", "talk" }
    };
    [Export] public string DefaultAnimation { get; set; } = "";

    [ExportGroup("Sounds")]
    [Export] public AudioStream MovementSound { get; set; } = new();
    [Export] public AudioStream PickupSound { get; set; } = new();
}