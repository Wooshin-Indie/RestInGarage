using Garage.Controller;
using Garage.Utils;
using Unity.Netcode;
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
			transform.GetComponent<Rigidbody>().useGravity = !isStart;
			transform.GetComponent<Collider>().isTrigger = isStart;
		}

		protected override void OnEndInteraction(Transform controller)
		{

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
				transform.position = controller.GetSocket(PropType.Tire).position;
				transform.rotation = controller.GetSocket(PropType.Tire).rotation;
				return;
			}

			if (!IsOwner)
			{
				return;
			}
			else
			{
				UpdatePlayerPositionServerRPC(transform.position);
				UpdatePlayerRotateServerRPC(transform.rotation);
				UpdatePlayerVelocityServerRPC(Vector3.zero);
			}
		}


		#region Transform RPC


		[ServerRpc(RequireOwnership = false)]
		public void UpdatePlayerVelocityServerRPC(Vector3 velocity)
		{
			UpdatePlayerVelocityClientRPC(velocity);
		}
		[ClientRpc]
		public void UpdatePlayerVelocityClientRPC(Vector3 velocity)
		{
			if (IsOwner) return;
			rigid.linearVelocity = velocity;
		}

		[ServerRpc(RequireOwnership = false)]
		public void UpdatePlayerPositionServerRPC(Vector3 playerPosition)
		{
			UpdatePlayerPositionClientRPC(playerPosition);
		}

		[ClientRpc]
		private void UpdatePlayerPositionClientRPC(Vector3 playerPosition)
		{
			if (IsOwner) return;
			rigid.MovePosition(playerPosition);
		}

		[ServerRpc(RequireOwnership = false)]
		private void UpdatePlayerRotateServerRPC(Quaternion playerQuat)
		{
			UpdatePlayerRotateClientRPC(playerQuat);
		}

		[ClientRpc]
		private void UpdatePlayerRotateClientRPC(Quaternion playerQuat)
		{
			if (IsOwner) return;
			rigid.MoveRotation(playerQuat);
		}
		#endregion

	}
}
