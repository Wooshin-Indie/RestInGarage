using Garage.Controller;
using Garage.Manager;
using Garage.Props;
using Garage.Utils;
using UnityEngine;

namespace Garage.Actions
{
    [CreateAssetMenu(fileName = "OilSprayAction", menuName = "SO/Prop Action/Oil Spray Action")]
    public class OilSprayAction : PropAction<OilPump>
    {
        public override void OnStart(OilPump prop)
        {
            prop.Controller.GetComponent<PlayerController>().SetAnimParam((int)AnimationType.OilSpray, true);
            prop.OilGun.DelayedStartOilSpray();
            Managers.Input.DisablePlayerMove();
        }
        public override void OnHolding(OilPump prop)
        {

        }
        public override void OnReleased(OilPump prop)
        {
            prop.OilGun.StopOilSpray();
            prop.Controller.GetComponent<PlayerController>().SetAnimParam((int)AnimationType.OilSpray, false);
            Managers.Input.EnablePlayerMove();
        }
        public override void OnAnimationKey(OilPump prop)
        {

		}

		public override void OnCanceled(OilPump prop)
		{

		}
	}
}
