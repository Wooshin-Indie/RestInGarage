using Garage.Controller;
using Garage.Interfaces;
using Garage.Manager;
using Garage.Utils;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Props
{
	public class WrenchProp : OwnableProp, IPlaceable, IActionable
	{
		[SerializeField] private GameObject previewPrefab;
		[SerializeField] private bool isAbleToRun;
		[SerializeField] private AnimationType animType;

		public AnimationType AnimType => animType;

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

			if (GameManagerEx.Instance.IsDay)
			{
				controller.IsAbleToRun = this.isAbleToRun;
				transform.GetComponent<Rigidbody>().useGravity = false;
				rigid.isKinematic = true;
				transform.GetComponent<Collider>().isTrigger = true;
				SyncStateServerRPC(true);
			}
		}

		public override void OnEndInteraction(Transform controller)
		{
			rigid.isKinematic = false;

            controller.GetComponent<PlayerController>().IsAbleToRun = true;
            transform.GetComponent<Rigidbody>().useGravity = true;
			transform.GetComponent<Collider>().isTrigger = false;
			SyncStateServerRPC(false);

			base.OnEndInteraction(controller);
		}

		private void Update()
		{
			if (GameManagerEx.Instance.IsDay)
			{
				if (controller != null)
				{
					rigid.MovePosition(controller.GetSocket(PropType.Wrench).position);
					rigid.MoveRotation(controller.GetSocket(PropType.Wrench).rotation);
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
			else
			{
				rigid.MovePosition(gridPosition.Value);
				rigid.MoveRotation(Quaternion.identity);
				rigid.linearVelocity = Vector3.zero;
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

		public Vector2Int GetSize()
		{
			return Vector2Int.one;
		}

		public GameObject GetPreviewPrefab()
		{
			return previewPrefab;
		}

		public void OnStartPropAction(Transform controller)
		{

		}

		public void OnStopPropAction(Transform controller)
		{
			OnEndInteraction(controller);
			ThrowWrench(controller);
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (!IsHost) return;
            if (!collision.gameObject.CompareTag(Constants.TAG_PLAYER)) return;
			// HACK - temp param
			if (rigid.linearVelocity.sqrMagnitude < 10f) return;
			if (controller != null) return;

			// TODO - 필요시 플레이어가 맞는 부분에 VFX 생성
			// VFXManager.Instance.PlayVFX(~, collision.GetContact(0).point, ~);
			Vector3 knockbackDirection = Vector3.ProjectOnPlane(collision.transform.position - transform.position, Vector3.up);
			collision.gameObject.GetComponent<PlayerController>().KnockBackClientRPC(knockbackDirection, rigid.mass);
		}

		private void ThrowWrench(Transform controller)
		{
			PlayerController pc = controller.GetComponent<PlayerController>();
			float rollingForce = pc.GetTireRollingForce();

			///	-- NOTE --
			/// 해머의 특성 (무게중심이나 질량) 때문에
			/// 던질 때 플레이어를 밀치거나 회전이 이상하게 되는 경우가 발생
			/// Rotation, Position을 플레이어와 겹치지 않도록 조정하고 각속도도 원하는대로 회전시킴
			/// ----------

			rigid.MoveRotation(Quaternion.identity);
			rigid.MovePosition(transform.position + (controller.up + controller.forward) * 1f);
			rigid.linearVelocity = ((controller.up + controller.forward) * rollingForce * 0.3f);
			rigid.angularVelocity = transform.up * 10f;
		}
	}
}
