using Garage.Interfaces;
using Garage.Manager;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Props
{
	public class TireRack : OwnableProp, IPlaceable
	{
		[SerializeField] private GameObject tirePrefab;

		protected override void StartInteraction(ulong newOwnerClientId)
		{
			base.StartInteraction(newOwnerClientId);


			if (GameManagerEx.Instance.IsDay)
			{
				SpawnTireServerRpc(newOwnerClientId);
				OnEndInteraction(null);
			}
		}

		[ServerRpc(RequireOwnership = false)]
		private void SpawnTireServerRpc(ulong newOwnerClientId)
		{
			GameObject go = Instantiate(tirePrefab, NetworkManager.Singleton.ConnectedClients[newOwnerClientId].PlayerObject.transform.position, Quaternion.identity);
			NetworkObject networkObject = go.GetComponent<NetworkObject>();
			networkObject.Spawn();
			go.GetComponent<TireProp>().TryInteract(newOwnerClientId);
		}

		protected override void OnEndInteraction(Transform controller)
		{
			base.OnEndInteraction(controller);
		}

		private void Update()
		{

		}

		public Vector2Int GetSize()
		{
			return new Vector2Int(2, 4);
		}
	}
}
