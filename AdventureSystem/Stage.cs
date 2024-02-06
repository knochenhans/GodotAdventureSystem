using Godot;
using System;
using Godot.Collections;

public partial class Stage : Node2D
{
	public Node2D HotspotPolygonsNode { get; set; }
	public Interface InterfaceNode { get; set; }
	public TextureRect BackgroundNode { get; set; }

	public PlayerCharacter PlayerCharacter { get; set; }

	// [Signal]
	// public delegate void SetCommandLabelEventHandler(string commandLabel);

	[Signal]
	public delegate void ThingHoveredEventHandler(string thingID);

	[Signal]
	public delegate void ThingLeaveEventHandler(string thingID);

	[Signal]
	public delegate void ThingClickedEventHandler(string thingID);

	public override void _Ready()
	{
		base._Ready();

		HotspotPolygonsNode = GetNode<Node2D>("HotspotPolygons");
		InterfaceNode = GetNode<Interface>("../Interface");
		BackgroundNode = GetNode<TextureRect>("Background");

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

		foreach (var character in GetNode<Node2D>("Characters").GetChildren())
			if (character is Character characterNode)
				characterNode.InputEvent += (viewport, @event, shapeIdx) => _OnThingInputEvent(@event, characterNode);

		var navigationregion = GetNode<NavigationRegion2D>("NavigationRegion2D");
	}

	public void _OnThingInputEvent(InputEvent @event, Thing thing)
	{
		if (@event is InputEventMouseMotion mouseMotionEvent)
			EmitSignal(SignalName.ThingHovered, thing.ID);
		else if (@event is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.Pressed && mouseButtonEvent.ButtonIndex == MouseButton.Left)
			EmitSignal(SignalName.ThingClicked, thing.ID);
	}

	public Vector2 GetSize()
	{
		return BackgroundNode.Texture.GetSize();
	}

	public Object FindObject(string objectID)
	{
		foreach (var object_ in GetNode<Node2D>("Objects").GetChildren())
			if (object_ is Object objectNode && objectNode.ID == objectID)
				return objectNode;
		return null;
	}

	public Array<Thing> CollectThings()
	{
		Array<Thing> things = new();

		foreach (var objectNode in GetNode<Node2D>("Objects").GetChildren())
			things.Add(objectNode as Thing);

		foreach (var characterNode in GetNode<Node2D>("Characters").GetChildren())
			things.Add(characterNode as Thing);

		foreach (var hotspotAreaNode in HotspotPolygonsNode.GetChildren())
			things.Add(hotspotAreaNode as Thing);

		return things;
	}

	public void InitPlayerCharacter(PlayerCharacter playerCharacter)
	{
		PlayerCharacter = playerCharacter;
		PlayerCharacter.Position = GetNode<Entry>("Entries/Default").Position;

		AddChild(PlayerCharacter);
	}
}
