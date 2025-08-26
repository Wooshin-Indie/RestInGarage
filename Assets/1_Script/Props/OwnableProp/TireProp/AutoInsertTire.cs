using Garage.Controller;
using Garage.Manager;
using Garage.Structs.CarPart;
using Garage.Utils;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Props
{
	public class AutoInsertTire : TireProp
	{
		[SerializeField] protected float speedThreshold;

		[SerializeField] private Vector3 boxHalfExtents = new Vector3(1f, 1f, 1f);
		
		private Collider[] results = new Collider[10];
		private PlayerController prevController = null;

		public override void Update()
		{
			base.Update();

			if (controller != null)
			{
				prevController = controller;
				return;
			}

			int count = Physics.OverlapBoxNonAlloc(
				transform.position,   
				boxHalfExtents,                   
				results,
				transform.rotation,               
				Constants.LAYER_INTERACTABLE            
			);

			for (int i = 0; i < count; i++)
			{
				Collider col = results[i];
				if (col == null) continue;

				CarPartTire tire = col.GetComponent<CarPartTire>();
				if (tire != null && tire.IsAbleToInteract(this))
				{
					tire.Interact(prevController, this);
					Managers.Spawn.DespawnObject(GetComponent<NetworkObject>().NetworkObjectId);
					break;
				}
			}
		}

		void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.green;

			Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
			Gizmos.DrawWireCube(Vector3.zero, boxHalfExtents * 2f);
		}
	}
}