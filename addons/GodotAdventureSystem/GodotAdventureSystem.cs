#if TOOLS
using Godot;
using System;

[Tool]
public partial class GodotAdventureSystem : EditorPlugin
{
	EditorUndoRedoManager UndoRedoManager;
	Node EditedSceneRoot { get; set; }
	Control DockNode = GD.Load<PackedScene>("res://addons/GodotAdventureSystem/Dock.tscn").Instantiate<Control>();

	Script HotspotScript = GD.Load<Script>("res://addons/GodotAdventureSystem/Hotspot.cs");
	Script EntryScript = GD.Load<Script>("res://addons/GodotAdventureSystem/Entry.cs");
	Script ExitScript = GD.Load<Script>("res://addons/GodotAdventureSystem/Exit.cs");
	Script StageBackgroundScript = GD.Load<Script>("res://addons/GodotAdventureSystem/StageBackground.cs");

	public override void _EnterTree()
	{
		AddCustomType("Hotspot", "Polygon2D", HotspotScript, null);
		AddCustomType("Entry", "Marker2D", EntryScript, null);
		AddCustomType("Exit", "Polygon2D", HotspotScript, null);
		AddCustomType("StageBackground", "TextureRect", StageBackgroundScript, null);

		Button AddHotspotButton = DockNode.GetNode<Button>("VBoxContainer/AddHotspot");
		Button AddStageButton = DockNode.GetNode<Button>("VBoxContainer/AddStage");
		Button AddEntryButton = DockNode.GetNode<Button>("VBoxContainer/AddEntry");
		Button AddExitButton = DockNode.GetNode<Button>("VBoxContainer/AddExit");
		Button AddStageBackgroundButton = DockNode.GetNode<Button>("VBoxContainer/AddBackground");

		AddControlToDock(DockSlot.LeftUl, DockNode);

		AddHotspotButton.Pressed += OnAddHotspotButtonPressed;
		AddStageButton.Pressed += OnAddStageButtonPressed;
		AddEntryButton.Pressed += OnAddEntryButtonPressed;
		AddExitButton.Pressed += OnAddExitButtonPressed;
		AddStageBackgroundButton.Pressed += OnAddBackgroundButtonPressed;

		UndoRedoManager = GetUndoRedo();
	}

	public override void _ExitTree()
	{
		RemoveControlFromDocks(DockNode);
		DockNode.Free();

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
		AddNodeToStage<Hotspot>("Hotspot", DoAddHotspotNode, UndoAddHotspotNode);
	}

	public void OnAddEntryButtonPressed()
	{
		AddNodeToStage<Entry>("Entry", DoAddEntryNode, UndoAddEntryNode);
	}

	public void OnAddExitButtonPressed()
	{
		AddNodeToStage<Exit>("Exit", DoAddExitNode, UndoAddExitNode);
	}

	public void OnAddBackgroundButtonPressed()
	{
		AddNodeToStage<StageBackground>("StageBackground", DoStageAddBackgroundNode, UndoStageAddBackgroundNode);
	}

	private void AddNodeToStage<T>(string nodeName, Action<T> doMethod, Action<T> undoMethod) where T : Node, new()
	{
		EditedSceneRoot = EditorInterface.Singleton.GetEditedSceneRoot();
		if (EditedSceneRoot.IsInGroup("stage"))
		{
			var node = new T
			{
				Name = GetNewNodeName(nodeName)
			};

			UndoRedoManager.CreateAction($"Add {nodeName}", customContext: EditedSceneRoot);
			UndoRedoManager.AddDoMethod(this, doMethod.Method.Name, node);
			UndoRedoManager.AddDoReference(node);
			UndoRedoManager.AddUndoMethod(this, undoMethod.Method.Name, node);
			UndoRedoManager.CommitAction();

			var editorSelection = EditorInterface.Singleton.GetSelection();
			editorSelection.Clear();
			editorSelection.AddNode(node);
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

		hotspot.Polygon = new Vector2[]
		{
			new(0, 0),
			new(0, 32),
			new(32, 32),
			new(32, 0),
		};

		hotspot.Owner = EditedSceneRoot;
	}

	public void UndoAddHotspotNode(Hotspot hotspot) => RemoveNodeFromStage(hotspot);

	public void DoAddEntryNode(Entry entry) => AddNodeToStage(entry);

	public void UndoAddEntryNode(Entry entry) => RemoveNodeFromStage(entry);

    public void DoAddExitNode(Exit exit)
    {
        EditedSceneRoot.AddChild(exit);

		exit.Polygon = new Vector2[]
		{
			new(0, 0),
			new(0, 32),
			new(32, 32),
			new(32, 0),
		};

		exit.Owner = EditedSceneRoot;
    }

    public void UndoAddExitNode(Exit exit) => RemoveNodeFromStage(exit);

	public void DoStageAddBackgroundNode(StageBackground background) => AddNodeToStage(background);

	public void UndoStageAddBackgroundNode(StageBackground background) => RemoveNodeFromStage(background);

	private void AddNodeToStage(Node node)
	{
		EditedSceneRoot.AddChild(node);
		node.Owner = EditedSceneRoot;
	}

	private void RemoveNodeFromStage(Node node)
	{
		EditedSceneRoot.RemoveChild(node);
		node.QueueFree();
	}
}
#endif
