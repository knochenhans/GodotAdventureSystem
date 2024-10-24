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

		var addHotspotButton = Dock.GetNode<Button>("VBoxContainer/AddHotspot");
		addHotspotButton.Pressed += OnAddHotspotButtonPressed;

		var addStageButton = Dock.GetNode<Button>("VBoxContainer/AddStage");
		addStageButton.Pressed += OnAddStageButtonPressed;

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
		if (EditedSceneRoot != null)
		{

			var nodes = EditedSceneRoot.GetChildren();
			var newName = name;

			foreach (var node in nodes)
			{
				var nodeName = node.Name.ToString();
				if (nodeName.StartsWith(newName))
				{
					newName += "1";
					return GetNewNodeName(newName);
				}
			}
			return newName;
		}
		return "";
	}

	public void OnAddStageButtonPressed()
	{
		// var stage = new Stage
		// {
		// 	Name = GetNewNodeName("Stage")
		// };

		// UndoRedoManager.CreateAction("Add Stage", customContext: EditedSceneRoot);
		// UndoRedoManager.AddDoMethod(this, MethodName.DoAddStageNode, stage);
		// UndoRedoManager.AddDoReference(stage);
		// UndoRedoManager.AddUndoMethod(this, MethodName.UndoAddStageNode, stage);
		// UndoRedoManager.CommitAction();

		// var editorSelection = EditorInterface.Singleton.GetSelection();
		// editorSelection.Clear();
		// editorSelection.AddNode(stage);

		// EditorInterface.Singleton.OpenSceneFromPath()

		// var newScene = ResourceLoader.Load<PackedScene>("res://addons/GodotAdventureSystem/Stage.tscn").Instantiate();

		// GD.Print(EditorInterface.Singleton.GetEditorMainScreen().GetChildren()[0].GetChildren());
		// GD.Print(EditorInterface.Singleton.GetEditedSceneRoot());

		// var baseControl = EditorInterface.Singleton.GetBaseControl();
		// var editorTitleBar = baseControl.FindChild("*EditorTitleBar*", true, false);
		// var scenePopup = editorTitleBar.FindChildren("*", "PopupMenu", true, false)[0] as PopupMenu;
		// var id = scenePopup.GetItemId(0);
		// scenePopup.EmitSignal("id_pressed", id);

		// var fileSystemDock = EditorInterface.Singleton.GetFileSystemDock();
		EditorInterface.Singleton.OpenSceneFromPath("res://addons/GodotAdventureSystem/Stage.tscn");
	}

	public void DoAddStageNode(Stage stage)
	{
		EditedSceneRoot.AddChild(stage);
	}

	public void UndoAddStageNode(Stage stage)
	{
		EditedSceneRoot.RemoveChild(stage);
		stage.QueueFree();
	}

	public void OnAddHotspotButtonPressed()
	{
		// We have to set this here as it's not available in _EnterTree for some reason
		EditedSceneRoot = EditorInterface.Singleton.GetEditedSceneRoot();
		if (EditedSceneRoot.IsInGroup("stage"))
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

			var editorSelection = EditorInterface.Singleton.GetSelection();
			editorSelection.Clear();
			editorSelection.AddNode(hotspot);
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
