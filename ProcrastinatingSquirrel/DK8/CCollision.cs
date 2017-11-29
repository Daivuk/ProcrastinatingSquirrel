using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
//using Microsoft.Xna.Framework.Net;
//using Microsoft.Xna.Framework.Storage;
using DK8;

namespace DK8
{
	class CCollision
	{
		public static bool CircleToAABBIntersection(
			ref Vector2 rectCenter, ref Vector2 rectSize, ref Vector2 p1,
			ref Vector2 p2, float radius, 
			out Vector2 normal, out Vector2 intersectionPoint)
		{
			normal = Vector2.UnitX;
			intersectionPoint = Vector2.Zero;

			return false;
		}

		public static bool CircleToAABBTest(ref Vector2 tileCenter, ref Vector2 tileSize, ref Vector2 circle, float radius)
		{
			// Find the closest point to the circle within the rectangle
			float closestX = MathHelper.Clamp(circle.X, tileCenter.X - tileSize.X, tileCenter.X + tileSize.X);
			float closestY = MathHelper.Clamp(circle.Y, tileCenter.Y - tileSize.Y, tileCenter.Y + tileSize.Y);

			// Calculate the distance between the circle's center and this closest point
			float distanceX = circle.X - closestX;
			float distanceY = circle.Y - closestY;

			// If the distance is less than the circle's radius, an intersection occurs
			float distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);
			return distanceSquared < (radius * radius);
		}
	}
}
