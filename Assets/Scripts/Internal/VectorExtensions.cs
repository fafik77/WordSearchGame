using System;

namespace UnityEngine
{
	public static class VectorExtensions
	{
		public static Vector2 Abs(this Vector2 vector)
		{
			return new Vector2(Math.Abs(vector.x), Math.Abs(vector.y));
		}
		public static Vector3 Abs(this Vector3 vector)
		{
			return new Vector3(Math.Abs(vector.x), Math.Abs(vector.y), Math.Abs(vector.z));
		}
	}
}
