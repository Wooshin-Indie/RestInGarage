
using UnityEngine;

namespace Garage.Utils
{
	public static class Debugger
	{
		public static void DebugDrawBox(Vector3 center, Vector3 size, Quaternion rotation, Color color)
		{
			Vector3 halfSize = size * 0.5f;

			Vector3[] corners = new Vector3[8];
			for (int i = 0; i < 8; i++)
			{
				Vector3 corner = new Vector3(
					(i & 1) == 0 ? -halfSize.x : halfSize.x,
					(i & 2) == 0 ? -halfSize.y : halfSize.y,
					(i & 4) == 0 ? -halfSize.z : halfSize.z
				);
				corners[i] = center + rotation * corner;
			}

			Debug.DrawLine(corners[0], corners[1], color);
			Debug.DrawLine(corners[0], corners[2], color);
			Debug.DrawLine(corners[0], corners[4], color);
			Debug.DrawLine(corners[1], corners[3], color);
			Debug.DrawLine(corners[1], corners[5], color);
			Debug.DrawLine(corners[2], corners[3], color);
			Debug.DrawLine(corners[2], corners[6], color);
			Debug.DrawLine(corners[3], corners[7], color);
			Debug.DrawLine(corners[4], corners[5], color);
			Debug.DrawLine(corners[4], corners[6], color);
			Debug.DrawLine(corners[5], corners[7], color);
			Debug.DrawLine(corners[6], corners[7], color);
		}
	}
}
