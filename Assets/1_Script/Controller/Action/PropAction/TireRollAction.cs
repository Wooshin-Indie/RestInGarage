using Garage.Controller;
using Garage.Manager;
using Garage.Utils;
using UnityEngine;

namespace Garage.Actions
{
    public class TireRollAction : PropAction
    {
        public override void OnStart(Transform controller)
        {
            Managers.Input.DisablePlayerMove();
            controller.GetComponent<PlayerController>().ChargeTireRoll();
        }
        public override void OnHolding(Transform controller)
        {
            controller.GetComponent<PlayerController>().ChargeTireRoll();
            //controller.GetComponent<PlayerController>().HoldTireRollCharge();
        }
        public override void OnReleased(Transform controller)
        {

        }
    }
}
