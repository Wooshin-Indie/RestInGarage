using Garage.Props;
using Garage.UI.Item;
using Garage.Utils;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.UIElements;
using UnityEngine;

namespace Garage.Manager
{

	public enum BuildFailType
	{
		WrongPlace,
		NoMoney,
	}

	public class BuildingNetworkManager : NetworkBehaviour
	{
		#region Singleton
		private static BuildingNetworkManager instance;
		public static BuildingNetworkManager Instance { get => instance; }

		void Awake()
		{
			Init();
		}

		private void Init()
		{
			if (null == instance)
			{
				instance = this;
				DontDestroyOnLoad(this.gameObject);
			}
			else
			{
				Destroy(this.gameObject);
			}
		}
		#endregion

		[ServerRpc(RequireOwnership = false)]
		public void TryPlaceServerRpc(ulong propNetId, int gridIdx, int wheelRotate, Vector2Int[] tileIndices, ulong clientId)
		{
			// TODO - 위치에 따라서 살지 팔지 
			if (BuildingManager.Instance.ItemDictionary.TryGetValue(propNetId, out OwnableProp oProp))
			{
				if (!EconomyManager.Instance.UseMoney_HostOnly(oProp.ItemData.BuyPrice))
				{
					FailToPlaceClientRPC(BuildFailType.NoMoney, clientId);
					return;
				}
				BuildingManager.Instance.PlacedBuildings.Add(propNetId, oProp);
				BuildingManager.Instance.ItemDictionary.Remove(propNetId);
			}

			NetworkObject obj = NetworkManager.SpawnManager.SpawnedObjects[propNetId];
			OwnableProp prop = obj.GetComponent<OwnableProp>();

			bool success = true;

			foreach (var index in tileIndices)
			{
				if (!BuildingManager.Instance.IsInBounds(gridIdx, index)) { success = false; break; }
				if (!BuildingManager.Instance.GridTiles[gridIdx][index.x, index.y].IsPlaceable(prop)) { success = false; break; }
			}

			if (!success)
			{
				FailToPlaceClientRPC(BuildFailType.WrongPlace, clientId);
				return;
			}

			for (int t = 0; t < BuildingManager.Instance.GridTiles.Count; t++)
			{
				for (int i = 0; i < BuildingManager.Instance.GridTiles[t].GetLength(0); i++)
				{
					for (int j = 0; j < BuildingManager.Instance.GridTiles[t].GetLength(1); j++)
					{
						if (BuildingManager.Instance.GridTiles[t][i, j].PropNetRef.Value.NetworkObjectId == propNetId)
							BuildingManager.Instance.GridTiles[t][i, j].SetProp(null);
					}
				}
			}

			BuildingManager.Instance.OnBuyItem(propNetId);
			OnShopItemBuyedClientRPC(propNetId);

			if (gridIdx == BuildingManager.Instance.GridTiles.Count - 1)
			{
				EconomyManager.Instance.EarnMoney_HostOnly(prop.ItemData.SellPrice);
				OwnableProp tmpProp = null;
				if (BuildingManager.Instance.PlacedBuildings.TryGetValue(propNetId, out tmpProp))
				{
					BuildingManager.Instance.PlacedBuildings.Remove(propNetId);

					tmpProp.GetComponent<NetworkObject>().Despawn();
					Destroy(tmpProp.gameObject);
				}

				if (BuildingManager.Instance.ItemDictionary.TryGetValue(propNetId, out tmpProp))
				{
					BuildingManager.Instance.ItemDictionary.Remove(propNetId);

					tmpProp.GetComponent<NetworkObject>().Despawn();
					Destroy(tmpProp.gameObject);
				}
			}
			else
			{
				foreach (var index in tileIndices)
				{
					BuildingManager.Instance.GridTiles[gridIdx][index.x, index.y].SetProp(prop);
				}

				Vector3 position = BuildingManager.Instance.GetCenterWorldPosition(gridIdx, tileIndices);
				int rotation = wheelRotate;

				prop.SetGridPosition(position);
				prop.GetComponent<Rigidbody>().SetRotation(Quaternion.Euler(0f, rotation * 90f, 0f));

				TryPlaceResultClientRpc(propNetId, position, rotation);
			}
		}


		[ClientRpc]
		private void FailToPlaceClientRPC(BuildFailType type, ulong clientId)
		{
			if (clientId != NetworkManager.Singleton.LocalClientId) return;

			SoundManager.Instance.PlaySfx(SFXType.Wrong, .7f, 1f);
			switch (type)
			{
				case BuildFailType.WrongPlace:
					break;
				case BuildFailType.NoMoney:
					UIManager.Game.OnInsufficientBalance();
					break;
			}
		}

		[ClientRpc]
		private void TryPlaceResultClientRpc(ulong propNetId, Vector3 pos, int rotation)
		{
			if (IsHost) return;

			var obj = NetworkManager.SpawnManager.SpawnedObjects[propNetId];
			var prop = obj.GetComponent<OwnableProp>();

			prop.GetComponent<Rigidbody>().SetPosition(pos);
			prop.GetComponent<Rigidbody>().SetRotation(Quaternion.Euler(0f, rotation * 90f, 0f));
		}


		[SerializeField] private GameObject priceTextPrefab;
		private Dictionary<ulong, ItemPriceText> priceTexts = new();
		
		[ClientRpc]
		public void OnShopItemRevealedClientRPC(Vector3 position, ulong netId, int buyPrice)
		{
			ItemPriceText pt = Instantiate(priceTextPrefab).GetComponent<ItemPriceText>();	
			pt.SetItemPrice(position, buyPrice);
			priceTexts.Add(netId, pt);
		}

		[ClientRpc]
		public void OnShopItemBuyedClientRPC(ulong netId)
		{
			if (priceTexts.TryGetValue(netId, out ItemPriceText pt) && pt != null)
			{
				Destroy(pt.gameObject);
			}
			priceTexts.Remove(netId);
		}

		[ClientRpc]
		public void OnShopItemEraseAllClientRPC()
		{
			foreach(var item in priceTexts)
			{
				if (item.Value.gameObject != null)
					Destroy(item.Value.gameObject);
			}
			priceTexts.Clear();
		}
	}
}
