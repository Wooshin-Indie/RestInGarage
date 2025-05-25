#if UNITY_EDITOR
using UnityEditor;
#endif
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



		public static void DrawCapsuleGizmo(Transform transform, Vector3 p1, Vector3 p2, float radius)
		{
#if UNITY_EDITOR
			Handles.color = Color.cyan;

			Vector3 up = (p2 - p1).normalized;
			float height = Vector3.Distance(p1, p2);
			float halfHeight = Mathf.Max(0f, height / 2f - radius);

			// 중심 방향 계산
			Vector3 center = (p1 + p2) / 2f;
			Quaternion rotation = Quaternion.LookRotation(up);

			// 반구 시각화
			Handles.DrawWireArc(p1, Vector3.right, Vector3.up, 360, radius);
			Handles.DrawWireArc(p2, Vector3.right, Vector3.up, 360, radius);
			Handles.DrawWireDisc(p1, up, radius);
			Handles.DrawWireDisc(p2, up, radius);

			// 원기둥 측면 연결
			Vector3 right = Vector3.Cross(up, Vector3.up).normalized * radius;
			Vector3 forward = Vector3.Cross(right, up).normalized * radius;

			Handles.DrawLine(p1 + right, p2 + right);
			Handles.DrawLine(p1 - right, p2 - right);
			Handles.DrawLine(p1 + forward, p2 + forward);
			Handles.DrawLine(p1 - forward, p2 - forward);
#endif
		}
	}
}
