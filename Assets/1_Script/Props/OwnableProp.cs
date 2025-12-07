using Garage.Controller;
using Garage.Interfaces;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.TextCore.Text;

namespace Garage.Props
{
	public class OwnableProp : PropBase, IKeyCodeDescription
    {
		private Material material;

		[SerializeField] private MeshRenderer meshRenderer;
		[SerializeField] private Color targetColor;

		[SerializeField, Tooltip("Determine carry this prop with two hand or not")]
		private bool isCarry;
		[SerializeField] private float carrySpeedMultiplier;

		private NetworkVariable<ulong> ownerClientId = new NetworkVariable<ulong>(ulong.MaxValue);
		protected PlayerController controller;
		public ulong OwnClientId => ownerClientId.Value;
		public PlayerController Controller => controller;

        protected NetworkVariable<Vector3> gridPosition = new();

		public bool IsCarry => isCarry;
		public float CarrySpeedMultiplier => carrySpeedMultiplier;

		private bool isTargetted = false;


        public virtual void Init()
        {
			InitKeyDescriptions();
        }

        public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			ownerClientId.OnValueChanged += OnClientIDChanged;
			material = meshRenderer.material;
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
            //GetComponent<NetworkObject>().RemoveOwnership();
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

			foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
			{
				var playerObj = client.PlayerObject;
				if (playerObj != null && playerObj.OwnerClientId == newOwnerClientId)
				{
					controller = playerObj.GetComponent<PlayerController>();
				}
			}

			if(controller == null)
			{
				Debug.LogError("[OwnableProp] - controller is null");
				return;
			}
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

        public void InitKeyDescriptions()
        {
			ItemData.InitKeyDataMaps();
        }
    }
}