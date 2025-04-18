using Garage.Interfaces;
using Garage.Manager;
using Garage.Utils;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Props
{
	public class OilPump : OwnableProp, IPlaceable
	{
		[SerializeField] private Vector3 initPos;
		[SerializeField] private Vector3 initRot;

		[SerializeField] private Transform rope;
		[SerializeField] private Transform oilgun;

		private Rigidbody gunRigid;
		private RaycastHit[] hits;

		public override void Awake()
		{
			gunRigid = oilgun.GetComponent<Rigidbody>();
			hits = new RaycastHit[5];
		}

		protected override void StartInteraction(ulong newOwnerClientId)
		{
			base.StartInteraction(newOwnerClientId);
		}

		protected override void OnEndInteraction(Transform controller)
		{
			base.OnEndInteraction(controller);
		}

		private void Update()
		{
			if (GameManagerEx.Instance.IsDay)
			{
				if (controller != null)
				{
					gunRigid.MovePosition(controller.GetSocket(PropType.Oilgun).position);
					gunRigid.MoveRotation(controller.GetSocket(PropType.Oilgun).rotation);
				}
				else
				{
					oilgun.localPosition = (initPos);
					oilgun.localRotation = (Quaternion.Euler(initRot));
				}

				CheckObstacle();
			}
			else
			{
				oilgun.localPosition = (initPos);
				oilgun.localRotation = (Quaternion.Euler(initRot));
			}
		}

		private void CheckObstacle()
		{
			Vector3 start = rope.position;
			Vector3 end = oilgun.position;

			Ray ray = new Ray(start, (end - start).normalized);
			int count = Physics.RaycastNonAlloc(ray, hits, Vector3.Distance(start, end));

			for(int i=0; i<count; i++)
			{
				GameObject hitObj = hits[i].collider.gameObject;

				if (hitObj == gameObject) continue;
				if (hitObj == oilgun.gameObject) continue;
				if (hitObj.CompareTag(Constants.TAG_PLAYER) && hitObj.GetComponent<NetworkObject>().IsLocalPlayer) continue;

				Debug.Log("막힘!");
				return;
			}

		}

		Vector2Int IPlaceable.GetSize()
		{
			return new Vector2Int(2, 2);
		}
	}
}
