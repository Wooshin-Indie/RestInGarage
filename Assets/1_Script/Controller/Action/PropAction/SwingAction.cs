using Garage.Controller;
using Garage.Manager;
using Garage.Utils;
using System.Globalization;
using UnityEngine;

namespace Garage.Actions
{
    [CreateAssetMenu(fileName = "SwingAction", menuName = "SO/Prop Action/Swing Action")]
    public class SwingAction : PropAction
    {
        public override void OnStart(Transform controller)
        {
            Managers.Input.DisablePlayerInputs();
            controller.GetComponent<PlayerController>().SetAnimParam((int)AnimationType.WrenchAttack, true);
            //애니메이션 끝날 때 Managers.Input.EnablePlayerInputs();
        }
        public override void OnHolding(Transform controller)
        {

        }
        public override void OnReleased(Transform controller)
        {

        }
        public override void OnAnimationKey(Transform controller)
        {
            Managers.Input.EnablePlayerInputs();
            // + 맞는 시점 잡아서 Hit 검사
        }
    }
}
