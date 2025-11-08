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
            //SetAnimParam((int)AnimationType.Swing, true);
            //애니메이션 끝날 때 Managers.Input.EnablePlayerInputs();
        }
        public override void OnHolding(Transform controller)
        {

        }
        public override void OnReleased(Transform controller)
        {
            //SetAnimParam((int)AnimationType.Swing, true);
            Managers.Input.EnablePlayerInputs();
        }
        public override void OnAnimationKey(Transform controller)
        {
            // 맞는 시점 잡아서 Hit 검사하기?
        }
    }
}
