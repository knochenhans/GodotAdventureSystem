using Godot;
using System;
using Godot.Collections;

public partial class Stage : Node2D
{
	public Node2D HotspotPolygonsNode { get; set; }
	public Interface InterfaceNode { get; set; }
	public Character PlayerCharacter { get; set; }
	public Node2D MapNode { get; set; }

	[Signal]
	public delegate void SetCommandLabelEventHandler(string commandLabel);

	[Signal]
	public delegate void ActivateThingEventHandler(Thing thing);

	public override void _Ready()
	{
		base._Ready();

		HotspotPolygonsNode = GetNode<Node2D>("HotspotPolygons");
		InterfaceNode = GetNode<Interface>("../Interface");
		PlayerCharacter = GetNode<Character>("PlayerCharacter");
		MapNode = GetNode<Node2D>("Map");

		// Convert HotspotPolygons to HotspotAreas
		Array<HotspotArea> hotspotAreas = new();

		var newHotspotAreaScene = ResourceLoader.Load<PackedScene>("res://AdventureSystem/HotspotArea.tscn");

		foreach (var hotspotPolygon in HotspotPolygonsNode.GetChildren())
		{
			if (hotspotPolygon is HotspotPolygon hotspotPolygonNode)
			{
				var hotspotArea = newHotspotAreaScene.Instantiate() as HotspotArea;
				hotspotArea.DisplayedName = hotspotPolygonNode.DisplayedName;
				hotspotArea.ID = hotspotPolygonNode.ID;
				// hotspotArea.Actions = hotspotPolygonNode.Actions;
				hotspotArea.GetNode<CollisionPolygon2D>("CollisionPolygon2D").Polygon = hotspotPolygonNode.Polygon;
				hotspotArea.Transform = hotspotPolygonNode.Transform;
				HotspotPolygonsNode.RemoveChild(hotspotPolygonNode);

				hotspotAreas.Add(hotspotArea);
			}
		}

		foreach (var hotspotArea in hotspotAreas)
		{
			hotspotArea.InputEvent += (viewport, @event, shapeIdx) => _OnThingInputEvent(@event, hotspotArea);
			HotspotPolygonsNode.AddChild(hotspotArea);
		}

		foreach (var object_ in GetNode<Node2D>("Objects").GetChildren())
			if (object_ is Object objectNode)
				objectNode.InputEvent += (viewport, @event, shapeIdx) => _OnThingInputEvent(@event, objectNode);

		var navigationregion = GetNode<NavigationRegion2D>("NavigationRegion2D");
	}

	public void _OnThingInputEvent(InputEvent @event, Thing thing)
	{
		if (@event is InputEventMouseMotion mouseMotionEvent)
			EmitSignal(SignalName.SetCommandLabel, MessageDataManager.GetMessages(thing.ID, "name")[0]);
		else if (@event is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.Pressed && mouseButtonEvent.ButtonIndex == MouseButton.Left)
			EmitSignal(SignalName.ActivateThing, thing);
	}

	public Vector2 GetSize()
	{
		return MapNode.GetNode<TextureRect>("Background").Texture.GetSize();
	}
}
