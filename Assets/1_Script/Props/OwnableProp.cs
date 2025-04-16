using Garage.Controller;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Props
{
	public class OwnableProp : PropBase
	{
		private NetworkVariable<ulong> ownerClientId = new NetworkVariable<ulong>(ulong.MaxValue);
		protected PlayerController controller;

		[SerializeField, Tooltip("Determine carry this prop with two hand or not")]
		private bool isCarry;
		public bool IsCarry => isCarry;

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			ownerClientId.OnValueChanged += OnClientIDChanged;
		}

		private void OnClientIDChanged(ulong prev, ulong clientId)
		{
			if (clientId == ulong.MaxValue)
			{
				controller = null;
				return;
			}

			controller = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerController>();
		}

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
		[ServerRpc(RequireOwnership = false)]
		private void RemoveOwnershipServerRpc()
		{
			ownerClientId.Value = ulong.MaxValue;
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
			RemoveOwnershipServerRpc();
		}
	}
}