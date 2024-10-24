using Godot;
using System;
using Godot.Collections;

[Icon("res://addons/GodotAdventureSystem/icons/Stage.svg")]
public partial class Stage : Node2D
{
	public Interface InterfaceNode { get; set; }
	public TextureRect BackgroundNode { get; set; }

	public PlayerCharacter PlayerCharacter { get; set; }

	[Signal]
	public delegate void ThingHoveredEventHandler(string thingID);

	[Signal]
	public delegate void ThingLeaveEventHandler(string thingID);

	[Signal]
	public delegate void ThingClickedEventHandler(string thingID);

	public override void _Ready()
	{
		base._Ready();

		InterfaceNode = GetNode<Interface>("../Interface");
		BackgroundNode = GetNode<TextureRect>("Background");

		CreateHotspotAreas();

		foreach (var object_ in GetTree().GetNodesInGroup("object"))
			if (object_ is Object objectNode)
				objectNode.InputEvent += (viewport, @event, shapeIdx) => OnThingInputEvent(@event, objectNode);

		foreach (var character in GetTree().GetNodesInGroup("character"))
			if (character is Character characterNode)
				characterNode.InputEvent += (viewport, @event, shapeIdx) => OnThingInputEvent(@event, characterNode);

		var walkableRegion = GetNode<NavigationRegion2D>("WalkableRegion");
	}

	// Convert Hotspots into HotspotAreas
	private void CreateHotspotAreas()
	{
		var hotspotNodes = GetTree().GetNodesInGroup("hotspot");

		Array<HotspotArea> hotspotAreas = new();

		var newHotspotAreaScene = ResourceLoader.Load<PackedScene>("res://addons/GodotAdventureSystem/HotspotArea.tscn");

		foreach (var _hotspotNode in hotspotNodes)
		{
			if (_hotspotNode is Hotspot hotspotNode)
			{
				var hotspotArea = newHotspotAreaScene.Instantiate() as HotspotArea;
				hotspotArea.ThingResource.DisplayedName = hotspotNode.DisplayedName;
				hotspotArea.ThingResource.ID = hotspotNode.ID;
				hotspotArea.GetNode<CollisionPolygon2D>("CollisionPolygon2D").Polygon = hotspotNode.Polygon;
				hotspotArea.Transform = hotspotNode.Transform;
				RemoveChild(hotspotNode);

				hotspotAreas.Add(hotspotArea);
			}
		}

		foreach (var hotspotArea in hotspotAreas)
		{
			hotspotArea.InputEvent += (viewport, @event, shapeIdx) => OnThingInputEvent(@event, hotspotArea);
			AddChild(hotspotArea);
		}
	}

	public void OnThingInputEvent(InputEvent @event, Thing thing)
	{
		if (@event is InputEventMouseMotion mouseMotionEvent)
			EmitSignal(SignalName.ThingHovered, thing.ThingResource.ID);
		else if (@event is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.Pressed && mouseButtonEvent.ButtonIndex == MouseButton.Left)
			EmitSignal(SignalName.ThingClicked, thing.ThingResource.ID);
	}

	public Vector2 GetSize()
	{
		return BackgroundNode.Texture.GetSize();
	}

	public Object FindObject(string objectID)
	{
		foreach (var object_ in GetTree().GetNodesInGroup("object"))
			if (object_ is Object objectNode && objectNode.ThingResource.ID == objectID)
				return objectNode;
		return null;
	}

	public Array<Thing> CollectThings()
	{
		Array<Thing> things = new();

		foreach (var objectNode in GetTree().GetNodesInGroup("object"))
			things.Add(objectNode as Thing);

		foreach (var characterNode in GetTree().GetNodesInGroup("character"))
			things.Add(characterNode as Thing);

		foreach (var hotspotAreaNode in GetTree().GetNodesInGroup("hotspot"))
			things.Add(hotspotAreaNode as Thing);

		return things;
	}

	public void SetupPlayerCharacter(PlayerCharacter playerCharacter, string entryID = "default")
	{
		PlayerCharacter = playerCharacter;
		var entries = GetTree().GetNodesInGroup("entry");

		var entryFound = false;

		foreach (var entryNode in entries)
		{
			if (entryNode is Entry entry && entry.ID == entryID && entry.GetParent() == this)
			{
				PlayerCharacter.Position = entry.Position;
				AddChild(PlayerCharacter);
				entryFound = true;
				break;
			}
		}

		if (!entryFound)
			Logger.Log($"No entry named '{entryID}' found in the stage, unable to place the player character.", Logger.LogTypeEnum.Error);
	}
}
