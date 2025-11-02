using Garage.Controller;
using Garage.Interfaces;
using Garage.Manager;
using Garage.Utils;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Props
{
	public class TireProp : OwnableProp, IActionableProp
	{
		[SerializeField] protected float height;
		[SerializeField] private List<Material> materials = new();

		protected TireSize tireSize;
		public TireSize TireSize { get => tireSize; set => tireSize = value; }

        public override void Awake()
        {
            base.Awake();
            Init();
        }
        public override void Init()
        {
            base.Init();
        }

        protected override void StartInteraction(ulong newOwnerClientId)
		{
			base.StartInteraction(newOwnerClientId);
			transform.GetComponent<Rigidbody>().useGravity = false;
            rigid.isKinematic = true;
            transform.GetComponent<Collider>().isTrigger = true;
            SyncStateServerRPC(true);
		}

		public override void OnEndInteraction(Transform controller)
		{
			rigid.isKinematic = false;
			transform.position = controller.position + new Vector3(0, height * 1.2f, 0) + controller.forward * 1.5f;
			transform.rotation = Quaternion.LookRotation(controller.forward);
			GetComponent<Rigidbody>().linearVelocity = Vector3.zero;

			transform.GetComponent<Rigidbody>().useGravity = true;
			transform.GetComponent<Collider>().isTrigger = false;
			SyncStateServerRPC(false);

			base.OnEndInteraction(controller);
        }

		public virtual void Update()
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
				UpdatePropPositionServerRPC(transform.position, NetworkManager.Singleton.LocalClientId);
				UpdatePropRotateServerRPC(transform.rotation, NetworkManager.Singleton.LocalClientId);
				UpdatePropVelocityServerRPC(Vector3.zero, NetworkManager.Singleton.LocalClientId);
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

		public void SetTireSize(TireSize size)
		{
			SetTireSizeClientRPC(size);
		}

		[ClientRpc]
		private void SetTireSizeClientRPC(TireSize size)
		{
			this.tireSize = size;
			GetComponent<Renderer>().material = materials[(int)size];
		}

        public void OnStartPropAction(Transform controller)
		{
			Managers.Input.DisablePlayerMove();
            this.controller.ChargeTireRoll();
        }
        public void OnHoldingPropAction(Transform controller)
        {
            this.controller.ChargeTireRoll();
        }
        public void OnReleasedPropAction(Transform controller)
        {
            this.controller.SetAnimParam((int)AnimationType.Carry, false);
            this.controller.SetAnimParam((int)AnimationType.Place);

            base.OnEndInteraction(controller);
		}

		public void TireRolling(float rollForce)
		{
            rigid.isKinematic = false;

			Transform playerTf = controller.transform;
            float rollingForce = rollForce;

            transform.position = playerTf.position + new Vector3(0, height * 1.2f, 0) + playerTf.forward * 1.5f;
            transform.rotation = Quaternion.LookRotation(playerTf.forward);
            GetComponent<Rigidbody>().linearVelocity = (playerTf.forward * rollingForce);

            transform.GetComponent<Rigidbody>().useGravity = true;
            transform.GetComponent<Collider>().isTrigger = false;
            SyncStateServerRPC(false);
        }
	}
}
