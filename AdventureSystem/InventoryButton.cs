using Godot;
using System;

public partial class InventoryButton : Button
{
	TextureRect TextureRect { get; set; }

	public override void _Ready()
	{
		TextureRect = GetNode<TextureRect>("%TextureRect");

		SetMeta("thingID", "");
	}

	public void SetThing(string thingID, Texture2D texture)
	{
		var image = texture.GetImage();
		Vector2 zoom = GetViewport().GetCamera2D().Zoom;
		image.Resize(image.GetWidth() * (int)zoom.X, image.GetHeight() * (int)zoom.Y, Image.Interpolation.Nearest);
		TextureRect.Texture = ImageTexture.CreateFromImage(image);

		SetMeta("thingID", thingID);
	}

	public void Clear()
	{
		TextureRect.Texture = null;
		SetMeta("thingID", "");
	}
}
