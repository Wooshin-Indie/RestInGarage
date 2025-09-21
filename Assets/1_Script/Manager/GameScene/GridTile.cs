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
        public NetworkVariable<GridType> gridType = new();
        public NetworkVariable<Vector2Int> GridPosition = new();

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			BuildingManager.Instance.RegisterTileOnClient(this);
			gameObject.SetActive(false);
		}

		void Awake()
		{
			rend = GetComponent<Renderer>();
		}
		public void InitGridTile(GridType type, Vector2Int pos)
		{
			GridPosition.Value = pos;
            gridType.Value = type;
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