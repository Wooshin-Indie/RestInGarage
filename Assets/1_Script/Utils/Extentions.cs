using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Garage.Utils
{
	public static class Extentions
	{
		public static float ManhatanDistance(this Vector3 a, Vector3 b)
		{
			return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z);
		}

		public static Tweener DOFontSize(this TextMeshProUGUI tmp, float targetSize, float duration)
		{
			float startSize = tmp.fontSize;
			return DOTween.To(() => tmp.fontSize, x => tmp.fontSize = x, targetSize, duration);
		}

		public static void SetPosition(this Rigidbody rigid, Vector3 position)
		{
			if (rigid == null)
			{
				Debug.LogError("rigid is null");
				return;
			}

			if (rigid.isKinematic)
			{
				rigid.transform.position = position;
			}
			else
			{
				rigid.position = position;
			}
		}
		public static void SetRotation(this Rigidbody rigid, Quaternion quat)
		{
			if (rigid == null)
			{
				Debug.LogError("rigid is null");
				return;
			}

			if (rigid.isKinematic)
			{
				rigid.transform.rotation = quat;
			}
			else
			{
				rigid.rotation = quat;
			}
		}
	}
}
