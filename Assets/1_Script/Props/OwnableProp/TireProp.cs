using Garage.Controller;
using Garage.Utils;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace Garage.Props
{
	public class TireProp : OwnableProp
	{
		Rigidbody rigid;

		private void Awake()
		{
			rigid = GetComponent<Rigidbody>();
		}

		protected override void StartInteraction(ulong newOwnerClientId)
		{
			NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>().StartInteraction(this);

			transform.GetComponent<Rigidbody>().useGravity = false;
            rigid.isKinematic = true;
            transform.GetComponent<Collider>().isTrigger = true;
			SyncStateServerRPC(true);
		}

		[ServerRpc(RequireOwnership = false)]
		private void SyncStateServerRPC(bool isStart)
		{
			SyncStateClientRPC(isStart);
		}

		[ClientRpc]
		private void SyncStateClientRPC(bool isStart)
		{
			rigid.useGravity = !isStart;
			rigid.isKinematic = isStart;
			transform.GetComponent<Collider>().isTrigger = isStart;
		}

		protected override void OnEndInteraction(Transform controller)
		{
			rigid.isKinematic = false;
			transform.position = controller.position + new Vector3(0, height * 1.2f, 0) + controller.forward * 1.5f;
			transform.rotation = Quaternion.LookRotation(controller.forward);
			GetComponent<Rigidbody>().linearVelocity = (controller.forward * 10f);

			transform.GetComponent<Rigidbody>().useGravity = true;
			transform.GetComponent<Collider>().isTrigger = false;
			SyncStateServerRPC(false);

			base.OnEndInteraction(controller);
		}

		private void Update()
		{
			if (controller != null)
			{
				rigid.MovePosition(controller.GetSocket(PropType.Tire).position);
				rigid.MoveRotation(controller.GetSocket(PropType.Tire).rotation);
				return;
			}

			if (!IsOwner)
			{
				return;
			}
			else
			{
				UpdatePlayerPositionServerRPC(transform.position, NetworkManager.Singleton.LocalClientId);
				UpdatePlayerRotateServerRPC(transform.rotation, NetworkManager.Singleton.LocalClientId);
				UpdatePlayerVelocityServerRPC(Vector3.zero, NetworkManager.Singleton.LocalClientId);
			}
		}


		#region Transform RPC


		[ServerRpc(RequireOwnership = false)]
		public void UpdatePlayerVelocityServerRPC(Vector3 velocity, ulong clientId)
		{
			UpdatePlayerVelocityClientRPC(velocity ,clientId);
		}
		[ClientRpc]
		public void UpdatePlayerVelocityClientRPC(Vector3 velocity, ulong clientId)
		{
			if (clientId == NetworkManager.Singleton.LocalClientId) return;
			rigid.linearVelocity = velocity;
		}

		[ServerRpc(RequireOwnership = false)]
		public void UpdatePlayerPositionServerRPC(Vector3 playerPosition, ulong clientId)
		{
			UpdatePlayerPositionClientRPC(playerPosition, clientId);
		}

		[ClientRpc]
		private void UpdatePlayerPositionClientRPC(Vector3 playerPosition, ulong clientId)
		{
			if (clientId == NetworkManager.Singleton.LocalClientId) return;
			Debug.Log("MOVEPOS : " + playerPosition);
			rigid.MovePosition(playerPosition);
		}

		[ServerRpc(RequireOwnership = false)]
		private void UpdatePlayerRotateServerRPC(Quaternion playerQuat, ulong clientId)
		{
			UpdatePlayerRotateClientRPC(playerQuat, clientId);
		}

		[ClientRpc]
		private void UpdatePlayerRotateClientRPC(Quaternion playerQuat, ulong clientId)
		{
			if (clientId == NetworkManager.Singleton.LocalClientId) return;
			rigid.MoveRotation(playerQuat);
		}
		#endregion

	}
}
