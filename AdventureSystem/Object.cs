using Godot;
using System;

public partial class Object : Thing
{
    public Texture2D GetTexture()
    {
        var animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        return animatedSprite2D.SpriteFrames.GetFrameTexture(animatedSprite2D.Animation, animatedSprite2D.Frame);
    }
}
