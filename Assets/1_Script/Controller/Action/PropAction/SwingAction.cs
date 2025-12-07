using Garage.Controller;
using Garage.Manager;
using Garage.Props;
using Garage.Utils;
using UnityEngine;

namespace Garage.Actions
{
	[CreateAssetMenu(fileName = "SwingAction", menuName = "SO/Prop Action/Swing Action")]
    public class SwingAction : PropAction<WrenchProp>
    {
        public override void OnStart(WrenchProp prop)
        {
            Managers.Input.DisablePlayerInputs();
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
            Managers.Input.EnablePlayerInputs();
            // + 맞는 시점 잡아서 Hit 검사
        }

		public override void OnCanceled(WrenchProp prop)
		{

		}
	}
}
