using Godot;
using System;

public partial class DialogOptionLabel : Label
{
	public void OnMouseEnter()
	{
		AddThemeColorOverride("font_color", new Color("c686c5"));
	}

	public void OnMouseExit()
	{
		RemoveThemeColorOverride("font_color");
	}
}
