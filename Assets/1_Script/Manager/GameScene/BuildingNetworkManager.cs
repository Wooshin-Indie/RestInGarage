using Garage.Props;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Manager
{
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
		public void TryPlaceServerRpc(ulong propNetId, int gridIdx, int wheelRotate, Vector2Int[] tileIndices)
		{
			NetworkObject obj = NetworkManager.SpawnManager.SpawnedObjects[propNetId];
			OwnableProp prop = obj.GetComponent<OwnableProp>();

			bool success = true;

			foreach (var index in tileIndices)
			{
				if (!BuildingManager.Instance.IsInBounds(index)) { success = false; break; }
				if (!BuildingManager.Instance.GridTiles[gridIdx][index.x, index.y].IsPlaceable(prop)) { success = false; break; }
			}

			if (!success)
			{
				TryPlaceResultClientRpc(false, propNetId, Vector3.zero, 0);
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
			foreach (var index in tileIndices)
			{
				BuildingManager.Instance.GridTiles[gridIdx][index.x, index.y].SetProp(prop);
			}

			Vector3 position = BuildingManager.Instance.GetCenterWorldPosition(gridIdx, tileIndices);
			int rotation = wheelRotate;

			prop.transform.position = position;
			prop.transform.rotation = Quaternion.Euler(0f, rotation * 90f, 0f);

			TryPlaceResultClientRpc(true, propNetId, position, rotation);
		}

		[ClientRpc]
		private void TryPlaceResultClientRpc(bool success, ulong propNetId, Vector3 pos, int rotation)
		{
			if (IsHost) return;

			if (!success)
			{
				Debug.Log("설치 실패");
				return;
			}

			var obj = NetworkManager.SpawnManager.SpawnedObjects[propNetId];
			var prop = obj.GetComponent<OwnableProp>();

			prop.transform.position = pos;
			prop.transform.rotation = Quaternion.Euler(0f, rotation * 90f, 0f);
		}
	}
}
