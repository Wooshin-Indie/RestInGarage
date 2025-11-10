using Garage.Controller;
using Garage.Manager;
using Garage.Utils;
using System.Globalization;
using UnityEngine;

namespace Garage.Actions
{
    [CreateAssetMenu(fileName = "SprayAction", menuName = "SO/Prop Action/Spray Action")]
    public class SprayAction : PropAction
    {
        public override void OnStart(Transform controller)
        {
            controller.GetComponent<PlayerController>().SetAnimParam((int)AnimationType.Oil, true);
            Managers.Input.DisablePlayerMove();
        }
        public override void OnHolding(Transform controller)
        {

        }
        public override void OnReleased(Transform controller)
        {
            controller.GetComponent<PlayerController>().SetAnimParam((int)AnimationType.Oil, false);
            Managers.Input.EnablePlayerMove();
        }
        public override void OnAnimationKey(Transform controller)
        {

        }
    }
}
