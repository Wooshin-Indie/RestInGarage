using Garage.Controller;
using Garage.Manager;
using Garage.Utils;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Props
{
	public class TireProp : OwnableProp
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
			isTireRolling = false;
            SyncStateServerRPC(true);
		}

		public override void OnEndInteraction(Transform controller)
		{
			rigid.isKinematic = false;
			transform.position = controller.position + new Vector3(0, height * 1.2f, 0) + controller.forward * 1.5f;
			transform.rotation = Quaternion.LookRotation(controller.forward);

			transform.GetComponent<Rigidbody>().useGravity = true;
			transform.GetComponent<Collider>().isTrigger = false;
			SyncStateServerRPC(false);

			base.OnEndInteraction(controller);
        }

		public virtual void Update()
		{
			if (controller != null && !isTireRolling)
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
            this.controller.OnUpdatePlayerGage();
        }
        public void OnHoldingPropAction(Transform controller)
        {
            this.controller.OnUpdatePlayerGage();
        }
        public void OnReleasedPropAction(Transform controller)
        {

        }
        public virtual void OnAnimationKeyPropAction(Transform controller)
        {
            TireRolling(controller.GetComponent<PlayerController>().GetTireRollingForce());
            controller.GetComponent<PlayerController>().TryEndInteractWithProp();

            base.OnEndInteraction(controller);
        }

		private bool isTireRolling = false;
        public void TireRolling(float rollForce)
        {
            rigid.isKinematic = false;
			isTireRolling = true;

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
