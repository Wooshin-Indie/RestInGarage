using Garage.Controller;
using Garage.Utils;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Props
{
	public class OilPump : OwnableProp
	{
		[SerializeField] private Transform oilgunSocket;
		[SerializeField] private Transform oilgun;

		private Rigidbody gunRigid;

		public override void Awake()
		{
			gunRigid = oilgun.GetComponent<Rigidbody>();
		}

		protected override void StartInteraction(ulong newOwnerClientId)
		{
			NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>().StartInteraction(this);
		}

		protected override void OnEndInteraction(Transform controller)
		{
			base.OnEndInteraction(controller);
		}

		private void Update()
		{
			if (controller != null)
			{
				gunRigid.MovePosition(controller.GetSocket(PropType.Oilgun).position);
				gunRigid.MoveRotation(controller.GetSocket(PropType.Oilgun).rotation);
				return;
			}
		}
	}
}
