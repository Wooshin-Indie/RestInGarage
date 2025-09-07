using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

namespace Garage.Structs
{
    public enum ItemType
    {
        None = 0,   // Tire같은 설치되지 않는 Prop들
        Rack,
        OilPump,
        Wrench,
        Extinguisher
    }

    // 수치화되는 특징들을 나타냅니다.
	[System.Serializable]
	public class ItemFeature
	{
		// HACK - 이거 FeatureName대신 Enum으로 바꿔야될듯
		public string FeatureName = "";
		public float FeatureValue = 0f;
		public bool IsPositiveValue = false;
		public bool IsPositiveFeature = false;
	}

    // 수치화 되지않는 특징들을 나타냅니다.
    // ex. IsAbleToRun
    [System.Serializable]
    public class StringFeature
    {
        public string FeatureName = "";
        public bool IsPositiveFeature = false;
    }

    [System.Serializable]
    public class UpgradeData
    {
		public int buyPrice;
		public int sellPrice;
		public int upgradePrice;
        public float progressMult;
		public List<ItemFeature> features = new();
    }

    [System.Serializable]
    public class KeyData
    {
        // 인스펙터에서 'Interact', 'Hold' 등의 InputAction을 직접 연결할 수 있습니다.
        public InputActionReference Action;
        public LocalizedString LocalizedDescription; // 키에 대한 설명 (예: "들기", "놓기")

        // 런타임에 사용할 바인딩된 키 이름 (예: "E")
        [HideInInspector] public string KeyDisplayName;
    }

    // 단순하게 만들어뒀고,
    // 나중에 itemType, 설명 등 여러가지 추가가능
    [CreateAssetMenu(fileName = "Item Data", menuName = "SO/Item Data")]
	public class ItemData : ScriptableObject
	{
		[SerializeField] private bool isRevealData = true;

		[SerializeField] private int itemID;
        [SerializeField] private ItemType itemType;
		[SerializeField] private string itemName;
		[SerializeField] private string descriptionKey;
		[SerializeField] private List<UpgradeData> upgradeDatas = new();
        [SerializeField] private List<StringFeature> stringFeatures = new();

        [SerializeField] private List<KeyData> idleKeyDataList;
        [SerializeField] private List<KeyData> carryKeyDataList;
        [SerializeField] private List<KeyData> interactKeyDataList;
        [SerializeField] private List<KeyData> carryNightKeyDataList;
        private Dictionary<string, KeyData> idleKeyDataMap;
        private Dictionary<string, KeyData> carryKeyDataMap;
        private Dictionary<string, KeyData> interactKeyDataMap;
        private Dictionary<string, KeyData> carryNightKeyDataMap;
        public void InitKeyDataMaps()
        {
            InitKeyDataMap(idleKeyDataList, idleKeyDataMap);
            InitKeyDataMap(carryKeyDataList, carryKeyDataMap);
            InitKeyDataMap(interactKeyDataList, interactKeyDataMap);
            InitKeyDataMap(carryNightKeyDataList, carryNightKeyDataMap);
        }
        private void InitKeyDataMap(List<KeyData> keyList, Dictionary<string, KeyData> keyDataMap)
        {
            keyDataMap = new Dictionary<string, KeyData>();

            foreach (var data in keyList)
            {
                if (data.Action == null) continue;

                // InputAction의 이름을 Key로 사용하여 딕셔너리에 추가
                // 예: "Interact", "Rotate"
                string actionName = data.Action.action.name;
                if (!keyDataMap.ContainsKey(actionName))
                {
                    // 바인딩된 키의 표시 이름을 가져와서 저장
                    // GetBindingDisplayString()은 바인딩된 키를 "E", "LMB" 등으로 보기 좋게 반환합니다.
                    data.KeyDisplayName = data.Action.action.GetBindingDisplayString();
                    keyDataMap.Add(actionName, data);
                }
            }
        }

        public KeyData GetCarryKeyData(InputAction action)
        {
            if (action == null) return null;

            return GetCarryKeyData(action.name);
        }
        public KeyData GetCarryKeyData(string actionName)
        {
            carryKeyDataMap.TryGetValue(actionName, out var data);
            return data;
        }

        public int GetBuyPrice(int upgrade)
        {
            if (upgradeDatas.Count <= upgrade)
            {
                return 0;
            }
            return upgradeDatas[upgrade].buyPrice;
		}
		public int GetSellPrice(int upgrade)
		{
			if (upgradeDatas.Count <= upgrade)
			{
				return 0;
			}
			return upgradeDatas[upgrade].sellPrice;
		}
		public List<ItemFeature> GetItemFeatures(int upgrade)
		{
			if (upgradeDatas.Count <= upgrade)
			{
				return null;
			}
			return upgradeDatas[upgrade].features;
		}

        public List<UpgradeData> UpgradeDatas => upgradeDatas;
        public List<StringFeature> StringFeatures => stringFeatures;
		public bool IsRevealData => isRevealData;
		public int ItemID => itemID;
        public ItemType ItemType => itemType;
		public string ItemName => itemName;
		public string DescriptionKey => descriptionKey;
        public List<KeyData> IdleKeyDataList => idleKeyDataList;
        public List<KeyData> CarryKeyDataList => carryKeyDataList;
        public List<KeyData> InteractKeyDataList => interactKeyDataList;
        public List<KeyData> CarryNightKeyDataList => carryNightKeyDataList;
    }
}
