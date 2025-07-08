using DG.Tweening;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
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
			if (Mathf.Abs(value - v.x) <= Mathf.Abs(value - v.y))
			{
				return v.x;
			}
			else
				return v.y;
        }

        public static KeyCode? GetFirstKeyboardBinding(this InputAction action)
        {
            if (action == null) return null;

            // action.bindings는 해당 액션에 연결된 모든 바인딩의 배열입니다.
            foreach (var binding in action.bindings)
            {
                // binding.path는 "<Keyboard>/e", "<Gamepad>/buttonSouth"와 같은 문자열입니다.
                // 키보드 바인딩인지 확인합니다.
                if (binding.path != null && binding.path.StartsWith("<Keyboard>"))
                {
                    // "<Keyboard>/".Length 만큼 잘라내어 키 이름("e")만 추출합니다.
                    string keyName = binding.path.Substring("<Keyboard>/".Length);

                    // 키 이름을 KeyCode enum으로 변환합니다. (대소문자 무시)
                    if (Enum.TryParse<KeyCode>(keyName, true, out var keyCode))
                    {
                        return keyCode;
                    }
                }
            }

            return null;
        }
    }
}
