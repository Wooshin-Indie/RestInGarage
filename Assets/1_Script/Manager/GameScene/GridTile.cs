using Garage.Manager;
using Garage.Props;
using Unity.Netcode;
using UnityEngine;

namespace Garage
{
	public class GridTile : NetworkBehaviour
	{
		public Renderer rend;
		public NetworkVariable<NetworkObjectReference> PropNetRef = new();

		public OwnableProp prop => PropNetRef.Value.TryGet(out var obj) ? obj.GetComponent<OwnableProp>() : null;

		public NetworkVariable<Vector3Int> GridPosition = new();

		void Awake()
		{
			rend = GetComponent<Renderer>();
		}
		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			gameObject.SetActive(false);

			if (IsClient && !IsHost)
			{
				BuildingManager.Instance.RegisterTile(this);
			}
		}
		public void SetGridPosition(int idx, int row, int col)
		{
			GridPosition.Value.Set(idx, row, col);
		}
		public void SetMaterial(Material mat)
		{
			rend.material = mat;
		}
		public bool IsPlaceable(OwnableProp target)
		{
			return prop == null || prop == target;
		}

		public void SetProp(OwnableProp p)
		{
			if (IsServer)
			{
				PropNetRef.Value = p != null ? p.NetworkObject : default;
			}
		}
	}
}