using Godot;
using System;

public partial class DialogOptionLabel : Label
{
	public void _OnMouseEnter()
	{
		AddThemeColorOverride("font_color", new Color("c686c5"));
	}

	public void _OnMouseExit()
	{
		RemoveThemeColorOverride("font_color");
	}
}
