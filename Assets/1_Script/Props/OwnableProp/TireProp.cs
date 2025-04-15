using Garage.Controller;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Props
{
	public class TireProp : OwnableProp
	{
		[SerializeField]
		private Transform targetTrasnsform = null;
		protected override void StartInteraction(ulong newOwnerClientId)
		{
			targetTrasnsform = NetworkManager.Singleton.LocalClient.PlayerObject
				.GetComponent<PlayerController>().GetSocket(Utils.PropType.Tire, this);

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
		}
	}
}
