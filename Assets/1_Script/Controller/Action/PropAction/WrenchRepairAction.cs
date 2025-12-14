using UnityEngine;
using UnityEngine;

using Garage.Controller;
using Garage.Manager;
using Garage.Props;
using Garage.Utils;
using UnityEngine;

namespace Garage.Actions
{
    [CreateAssetMenu(fileName = "WrenchRepairAction", menuName = "SO/Prop Action/Wrench Repair Action")]
    public class WrenchRepairAction : PropAction<WrenchProp>
    {
        public override void OnStart(WrenchProp prop)
        {
            //if (prop.Controller.CurrentFixablePart == null) return;

            Managers.Input.DisablePlayerInputs();

            prop.Controller.GetComponent<PlayerController>().SetAnimParam((int)AnimationType.HammerAttack, true);
        }
        public override void OnHolding(WrenchProp propr)
        {

        }
        public override void OnReleased(WrenchProp prop)
        {

        }

        public override void OnAnimationKey(WrenchProp prop)
        {
            Managers.Input.EnablePlayerInputs();
        }

        public override void OnCanceled(WrenchProp prop)
        {

        }
    }
}