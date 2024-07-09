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

	public Vector2 GetClosestPoint(Vector2 point)
	{
		var collisionPolygon2D = GetNode<CollisionPolygon2D>("CollisionPolygon2D");
		var nearestPoint = collisionPolygon2D.Polygon[0];
		var nearestDistance = (point - nearestPoint).Length();
		foreach (var polygonPoint in collisionPolygon2D.Polygon)
		{
			var distance = (point - polygonPoint).Length();
			if (distance < nearestDistance)
			{
				nearestPoint = polygonPoint;
				nearestDistance = distance;
			}
		}
		return nearestPoint;
	}
}
