using Garage.Utils;
using Garage.Structs;
using IUtil;
using UnityEngine;
using Garage.Manager;
using System.Collections.Generic;
using Unity.Netcode;

namespace Garage.Controller
{
	public class CarController : NetworkBehaviour
	{
        [Header("Car Parts Transform")]
        [SerializeField] public List<Transform> PartTransforms = new List<Transform>(); // 넣을 때 CarParts enum 순서 맞춰서 넣기

        [FoldoutGroup("Move Parameters")]
		[SerializeField] private float moveSpeed = 5f;
		[SerializeField] private float stopDistance = 15f;
		[SerializeField] private float tmpDistance = 7f;
		[SerializeField] private float steeringStrength;
		[SerializeField] private float maxSteerAngle;
		[SerializeField] private float laneSnapThreshold;
		[SerializeField] private bool isBroken = true;

		[FoldoutGroup("Overlap Parameters")]
		[SerializeField] private float boxLength = 10f;
		[SerializeField] private float boxWidth = 1f;
		[SerializeField] private float boxHeight = 1f;
		[SerializeField] private LayerMask obstacleLayer;

		private float targetLaneX = 0f;
		private bool isBypassing = false;
		private Rigidbody rigid;
		private Collider[] hitResults = new Collider[10];
		private CarStatus carStatus;
		public CarStatus CarStatus { get => carStatus; }

        private void Awake()
		{
			rigid = GetComponent<Rigidbody>();
			carStatus = new CarStatus();
		}

		private void FixedUpdate()
		{
			if (!IsHost) return;

			if (isBroken)
			{
				if ((direction == VehicleDirection.Up && transform.position.z > 0) ||
					(direction == VehicleDirection.Down && transform.position.z < 0))
				{
					StopVehicle();
					return;
				}
				if (IsObstacleAhead(out float distance) && distance < stopDistance)
				{
					StopVehicle();
					return;
				}
			}
			else
			{
				if (IsObstacleAhead(out float distance))
				{
					if (!isBypassing)
					{
						isBypassing = true;
						targetLaneX += 5;
					}
					else
					{
						if (distance < tmpDistance)
						{
							StopVehicle();
							return;
						}
					}
				}
			}

			MoveForward();
		}

		private void MoveForward()
		{
			Vector3 pos = rigid.position;
			float xOffset = targetLaneX - pos.x;

			Quaternion targetRot;

			if (Mathf.Abs(xOffset) > laneSnapThreshold)
			{
				float steerAmount = Mathf.Clamp(xOffset * steeringStrength, -maxSteerAngle, maxSteerAngle);

				if (direction == VehicleDirection.Up)
				{
					targetRot = Quaternion.Euler(0f, steerAmount, 0f);
				}
				else
				{
					targetRot = Quaternion.Euler(0f, 180f - steerAmount, 0f);
				}
			}
			else
			{
				targetRot = Quaternion.Euler(0f, direction == VehicleDirection.Up ? 0f : 180f, 0f);

				pos.x = targetLaneX;
				rigid.position = pos;
			}

			rigid.MoveRotation(Quaternion.Slerp(rigid.rotation, targetRot, Time.fixedDeltaTime * 2f));
			rigid.linearVelocity = rigid.rotation * Vector3.forward * moveSpeed;
		}

		private void StopVehicle()
		{
			rigid.linearVelocity = Vector3.zero;
		}


		private bool IsObstacleAhead(out float hitDistance)
		{
			Vector3 boxCenter = transform.position + Vector3.up * (boxHeight * 0.5f) + transform.forward * (boxLength * 0.5f);
			Vector3 halfExtents = new Vector3(boxWidth * 0.5f, boxHeight * 0.5f, boxLength * 0.5f);
			Quaternion orientation = transform.rotation;

			int hitCount = Physics.OverlapBoxNonAlloc(boxCenter, halfExtents, hitResults, orientation, obstacleLayer);

			if (hitCount > 0)
			{
				float closestDist = float.MaxValue;

				for (int i = 0; i < hitCount; i++)
				{
					if (hitResults[i].transform.IsChildOf(transform)) continue;

					float dist = Vector3.Distance(transform.position, hitResults[i].ClosestPoint(transform.position));
					if (dist < closestDist)
						closestDist = dist;
				}

				if (closestDist < float.MaxValue)
				{
					hitDistance = closestDist;
					return true;
				}
			}

			hitDistance = Mathf.Infinity;
			return false;
		}

		private VehicleDirection direction = VehicleDirection.None;
		public VehicleDirection Direction { get => direction; }
        public void SetLane(float laneX, VehicleDirection dir)
		{
			SetLaneClientRPC(laneX, dir);
        }

		[ClientRpc]
		private void SetLaneClientRPC(float laneX, VehicleDirection dir)
		{
            targetLaneX = laneX;
            direction = dir;
        }

		private void OnDrawGizmosSelected()
		{
			Vector3 boxCenter = transform.position + Vector3.up * (boxHeight * 0.5f) + transform.forward * (boxLength * 0.5f);
			Vector3 halfExtents = new Vector3(boxWidth * 0.5f, boxHeight * 0.5f, boxLength * 0.5f);
			Quaternion orientation = transform.rotation;

			Gizmos.color = Color.red;
			Gizmos.matrix = Matrix4x4.TRS(boxCenter, orientation, Vector3.one);
			Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2);
		}

		public void InitCarStatus()
		{
			InitCarStatusClientRPC();
        }

		[ClientRpc]
		private void InitCarStatusClientRPC()
		{
            UIManager.Game.GenerateCarStatusUIs(this, carStatus);
        }

        [ClientRpc]
        public void SyncIsBrokenClientRPC(int carStatusIsBroken)
        {
			if (IsHost) return;
			carStatus.isBroken = carStatusIsBroken;
        }

		public CarParts tmpPart;
		[Button]
		public void TmpRepairMethod()
		{
			RepairingBrokenPartServerRPC(tmpPart);
        }

		[ServerRpc(RequireOwnership = false)]
		public void RepairingBrokenPartServerRPC(CarParts carPart)
		{
			RepairingBrokenPartClientRPC(carPart);
        }

		[ClientRpc]
		private void RepairingBrokenPartClientRPC(CarParts carPart)
		{
            UIManager.Game.RemoveCarStatusUI(this, carPart);
        }
    }
}