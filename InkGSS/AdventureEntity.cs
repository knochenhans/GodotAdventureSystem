using System;
using System.Threading.Tasks;
using Godot;

public partial class AdventureEntity : Entity
{
    public async Task SpeechBubble(string message)
    {
        GD.Print($"Entity {Name} says: {message}");
        await Task.Delay(1000); // Simulate delay
    }

    public async Task MoveTo(Vector2 position, int v, bool isRelative)
    {
        GD.Print($"Entity {Name} moves to: {position}");
        await Task.Delay(1000); // Simulate delay
    }

    public async Task PlayAnimation(string animationName)
    {
        GD.Print($"Entity {Name} plays animation: {animationName}");
        await Task.Delay(1000); // Simulate delay
    }
}
