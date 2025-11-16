using Garage.Controller;
using Garage.Manager;
using Garage.Props;
using Garage.Utils;
using UnityEngine;

namespace Garage.Actions
{
    [CreateAssetMenu(fileName = "SprayAction", menuName = "SO/Prop Action/Spray Action")]
    public class SprayAction : PropAction<OilPump>
    {
		public override void OnStart(OilPump prop)
		{
			prop.Controller.GetComponent<PlayerController>().SetAnimParam((int)AnimationType.Oil, true);
			Managers.Input.DisablePlayerMove();
		}

		public override void OnHolding(OilPump prop)
		{

		}

		public override void OnReleased(OilPump prop)
		{
			prop.Controller.GetComponent<PlayerController>().SetAnimParam((int)AnimationType.Oil, false);
			Managers.Input.EnablePlayerMove();
		}

		public override void OnAnimationKey(OilPump prop)
		{

		}
	}
}
