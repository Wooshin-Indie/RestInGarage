using Garage.Controller;
using Garage.Manager;
using Garage.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Garage
{
    public class VehicleSpawnPoint : MonoBehaviour
    {
		[Header("Overlap Parameter")]
		[SerializeField] private float safeBoxRadius;
		[SerializeField] private float gameoverBoxRadius;
		[SerializeField] private LayerMask targetLayer;

		private VehicleDirection direction = VehicleDirection.None;
		public VehicleDirection Direction => direction;
        private Collider[] colliders;

		private void Awake()
		{
			colliders = new Collider[30];
		}

		public void SetSpawnDir(VehicleDirection dir)
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

		private int safeCounts = 0;
		private int gameoverCounts = 0;
		public bool IsAbleToSpawn()
		{
			return safeCounts == 0;
		}

		// HACK - 임시로 저장한 기준
		private float waitForCountSec = 7f;
		private float waitForOverSec = 5f;


		private HashSet<CarController> currentlyInside = new HashSet<CarController>();
		private HashSet<CarController> previouslyInside = new HashSet<CarController>();


		private float elapsedTime = 0f;
		private void FixedUpdate()
		{
			safeCounts = Physics.OverlapBoxNonAlloc(transform.position, Vector3.one * safeBoxRadius, colliders, Quaternion.identity, targetLayer);
			CountdownProcess();
		}

		private void CountdownProcess()
		{
			previouslyInside.Clear();
			foreach (var car in currentlyInside)
				previouslyInside.Add(car);

			currentlyInside.Clear();

			HashSet<CarController> processedCars = new HashSet<CarController>();
			gameoverCounts = Physics.OverlapBoxNonAlloc(transform.position, Vector3.one * gameoverBoxRadius, colliders, Quaternion.identity, targetLayer);

			for (int i = 0; i < gameoverCounts; i++)
			{
				CarController car = colliders[i].GetComponentInParent<CarController>();
				if (car == null || processedCars.Contains(car)) continue;

				processedCars.Add(car);

				car.GameoverTime += Time.fixedDeltaTime;
				float elapsedTime = car.GameoverTime;

				currentlyInside.Add(car);

				if (elapsedTime >= waitForCountSec && elapsedTime < waitForCountSec + waitForOverSec)
				{
					car.ShowCountdownUIClientRPC(elapsedTime - waitForCountSec, waitForOverSec);
				}
				else if (elapsedTime >= waitForCountSec + waitForOverSec)
				{
					Debug.Log("GAME OVER");
				}
			}

			foreach (var car in previouslyInside)
			{
				if (!currentlyInside.Contains(car))
				{
					car.GameoverTime = 0f;
					car.HideCountdownUIClientRPC();
				}
			}
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.identity, Vector3.one);
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one * safeBoxRadius * 2);

			Gizmos.DrawWireCube(Vector3.zero, Vector3.one * gameoverBoxRadius * 2);
		}

	}
}