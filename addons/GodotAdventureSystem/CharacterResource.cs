using Godot;

public partial class CharacterResource : Resource
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
    [Export] public string IdlePrefix { get; set; } = "idle";
    [Export] public string MovementPrefix { get; set; } = "move";

    [ExportGroup("Sounds")]
    [Export] public AudioStream MovementSound { get; set; } = new();
    [Export] public AudioStream PickupSound { get; set; } = new();
}