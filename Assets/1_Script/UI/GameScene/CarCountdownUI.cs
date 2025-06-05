
using UnityEngine;
using UnityEngine.UI;

namespace Garage.UI.Item
{
	public class CarCountdownUI : MonoBehaviour
	{
		[SerializeField] private Image fillImage;

		public void SetAmount(float amount)
		{
			fillImage.fillAmount = amount;
		}

		public void SetPosition(Vector3 pos)
		{
			Vector3 screenPos = pos;
			Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

			RectTransform rt = GetComponent<RectTransform>();
			Vector2 uiSize = rt.rect.size * rt.lossyScale;

			Vector2 margin = uiSize / 2f;

			if (screenPos.x >= margin.x && screenPos.x <= Screen.width - margin.x &&
				screenPos.y >= margin.y && screenPos.y <= Screen.height - margin.y)
			{
				transform.position = screenPos;
			}
			else
			{
				Vector2 dir = ((Vector2)screenPos - screenCenter).normalized;

				float maxX = Screen.width - margin.x;
				float maxY = Screen.height - margin.y;

				float t = float.MaxValue;

				if (dir.x != 0)
				{
					float tx = (dir.x > 0) ? (maxX - screenCenter.x) / dir.x : (margin.x - screenCenter.x) / dir.x;
					t = Mathf.Min(t, tx);
				}

				if (dir.y != 0)
				{
					float ty = (dir.y > 0) ? (maxY - screenCenter.y) / dir.y : (margin.y - screenCenter.y) / dir.y;
					t = Mathf.Min(t, ty);
				}

				Vector2 edgePos = screenCenter + dir * t;
				transform.position = edgePos;
			}
		}

		public void CreateUI()
		{

		}

		public void EraseUI()
		{
			
		}
	}
}
