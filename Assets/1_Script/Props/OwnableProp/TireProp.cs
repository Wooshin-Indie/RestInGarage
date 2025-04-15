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

			transform.GetComponent<Collider>().isTrigger = true;
		}

		protected override void OnEndInteraction(Transform controller)
		{
			base.OnEndInteraction(controller);

			
			transform.position = controller.position + new Vector3(0, height * 1.2f, 0) + controller.forward * 1.5f;
			transform.rotation = Quaternion.LookRotation(controller.forward);
			GetComponent<Rigidbody>().linearVelocity = (controller.forward * 10f);

			transform.GetComponent<Collider>().isTrigger = false;
			targetTrasnsform = null;
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
