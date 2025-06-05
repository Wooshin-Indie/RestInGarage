using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization.Components;

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

		public static void SetLocalizedString(this TextMeshProUGUI tmp, string table, string key)
		{
			tmp.GetComponent<LocalizeStringEvent>().StringReference = new UnityEngine.Localization.LocalizedString
			{
				TableReference = table,
				TableEntryReference = key
			};
		}

		public static int GetRandomValue(this Vector2Int v)
		{
			return UnityEngine.Random.Range(v.x, v.y + 1);
		}
		public static float GetRandomValue(this Vector2 v)
		{
			return UnityEngine.Random.Range(v.x, v.y);
		}

		public static bool IsBetween(this Vector2 v, float value)
		{
			float min;
			float max;
			if (v.x <= v.y)
            {
				min = v.x;
                max = v.y;
            }
			else
            {
                min = v.y;
                max = v.x;
            }

			if (min <= value && value <= max)
			{
				return true;
			}
			else
				return false;
        }

		public static float GetCloserValue(this Vector2 v, float value)
		{
			if (Mathf.Abs(value - v.x) >= Mathf.Abs(value - v.y))
			{
				return v.x;
			}
			else
				return v.y;
        }
	}
}
