using Garage.Controller;
using Garage.Manager;
using Garage.Props;
using Garage.Utils;
using UnityEngine;

namespace Garage.Actions
{
    [CreateAssetMenu(fileName = "FireExtinguishAction", menuName = "SO/Prop Action/Fire Extinguish Action")]
    public class FireExtinguishAction : PropAction<Extinguisher>
    {
        public override void OnStart(Extinguisher prop)
        {
            Managers.Input.DisablePlayerMove();
            prop.Controller.SetAnimParam((int)AnimationType.Oil, true);
            prop.StartSpray();
        }
        public override void OnHolding(Extinguisher prop)
        {
            // 마우스로 조작할거면 마우스 방향으로 플레이어 회전?
        }
        public override void OnReleased(Extinguisher prop)
        {
            prop.StopSpray();
            prop.Controller.SetAnimParam((int)AnimationType.Oil, false);
            Managers.Input.EnablePlayerMove();
        }
        public override void OnAnimationKey(Extinguisher prop)
        {

        }

        public override void OnCanceled(Extinguisher prop)
        {

        }
    }
}
