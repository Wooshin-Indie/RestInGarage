using Garage.Controller;
using Garage.Manager;
using Garage.Utils;
using UnityEngine;

namespace Garage.Actions
{
    [CreateAssetMenu(fileName = "TireRollAction", menuName = "SO/Prop Action/Tire Roll Action")]
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
        }
        public override void OnReleased(Transform controller)
        {
            controller.GetComponent<PlayerController>().
                SetAnimParam((int)AnimationType.Carry, false);
            controller.GetComponent<PlayerController>().
                SetAnimParam((int)AnimationType.TireRoll);
        }
        public override void OnAnimationKey(Transform controller)
        {
            UIManager.Game.CloseTireRollingUI();
            controller.GetComponent<PlayerController>().IsRollChargeStarted = false;
            Managers.Input.EnablePlayerMove(); // 0.1초 뒤에 enable 하면 좋을 듯
        }
    }
}
