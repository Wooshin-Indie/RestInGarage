using Garage.Utils;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Props
{
	public class TireRack : OwnableProp
	{
		[SerializeField] private GameObject tirePrefab;

		protected override void StartInteraction(ulong newOwnerClientId)
		{
			base.StartInteraction(newOwnerClientId);

			SpawnTireServerRpc(newOwnerClientId);
			OnEndInteraction(null);
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
	}
}
