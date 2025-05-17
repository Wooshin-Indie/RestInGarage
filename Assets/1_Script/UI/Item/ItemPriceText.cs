using TMPro;
using UnityEngine;

namespace Garage.UI.Item
{
	public class ItemPriceText : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI tmp;

		private void Awake()
		{
		}

		public void SetItemPrice(Vector3 position, int price)
		{
			transform.position = position;
			tmp.text = $"$ {price.ToString()}";
		}
	}
}
