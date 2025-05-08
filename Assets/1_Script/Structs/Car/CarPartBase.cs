using Garage.Controller;
using Garage.Props;
using Garage.Utils;
using System.Collections;
using UnityEngine;

namespace Garage.Structs.CarPart
{
	public abstract class CarPartBase : MonoBehaviour
	{
		[SerializeField] protected CarParts part;
		protected CarController carController;

		public CarParts PartType => part;
		public CarController CarController => carController;


        public virtual void Awake()
		{
			carController = GetComponentInParent<CarController>();
			if (carController == null)
			{
				Debug.LogWarning("There is no car controller in part's parents");
			}
		}

		public void Interact(PlayerController player, OwnableProp prop)
		{
			carController.InteractWithPart(part, player, prop);
		}

		public bool IsAbleToInteract(OwnableProp prop)
		{
			return carController.IsAbleToInteract(part, prop);
		}
	}
}