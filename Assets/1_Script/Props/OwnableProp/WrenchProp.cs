using Garage.Controller;
using Garage.Interfaces;
using Garage.Manager;
using Garage.Utils;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Props
{
	public class WrenchProp : OwnableProp, IPlaceable
	{
		[SerializeField] private GameObject previewPrefab;

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
				controller.IsAbleToRun = false;
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
					UpdatePlayerVelocityServerRPC(Vector3.zero, NetworkManager.Singleton.LocalClientId);
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
	}
}
