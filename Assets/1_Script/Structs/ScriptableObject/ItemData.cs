using System.Collections.Generic;
using UnityEngine;

namespace Garage.Structs
{
	[System.Serializable]
	public class ItemFeature
	{
		// HACK - 이거 FeatureName대신 Enum으로 바꿔야될듯
		public string FeatureName = "";
		public float FeatureValue = 0f;
		public bool IsPositiveValue = false;
		public bool IsPositiveFeature = false;
	}

	// 단순하게 만들어뒀고,
	// 나중에 itemType, 설명 등 여러가지 추가가능
	[CreateAssetMenu(fileName = "Item Data", menuName = "SO/Item Data")]
	public class ItemData : ScriptableObject
	{
		[SerializeField] private bool isRevealData = true;

		[SerializeField] private int itemID;
		[SerializeField] private string itemName;
		[SerializeField] private int buyPrice;
		[SerializeField] private int sellPrice;

		[SerializeField] private string descriptionKey;
		[SerializeField] private List<ItemFeature> itemFeatures = new();

		public bool IsRevealData => isRevealData;
		public int ItemID => itemID;
		public string ItemName => itemName;
		public int BuyPrice => buyPrice;
		public int SellPrice => sellPrice;
		public string DescriptionKey => descriptionKey;
		public List<ItemFeature> ItemFeatures => itemFeatures;
	}
}
