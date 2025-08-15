using Godot;

public class CustomGameInputManager : GameInputManager
{
    public CustomGameInputManager(Game game, Camera2D camera, TileMapManager tileMapManager) : base(game, camera, tileMapManager)
    {

    }

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
}