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

	// public void SetThing(ThingResource thingResource)
	// {
	// 	if (thingResource != null)
	// 	{
	// 		var texture = thingResource.Texture;

	// 		if (texture != null)
	// 		{
	// 			var image = thingResource.Texture.GetImage();
	// 			Vector2 zoom = GetViewport().GetCamera2D().Zoom;
	// 			image.Resize(image.GetWidth() * (int)zoom.X, image.GetHeight() * (int)zoom.Y, Image.Interpolation.Nearest);
	// 			TextureRect.Texture = ImageTexture.CreateFromImage(image);

	// 			SetMeta("thingID", thingResource.ID);
	// 		}
	// 		else
	// 		{
	// 			Logger.Log($"InventoryButton: No texture found for thing {thingResource.ID}", Logger.LogTypeEnum.Error);
	// 			return;
	// 		}
	// 	}
	// 	else
	// 	{
	// 		TextureRect.Texture = null;
	// 		SetMeta("thingID", "");
	// 	}
	// }

	public void Clear()
	{
		TextureRect.Texture = null;
		SetMeta("thingID", "");
	}
}
