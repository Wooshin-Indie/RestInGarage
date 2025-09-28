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

		[Header("Extinguish Params")]
		[SerializeField] private ParticleSystem fireExPS;
		[SerializeField] private float extinguishDistance;
		[SerializeField] private float extinguishRadius;


		public float ExDistance => extinguishDistance;
		public float ExRadius => extinguishRadius;

		private NetworkVariable<bool> IsAction = new(
			false,
			NetworkVariableReadPermission.Everyone,
			NetworkVariableWritePermission.Owner
		);

		public override void Awake()
		{
			base.Awake();
			Init();
			fireExPS.Stop();

			IsAction.OnValueChanged -= OnActionChanged;
			IsAction.OnValueChanged += OnActionChanged;
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
				transform.GetComponent<Rigidbody>().useGravity = false;
				rigid.isKinematic = true;
				transform.GetComponent<Collider>().isTrigger = true;
				SyncStateServerRPC(true);

				fireExPS.startSpeed = extinguishDistance;
			}
		}

		public override void OnEndInteraction(Transform controller)
		{
			rigid.isKinematic = false;
			transform.GetComponent<Rigidbody>().useGravity = true;
			transform.GetComponent<Collider>().isTrigger = false;
			SyncStateServerRPC(false);

			if (IsAction.Value)
			{
				IsAction.Value = false;
			}
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

					if (IsAction.Value)
					{
						fireExPS.transform.rotation = controller.transform.rotation;
						controller.ExtinguishFire(transform.position);
					}

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

		public void OnStartPropAction(Transform controller)
		{
			if (!IsOwner)
			{
				Debug.LogWarning("You are not prop's owner"); 
				return;
			}

			IsAction.Value = true;
		}

		public void OnStopPropAction(Transform controller)
		{
			if (!IsOwner)
			{
				Debug.LogWarning("You are not prop's owner");
				return;
			}
			IsAction.Value = false;
		}

		private void OnActionChanged(bool prev, bool isAction)
		{
			if (prev == isAction) return;

            if (isAction)
            {
				fireExPS.Play();
            }
			else
			{
				fireExPS.Stop();
			}
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
