using Godot;

[Tool]
[Icon("res://addons/GodotAdventureSystem/icons/Hotspot.svg")]
public partial class Hotspot : Polygon2D
{
    [Export]
    public string ID { get; set; }

    [Export]
    public string DisplayedName { get; set; }

    [Export]
    public Godot.Collections.Dictionary<string, string> DefaultReactions { get; set; } = new Godot.Collections.Dictionary<string, string>
    {
        ["give"] = "",
        ["pick_up"] = "",
        ["use"] = "",
        ["open"] = "",
        ["look"] = "",
        ["push"] = "",
        ["close"] = "",
        ["talk_to"] = "",
        ["pull"] = "",
    };

    public override void _EnterTree()
    {
        Color = new Color("00ffff43");
        AddToGroup("hotspot");
    }
}