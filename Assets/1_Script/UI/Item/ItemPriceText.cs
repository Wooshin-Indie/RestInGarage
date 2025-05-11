using Garage.Utils;
using TMPro;
using UnityEngine;

namespace Garage.UI.Item
{
	public class ItemPriceText : MonoBehaviour
	{
		private Vector3 targetPosition = Vector3.zero;
		private RectTransform rect;
		private TextMeshProUGUI tmp;

		private void Awake()
		{
			rect = GetComponent<RectTransform>();
			tmp = GetComponent<TextMeshProUGUI>();
		}

		public void SetItemPrice(Vector3 position, int price)
		{
			targetPosition = position;
			GetComponent<TextMeshProUGUI>().text = price.ToString();
		}

		private float fontSize = 1000f;

		private void Update()
		{
			Vector3 anchorPos = Camera.main.WorldToScreenPoint(targetPosition);
			transform.position = new Vector3(anchorPos.x, anchorPos.y, 0);

			float manhatan = Mathf.Abs(Camera.main.transform.position.y - targetPosition.y) + Mathf.Abs(Camera.main.transform.position.z - targetPosition.z);
			tmp.fontSize = fontSize / manhatan;
		}
	}
}
