using UnityEngine;

namespace Garage.Utils
{
	public static class Extentions
	{
		public static float ManhatanDistance(this Vector3 a, Vector3 b)
		{
			return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z);
		}
	}
}
