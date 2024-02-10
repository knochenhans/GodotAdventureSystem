using Godot;
using System;

public partial class HotspotArea : Thing
{
	public Vector2 CalculateCenter()
	{
		var collisionPolygon2D = GetNode<CollisionPolygon2D>("CollisionPolygon2D");
		var center = new Vector2();
		foreach (var point in collisionPolygon2D.Polygon)
			center += point;
		center /= collisionPolygon2D.Polygon.Length;
		return center;
	}
}
