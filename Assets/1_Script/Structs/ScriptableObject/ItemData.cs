using UnityEngine;

namespace Garage.Structs
{
	// 단순하게 만들어뒀고,
	// 나중에 itemType, 설명 등 여러가지 추가가능
	[CreateAssetMenu(fileName = "Item Data", menuName = "SO/Item Data")]
	public class ItemData : ScriptableObject
	{
		[SerializeField] private int itemID;
		[SerializeField] private string itemName;
		[SerializeField] private int buyPrice;
		[SerializeField] private int sellPrice;

		public int ItemID => itemID;
		public string ItemName => itemName;
		public int BuyPrice => buyPrice;
		public int SellPrice => sellPrice;
	}
}
