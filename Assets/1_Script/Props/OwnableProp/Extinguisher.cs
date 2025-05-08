using Garage.Interfaces;
using Garage.Manager;
using Garage.Utils;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Props
{
	public class Extinguisher : OwnableProp, IActionable, IPlaceable
	{

		[SerializeField] private GameObject previewPrefab;
		[SerializeField] protected float height;


		protected override void StartInteraction(ulong newOwnerClientId)
		{
			base.StartInteraction(newOwnerClientId);
			if (GameManagerEx.Instance.IsDay)
			{
				transform.GetComponent<Rigidbody>().useGravity = false;
				rigid.isKinematic = true;
				transform.GetComponent<Collider>().isTrigger = true;
				SyncStateServerRPC(true);
			}
		}

		public override void OnEndInteraction(Transform controller)
		{
			rigid.isKinematic = false;
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
					rigid.MovePosition(controller.GetSocket(PropType.Extinguisher).position);
					rigid.MoveRotation(controller.GetSocket(PropType.Extinguisher).rotation);
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
				transform.position = gridPosition.Value;
				transform.rotation = Quaternion.identity;
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

		public void OnPropAction(Transform controller)
		{
			// controller forward 방향으로 소화기 발사
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
