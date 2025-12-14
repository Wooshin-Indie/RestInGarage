using UnityEngine;

using Garage.Controller;
using Garage.Manager;
using Garage.Props;
using Garage.Utils;
using UnityEngine;

namespace Garage.Actions
{
    [CreateAssetMenu(fileName = "WrenchAttackAction", menuName = "SO/Prop Action/Wrench Attack Action")]
    public class WrenchAttackAction : PropAction<WrenchProp>
    {
        private bool hasAttacked = false;
        public override void OnStart(WrenchProp prop)
        {
            hasAttacked = false;
            Managers.Input.DisablePlayerInputs();
            if (prop is HammerProp)
                prop.Controller.GetComponent<PlayerController>().SetAnimParam((int)AnimationType.HammerAttack, true);
            else
                prop.Controller.GetComponent<PlayerController>().SetAnimParam((int)AnimationType.WrenchAttack, true);
        }
        public override void OnHolding(WrenchProp propr)
        {

        }
        public override void OnReleased(WrenchProp prop)
        {

        }
        public override void OnAnimationKey(WrenchProp prop)
        {
            if (!hasAttacked)
            {
                hasAttacked = true;
                // 맞는 시점 잡아서 Hit 검사
            }
            else
            {
                Managers.Input.EnablePlayerInputs();
            }
        }

        public override void OnCanceled(WrenchProp prop)
        {

        }
    }
}