using Garage.Utils;
using Unity.Netcode;
using UnityEngine;

namespace Garage
{
    public class VehicleSpawnPoint : MonoBehaviour
    {
		[Header("Overlap Parameter")]
		[SerializeField] private float boxRadius;
		[SerializeField] private LayerMask targetLayer;

		VehicleDirection direction = VehicleDirection.None;
		private Collider[] colliders;

		private void Awake()
		{
			colliders = new Collider[1];
		}

		public void SetSpawnPoint(VehicleDirection dir)
		{
			switch (dir)
			{
				case VehicleDirection.Up:
					transform.rotation = Quaternion.identity;
					break;
				case VehicleDirection.Down:
					transform.rotation = Quaternion.Euler(0f, 180f, 0f);
					break;
			}
			direction = dir;
		}

		private int detectedCounts = 0;
		public bool IsAbleToSpawn()
        {
			return detectedCounts == 0;
		}

		private ulong prevCarId = ulong.MaxValue;
		private float elapsedTime = 0f;

		// HACK - 임시로 저장한 기준
		private float waitForCountSec = 2f;
		private float waitForOverSec = 5f;

		private void FixedUpdate()
		{
			detectedCounts = Physics.OverlapBoxNonAlloc(transform.position, Vector3.one * boxRadius, colliders, Quaternion.identity, targetLayer);


			if (detectedCounts == 1) 
			{
				if (prevCarId == colliders[0].GetComponentInParent<NetworkObject>().NetworkObjectId)
				{
					elapsedTime += Time.fixedDeltaTime;
				}
				else
				{
					prevCarId = colliders[0].GetComponentInParent<NetworkObject>().NetworkObjectId;
					elapsedTime = 0f;
				}
			}
			else
			{
				elapsedTime = 0f;
				prevCarId = ulong.MaxValue;
			}

			if (elapsedTime < waitForCountSec) { /* Wait for countdown */ }
			else if (elapsedTime < waitForCountSec + waitForOverSec)
			{
				Debug.Log($"Time left for over : {(int)(waitForCountSec + waitForOverSec - elapsedTime)}");
			}
			else
			{
				Debug.Log("GAME OVER");
			}
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.identity, Vector3.one);
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one * boxRadius * 2);
		}

	}
}