public partial class Menu : Scene
{
    public void OnStartButtonPressed() => SceneManagerNode.ChangeToScene("Game");
    public void OnOptionsButtonPressed() => SceneManagerNode.ChangeToScene("Options");
    public void OnExitButtonPressed() => SceneManagerNode.ChangeToScene("Credits");
}
