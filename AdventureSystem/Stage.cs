using Godot;
using System;
using Godot.Collections;

public partial class Stage : Node2D
{
	public Node2D HotspotPolygonsNode { get; set; }
	public Interface InterfaceNode { get; set; }
	public Character PlayerCharacter { get; set; }

	[Signal]
	public delegate void SetCommandLabelEventHandler(string commandLabel);

	[Signal]
	public delegate void ActivateHotspotEventHandler(HotspotArea hotspotArea);

	public override void _Ready()
	{
		base._Ready();

		HotspotPolygonsNode = GetNode<Node2D>("HotspotPolygons");
		InterfaceNode = GetNode<Interface>("../Interface");
		PlayerCharacter = GetNode<Character>("PlayerCharacter");

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
				hotspotArea.Actions = hotspotPolygonNode.Actions;
				hotspotArea.GetNode<CollisionPolygon2D>("CollisionPolygon2D").Polygon = hotspotPolygonNode.Polygon;
				hotspotArea.Transform = hotspotPolygonNode.Transform;
				HotspotPolygonsNode.RemoveChild(hotspotPolygonNode);

				hotspotAreas.Add(hotspotArea);
			}
		}

		foreach (var hotspotArea in hotspotAreas)
		{
			hotspotArea.InputEvent += (viewport, @event, shapeIdx) => _OnHotspotAreaInputEvent(@event, hotspotArea);
			HotspotPolygonsNode.AddChild(hotspotArea);
		}

		var navigationregion = GetNode<NavigationRegion2D>("NavigationRegion2D");
		// PlayerCharacter.GetNode<NavigationAgent2D>("NavigationAgent2D").SetNavigationMap(navigationregion.GetNavigationMap());
	}

	public void _OnHotspotAreaInputEvent(InputEvent @event, HotspotArea hotspotAreaNode)
	{
		if (@event is InputEventMouseMotion mouseMotionEvent)
			EmitSignal(SignalName.SetCommandLabel, hotspotAreaNode.DisplayedName);
		else if (@event is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.Pressed && mouseButtonEvent.ButtonIndex == MouseButton.Left)
			EmitSignal(SignalName.ActivateHotspot, hotspotAreaNode);
	}
}
