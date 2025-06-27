using Garage.Controller;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Props
{
	public class OwnableProp : PropBase
	{
		private NetworkVariable<ulong> ownerClientId = new NetworkVariable<ulong>(ulong.MaxValue);
		public ulong OwnerClientId => ownerClientId.Value;
        protected PlayerController controller;
		public PlayerController Controller => controller;
        protected NetworkVariable<Vector3> gridPosition = new();

		[SerializeField] private MeshRenderer renderer;
		[SerializeField] private Color targetColor;

		[SerializeField, Tooltip("Determine carry this prop with two hand or not")]
		private bool isCarry;
		public bool IsCarry => isCarry;

		private bool isTargetted = false;

		private Material material;

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			ownerClientId.OnValueChanged += OnClientIDChanged;
			material = renderer.material;
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
		public void TryInteract(ulong clientId)
		{
			RequestOwnershipServerRpc(clientId);
		}

		[ServerRpc(RequireOwnership = false)]
		private void RequestOwnershipServerRpc(ulong requestingClientId)
		{
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
			NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>().OnInteractionGranted(this);
		}

		public virtual void OnEndInteraction(Transform transform)
		{
			RemoveOwnershipServerRpc();
		}

		public bool IsOwned()
		{
			return ownerClientId.Value != ulong.MaxValue;
		}
		public ulong OwnerClientID()
		{
			return ownerClientId.Value;
        }
		public virtual void OnTargetted()
		{
			if (material == null || isTargetted) return;

			isTargetted = true;
			material.SetColor("_Emissive_Color", targetColor);
		}

		public virtual void OnUntargetted()
		{
			if (material == null || !isTargetted) return;

			isTargetted = false;
			material.SetColor("_Emissive_Color", Color.black);
		}

		public void SetGridPosition(Vector3 pos)
		{
			transform.position = pos;
			gridPosition.Value = pos;
		}
	}
}