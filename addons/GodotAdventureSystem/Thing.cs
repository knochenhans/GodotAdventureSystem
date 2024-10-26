using Godot;

public partial class Thing : Area2D
{
	[Export] public Resource Resource;

	AnimatedSprite2D Sprite => GetNode<AnimatedSprite2D>("AnimatedSprite2D");

	public override void _Ready()
	{
		var texture = (Resource as ThingResource).Texture;
		if (texture != null)
		{
			Sprite.SpriteFrames = new();
			Sprite.SpriteFrames.AddFrame("default", texture);
		}
	}
}