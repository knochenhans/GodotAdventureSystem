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
        viewportMousePosition = customGame.GetViewport().GetMousePosition();
        HoveredStageNode = stageNode;
        customGame.InterfaceNode.SetCommandLabel(stageNode.DisplayName);

        // Logger.Log($"Mouse entered stage node: {stageNode.ID}", "CustomGameInputManager", Logger.LogTypeEnum.Input);
    }

    public override void OnMouseExitsStageNode(StageNode stageNode)
    {
        viewportMousePosition = customGame.GetViewport().GetMousePosition();
        HoveredStageNode = null;
        customGame.InterfaceNode.SetCommandLabel("");

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

    private bool InputActive()
    {
        return customGame.CurrentCommandState != CustomGame.CommandStateEnum.Dialog &&
               CustomGame.GetCurrentAdventureStage().ScriptManager.CurrentRunningState != ScriptManager.RunningStateEnum.Running;
    }
}