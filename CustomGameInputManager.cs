using Godot;

public class CustomGameInputManager : BaseGameInputManager
{
    public CustomGameInputManager(BaseGame game, Camera2D camera) : base(game, camera) { }

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
        mousePosition = game.GetViewport().GetMousePosition();
        HoveredObject = obj;
        Logger.Log($"Mouse entered object: {obj.Name}", "CustomGameInputManager", Logger.LogTypeEnum.Input);
    }

    public override void OnMouseEntersEntity(Entity entity)
    {
        mousePosition = game.GetViewport().GetMousePosition();
        HoveredEntity = entity;
        Logger.Log($"Mouse entered entity: {entity.Name}", "CustomGameInputManager", Logger.LogTypeEnum.Input);
    }

    protected override void HoveredEntityClicked(InputEventMouseButton mouseButtonEvent)
    {
        if (HoveredEntity != null)
        {
            // Handle entity click logic here
        }
    }

    protected override void HoveredObjectClicked(InputEventMouseButton mouseButtonEvent)
    {
        if (HoveredObject != null)
        {
            // Handle object click logic here
        }
    }

    protected override void OnStageClicked(InputEventMouseButton mouseButtonEvent, bool shiftPressed, bool ctrlPressed = false)
    {
        if (mouseButtonEvent.IsPressed())
        {
            // Handle stage click logic here
        }
    }
}