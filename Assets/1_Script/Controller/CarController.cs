using IUtil;
using UnityEngine;

namespace Garage.Controller
{
	public class CarController : MonoBehaviour
	{

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

		private void Awake()
		{
			rigid = GetComponent<Rigidbody>();
		}

		private void FixedUpdate()
		{
			if (isBroken)
			{
				if (transform.position.z > 0)
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
				targetRot = Quaternion.Euler(0f, steerAmount, 0f);
			}
			else
			{
				targetRot = Quaternion.Euler(0f, 0f, 0f);

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
		private void OnDrawGizmosSelected()
		{
			Vector3 boxCenter = transform.position + Vector3.up * (boxHeight * 0.5f) + transform.forward * (boxLength * 0.5f);
			Vector3 halfExtents = new Vector3(boxWidth * 0.5f, boxHeight * 0.5f, boxLength * 0.5f);
			Quaternion orientation = transform.rotation;

			Gizmos.color = Color.red;
			Gizmos.matrix = Matrix4x4.TRS(boxCenter, orientation, Vector3.one);
			Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2);
		}

		public void SetLane(int idx)
		{
			targetLaneX = idx * 10f;
		}
	}
}