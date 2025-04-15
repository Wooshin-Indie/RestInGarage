using Unity.Netcode;
using UnityEngine;

namespace Garage.Props
{
	public class OwnableProp : PropBase
	{
		private NetworkVariable<ulong> ownerClientId = new NetworkVariable<ulong>(ulong.MaxValue);

		[SerializeField] protected float height;

		/// <summary>
		/// 외부에서 Interact 할 때 호출하는 함수
		/// </summary>
		public void TryInteract()
		{
			RequestOwnershipServerRpc(NetworkManager.Singleton.LocalClientId);
		}

		/// <summary>
		/// Interact 끝낼 때 호출하는 함수
		/// </summary>
		public void EndInteraction(Transform transform)
		{
			RequestRemoveOwnershipServerRPC();
			OnEndInteraction(transform);
		}

		[ServerRpc(RequireOwnership = false)]
		private void RequestRemoveOwnershipServerRPC()
		{
			ownerClientId.Value = ulong.MaxValue;
		}

		[ServerRpc(RequireOwnership = false)]
		private void RequestOwnershipServerRpc(ulong requestingClientId)
		{
			Debug.Log(ownerClientId.Value + ", " + requestingClientId);
			if (ownerClientId.Value == ulong.MaxValue)
			{
				ownerClientId.Value = requestingClientId;
				GetComponent<NetworkObject>().ChangeOwnership(requestingClientId);
				GrantInteractionClientRPC(requestingClientId);
			}
			else
			{

			}
		}

		[ClientRpc]
		private void GrantInteractionClientRPC(ulong clientId)
		{
			if (clientId != NetworkManager.Singleton.LocalClientId) return;
			StartInteraction(clientId);
		}

		protected virtual void StartInteraction(ulong newOwnerClientId)
		{

		}

		protected virtual void OnEndInteraction(Transform transform)
		{

		}
	}
}