#if TOOLS
using Godot;
using System;

[Tool]
public partial class GodotAdventureSystem : EditorPlugin
{
	private Control Dock { get; set; }

	EditorUndoRedoManager UndoRedoManager;
	Node EditedSceneRoot { get; set; }


	public override void _EnterTree()
	{
		var hotspotScript = GD.Load<Script>("res://addons/GodotAdventureSystem/Hotspot.cs");
		AddCustomType("Hotspot", "Polygon2D", hotspotScript, null);

		var entryScript = GD.Load<Script>("res://addons/GodotAdventureSystem/Entry.cs");
		AddCustomType("Entry", "Marker2D", entryScript, null);

		Dock = GD.Load<PackedScene>("res://addons/GodotAdventureSystem/Dock.tscn").Instantiate<Control>();
		AddControlToDock(DockSlot.LeftUl, Dock);

		var addHotspotButton = Dock.GetNode<Button>("AddHotspot");
		addHotspotButton.Pressed += _OnAddHotspotButtonPressed;

		UndoRedoManager = GetUndoRedo();
	}

	public override void _ExitTree()
	{
		RemoveControlFromDocks(Dock);
		Dock.Free();

		RemoveCustomType("Entry");
		RemoveCustomType("Hotspot");
	}

	public string GetNewNodeName(string name)
	{
		var nodes = EditedSceneRoot.GetChildren();
		var newName = name;

		foreach (var node in nodes)
		{
			if (node.Name == newName)
			{
				newName += "1";
				return GetNewNodeName(newName);
			}
		}

		return newName;
	}

	public void _OnAddHotspotButtonPressed()
	{
		// We have to set this here as it's not available in _EnterTree for some reason
		EditedSceneRoot = EditorInterface.Singleton.GetEditedSceneRoot();
		GD.Print(EditedSceneRoot);
		if (EditedSceneRoot is Stage)
		{

			var hotspot = new Hotspot
			{
				Name = GetNewNodeName("Hotspot")
			};

			UndoRedoManager.CreateAction("Add Hotspot", customContext: EditedSceneRoot);
			UndoRedoManager.AddDoMethod(this, MethodName.DoAddHotspotNode, hotspot);
			UndoRedoManager.AddDoReference(hotspot);
			UndoRedoManager.AddUndoMethod(this, MethodName.UndoAddHotspotNode, hotspot);
			UndoRedoManager.CommitAction();
		}
		else
		{
			var window = new AcceptDialog
			{
				Title = "Error",
				DialogText = "This can only be added to a Stage node.",
			};
			EditorInterface.Singleton.PopupDialogCentered(window);
		}
	}

	public void DoAddHotspotNode(Hotspot hotspot)
	{
		EditedSceneRoot.AddChild(hotspot);
		hotspot.Owner = EditedSceneRoot;
	}

	public void UndoAddHotspotNode(Hotspot hotspot)
	{
		EditedSceneRoot.RemoveChild(hotspot);
		hotspot.QueueFree();
	}
}
#endif
