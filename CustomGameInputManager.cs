using Godot;

public class CustomGameInputManager : BaseGameInputManager
{
    CustomGame customGame => (CustomGame)base.game;

    public CustomGameInputManager(CustomGame game, Camera2D camera) : base(game, camera) { }

    public override void HandleGlobalInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButtonEvent)
        {
            if (HoveredObject == null)
                OnStageClicked(mouseButtonEvent, false, false);

            if (mouseButtonEvent.IsPressed())
            {
                switch (mouseButtonEvent.ButtonIndex)
                {
                    case MouseButton.Left:
                        // UISoundPlayer.Instance.PlaySound("click1");
                        break;
                }

                if (HoveredObject != null)
                    HoveredObjectClicked(mouseButtonEvent);

                if (HoveredEntity != null)
                    HoveredEntityClicked(mouseButtonEvent);
            }
        }
    }

    public override void OnMouseEntersObject(Object obj)
    {
        viewportMousePosition = customGame.GetViewport().GetMousePosition();
        HoveredObject = obj;
        customGame.InterfaceNode.SetCommandLabel(obj.DisplayName);
    }

    public override void OnMouseExitsObject(Object obj)
    {
        viewportMousePosition = customGame.GetViewport().GetMousePosition();
        HoveredObject = null;
        customGame.InterfaceNode.SetCommandLabel("");
    }

    public override void OnMouseEntersEntity(Entity entity)
    {
        viewportMousePosition = customGame.GetViewport().GetMousePosition();
        HoveredEntity = entity;
        // customGame.InterfaceNode.SetCommandLabel(entity.DisplayName);

        customGame.OnStageNodeHovered(HoveredEntity);
    }

    public override void OnMouseExitsEntity(Entity entity)
    {
        viewportMousePosition = customGame.GetViewport().GetMousePosition();
        HoveredEntity = null;
        customGame.InterfaceNode.SetCommandLabel("");
    }

    protected override void HoveredEntityClicked(InputEventMouseButton mouseButtonEvent)
    {
        if (HoveredEntity != null)
        {
            customGame.OnStageNodeClicked(HoveredEntity, viewportMousePosition);
        }
    }

    protected override void HoveredObjectClicked(InputEventMouseButton mouseButtonEvent)
    {
        if (HoveredObject != null)
        {
            customGame.OnStageNodeClicked(HoveredObject, viewportMousePosition);
        }
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
}