using Garage.Utils;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Props
{
	public class TireProp : OwnableProp
	{

		[SerializeField] protected float height;
		protected override void StartInteraction(ulong newOwnerClientId)
		{
			base.StartInteraction(newOwnerClientId);
			transform.GetComponent<Rigidbody>().useGravity = false;
            rigid.isKinematic = true;
            transform.GetComponent<Collider>().isTrigger = true;
			SyncStateServerRPC(true);
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
	}
}
