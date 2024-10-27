using Godot;
using GodotInk;
using Godot.Collections;

[Icon("res://addons/GodotAdventureSystem/icons/Stage.svg")]
public partial class Stage : Node2D
{
	[Signal] public delegate void ThingHoveredEventHandler(string thingID);
	[Signal] public delegate void ThingLeaveEventHandler(string thingID);
	[Signal] public delegate void ThingClickedEventHandler(string thingID, Vector2 position);

	[Export] public string ID { get; set; } = "";
	[Export] public InkStory InkStory { get; set; }

	public Interface InterfaceNode => GetNode<Interface>("../Interface");
	public TextureRect BackgroundNode => GetNode<TextureRect>("Background");

	public PlayerCharacter PlayerCharacter { get; set; }
	public StageThingManager StageThingManager { get; set; } = new();
	public CustomScriptManager ScriptManager { get; set; }

	public override void _Ready()
	{
		base._Ready();

		CreateHotspotAreas();

		foreach (var object_ in GetTree().GetNodesInGroup("object"))
			if (object_ is Object objectNode)
				objectNode.InputEvent += (viewport, @event, shapeIdx) => OnThingInputEvent(@event, objectNode);

		foreach (var character in GetTree().GetNodesInGroup("character"))
			if (character is Character characterNode)
				characterNode.InputEvent += (viewport, @event, shapeIdx) => OnThingInputEvent(@event, characterNode);

		var walkableRegion = GetNode<NavigationRegion2D>("WalkableRegion");

		StageThingManager.Clear();
		StageThingManager.RegisterThings(CollectThings());

		ScriptManager = new CustomScriptManager(GetParent() as Game);
	}

	public override void _ExitTree()
	{
		base._ExitTree();

		ScriptManager.Cleanup();
		ScriptManager = null;
	}

	// Convert Hotspots into HotspotAreas
	private void CreateHotspotAreas()
	{
		var hotspotNodes = GetTree().GetNodesInGroup("hotspot");

		Array<HotspotArea> hotspotAreas = new();

		var newHotspotAreaScene = ResourceLoader.Load<PackedScene>("res://addons/GodotAdventureSystem/HotspotArea.tscn");
		var newExitAreaScene = ResourceLoader.Load<PackedScene>("res://addons/GodotAdventureSystem/ExitArea.tscn");

		foreach (var _hotspotNode in hotspotNodes)
		{
			if (_hotspotNode is Hotspot hotspotNode)
			{
				HotspotArea hotspotArea;
				if (hotspotNode is Exit)
					hotspotArea = newExitAreaScene.Instantiate() as HotspotArea;
				else
					hotspotArea = newHotspotAreaScene.Instantiate() as HotspotArea;

				(hotspotArea.Resource as ThingResource).DisplayedName = hotspotNode.DisplayedName;
				(hotspotArea.Resource as ThingResource).ID = hotspotNode.ID;
				hotspotArea.GetNode<CollisionPolygon2D>("CollisionPolygon2D").Polygon = hotspotNode.Polygon;
				hotspotArea.Transform = hotspotNode.Transform;

				if (hotspotNode is Exit exitNode)
				{
					(hotspotArea.Resource as ExitResource).Destination = exitNode.Destination;
					(hotspotArea.Resource as ExitResource).Entry = exitNode.Entry;
				}
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
			EmitSignal(SignalName.ThingHovered, (thing.Resource as ThingResource).ID);
		else if (@event is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.Pressed && mouseButtonEvent.ButtonIndex == MouseButton.Left)
			EmitSignal(SignalName.ThingClicked, (thing.Resource as ThingResource).ID, mouseButtonEvent.Position);
	}

	public Vector2 GetSize()
	{
		return BackgroundNode.Texture.GetSize();
	}

	public Object FindObject(string objectID)
	{
		foreach (var object_ in GetTree().GetNodesInGroup("object"))
			if (object_ is Object objectNode && (objectNode.Resource as ThingResource).ID == objectID)
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

	public void SetupPlayerCharacter(PlayerCharacter playerCharacter, string entryID = "")
	{
		PlayerCharacter = playerCharacter;
		var entries = GetTree().GetNodesInGroup("entry");

		Entry foundEntry = null;
		Entry fallbackEntry = null;

		foreach (var entryNode in entries)
		{
			if (entryNode is Entry entry)
			{
				// Use the first entry as a fallback if the entryID is not found
				fallbackEntry ??= entry;

				if (entry.ID == entryID && entry.GetParent() == this)
				{
					foundEntry = entry;
					break;
				}
			}
		}

		var finalEntry = foundEntry ?? fallbackEntry;
		if (finalEntry != null)
		{
			AddChild(PlayerCharacter);
			PlayerCharacter.Position = finalEntry.Position;
			PlayerCharacter.Orientation = finalEntry.Orientation;

			if (finalEntry == fallbackEntry)
				Logger.Log($"No entry named '{entryID}' found in the stage, using the first entry '{fallbackEntry.ID}' as a fallback.", Logger.LogTypeEnum.Warning);
		}
		else
		{
			Logger.Log($"No entry named '{entryID}' found in the stage, unable to place the player character.", Logger.LogTypeEnum.Error);
		}

		PlayerCharacter.Inventory.AddedThing += InterfaceNode.OnPlayerObjectAddedToInventory;
		PlayerCharacter.Inventory.RemovedThing += InterfaceNode.OnPlayerObjectRemovedFromInventory;
		PlayerCharacter.ThingPickedUp += OnCharacterPickedUpThing;
	}

	public void OnCharacterPickedUpThing(ThingResource thingResource)
	{
		StageThingManager.RemoveThing(thingResource.ID);
	}
}
