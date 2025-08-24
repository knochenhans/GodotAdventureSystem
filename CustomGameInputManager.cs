using Godot;

public class CustomGameInputManager : BaseGameInputManager
{
    CustomGame customGame => (CustomGame)base.game;

    public CustomGameInputManager(CustomGame game, Camera2D camera) : base(game, camera) { }

    public override void HandleGlobalInput(InputEvent @event)
    {
        if (!InputActive())
            return;

        if (@event is InputEventMouseButton mouseButtonEvent)
        {
            if (HoveredStageNode == null)
                OnStageClicked(mouseButtonEvent, false, false);

            if (mouseButtonEvent.IsPressed())
            {
                switch (mouseButtonEvent.ButtonIndex)
                {
                    case MouseButton.Left:
                        // UISoundPlayer.Instance.PlaySound("click1");
                        break;
                }

                if (HoveredStageNode != null)
                    HoveredStageNodeClicked(mouseButtonEvent);
            }
        }
    }

    public override void OnMouseEntersStageNode(StageNode stageNode)
    {
        if (customGame.CurrentCommandState == CustomGame.CommandStateEnum.Dialog)
            return;

        if (stageNode == null)
            return;

        viewportMousePosition = customGame.GetViewport().GetMousePosition();

        HoveredStageNode = stageNode;

        if (stageNode == customGame.PlayerEntity)
            return;

        customGame.InterfaceNode.SetCommandLabel(stageNode.DisplayName);
        // var thing = CurrentStage.StageThingManager.GetThing(thingID);
        // ThingResource thingResource;

        // if (thing == null)
        //     thingResource = CurrentStage.PlayerCharacter.Inventory.FindThing(thingID);
        // else
        //     thingResource = thing.Resource as ThingResource;

        var stageNodeName = stageNode.DisplayName;

        if (customGame.CurrentCommandState == CustomGame.CommandStateEnum.Idle)
            customGame.InterfaceNode.SetCommandLabel(stageNodeName);
        else if (customGame.CurrentCommandState == CustomGame.CommandStateEnum.VerbSelected)
            customGame.InterfaceNode.SetCommandLabel($"{customGame.Verbs[customGame.currentVerbID]} {stageNodeName}");

        // Logger.Log($"Mouse entered stage node: {stageNode.ID}", "CustomGameInputManager", Logger.LogTypeEnum.Input);
    }

    public override void OnMouseExitsStageNode(StageNode stageNode)
    {
        viewportMousePosition = customGame.GetViewport().GetMousePosition();
        HoveredStageNode = null;

        if (customGame.CurrentCommandState == CustomGame.CommandStateEnum.Idle)
            customGame.InterfaceNode.ResetCommandLabel();
        else if (customGame.CurrentCommandState == CustomGame.CommandStateEnum.VerbSelected)
            customGame.InterfaceNode.SetCommandLabel($"{customGame.Verbs[customGame.currentVerbID]}");

        // Logger.Log($"Mouse exited stage node: {stageNode.ID}", "CustomGameInputManager", Logger.LogTypeEnum.Input);
    }

    protected async override void HoveredStageNodeClicked(InputEventMouseButton mouseButtonEvent)
    {
        if (HoveredStageNode == null)
            return;

        var clickedStageNode = HoveredStageNode;

        // var thing = CurrentStage.StageThingManager.GetThing(thingID);
        // ThingResource thingResource;

        // if (thing == null)
        // {
        //     thingResource = CurrentStage.PlayerCharacter.Inventory.FindThing(thingID);
        // }
        // else
        // {
        //     if (thing is HotspotArea hotspotArea)
        //     {
        //         await CurrentStage.PlayerCharacter.MoveTo(mousePosition / Camera2DNode.Zoom + Camera2DNode.Position);
        //     }
        //     else
        //     {
        //         thingResource = thing.Resource as ThingResource;
        if (clickedStageNode != null && clickedStageNode != customGame.PlayerEntity)
            await customGame.MovePlayerToStageNode(clickedStageNode);
        //     }
        // }

        await customGame.PerformVerbAction(clickedStageNode.ID);
    }

    protected override void OnStageClicked(InputEventMouseButton mouseButtonEvent, bool shiftPressed, bool ctrlPressed = false)
    {
        if (!customGame.stageLimits.HasPoint(mouseButtonEvent.Position))
            return;

        if (mouseButtonEvent.IsPressed() && mouseButtonEvent.ButtonIndex == MouseButton.Left)
        {
            (customGame.GetEntity("player").Moveable as MoveableNavigation).MovementTarget = camera.GetGlobalMousePosition();
        }
    }

    public void OnVerbHovered(string verbID)
    {
        // if (CurrentCommandState == CommandStateEnum.Idle)
        //     InterfaceNode.SetCommandLabel(Verbs[verbID]);
    }

    public void OnVerbLeave()
    {
        // if (CurrentCommandState == CommandStateEnum.Idle)
        //     InterfaceNode.ResetCommandLabel();
    }

    public void OnVerbClicked(string verbID)
    {
        // Logger.Log($"_OnVerbActivated: Verb: {verbID} activated", Logger.LogTypeEnum.Script);

        customGame.InterfaceNode.SetCommandLabel(customGame.Verbs[verbID]);
        customGame.currentVerbID = verbID;
        customGame.CurrentCommandState = CustomGame.CommandStateEnum.VerbSelected;
    }

    public void OnGamePanelMouseMotion()
    {
        // if (CurrentCommandState == CommandStateEnum.Idle)
        //     InterfaceNode.ResetCommandLabel();
        // else
        // if (customGame.CurrentCommandState == CustomGame.CommandStateEnum.VerbSelected)
        //     customGame.InterfaceNode.SetCommandLabel(customGame.Verbs[customGame.currentVerbID]);
    }

    public async void OnGamePanelMousePressed(InputEventMouseButton mouseButtonEvent)
    {
        if (customGame.CurrentCommandState == CustomGame.CommandStateEnum.Idle)
        {
            // await CurrentStage.PlayerCharacter.MoveTo(mouseButtonEvent.Position / Camera2DNode.Zoom + Camera2DNode.Position, 1);
        }
        else if (customGame.CurrentCommandState == CustomGame.CommandStateEnum.VerbSelected)
        {
            if (mouseButtonEvent.ButtonIndex == MouseButton.Right)
                customGame.CurrentCommandState = CustomGame.CommandStateEnum.Idle;
        }
    }

    private bool InputActive()
    {
        return customGame.CurrentCommandState != CustomGame.CommandStateEnum.Dialog &&
               CustomGame.GetCurrentAdventureStage().ScriptManager.CurrentRunningState != ScriptManager.RunningStateEnum.Running;
    }
}