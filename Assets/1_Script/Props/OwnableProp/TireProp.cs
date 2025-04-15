using Garage.Controller;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Props
{
	public class TireProp : OwnableProp
	{
		[SerializeField]
		private Transform targetTrasnsform = null;

		Rigidbody rigid;
		protected override void StartInteraction(ulong newOwnerClientId)
		{
			targetTrasnsform = NetworkManager.Singleton.LocalClient.PlayerObject
				.GetComponent<PlayerController>().GetSocket(Utils.PropType.Tire, this);

			rigid = GetComponent<Rigidbody>();
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
			targetTrasnsform = null;


			base.OnEndInteraction(controller);
		}

		private void Update()
		{
			if (!IsOwner) return;
			if (targetTrasnsform == null) return;

			transform.position = targetTrasnsform.position;
			transform.rotation = targetTrasnsform.rotation;

			UpdatePlayerPositionServerRPC(transform.position);
			UpdatePlayerRotateServerRPC(transform.rotation);
			UpdatePlayerVelocityServerRPC(Vector3.zero);
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
			transform.position = (playerPosition);
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
			transform.rotation = (playerQuat);
		}
		#endregion

	}
}
