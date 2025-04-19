using Garage.Utils;
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
			colliders = new Collider[2];
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
		private void FixedUpdate()
		{
			detectedCounts = Physics.OverlapBoxNonAlloc(transform.position, Vector3.one * boxRadius, colliders, Quaternion.identity, targetLayer);
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.identity, Vector3.one);
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one * boxRadius * 2);
		}

	}
}