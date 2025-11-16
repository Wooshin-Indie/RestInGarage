using Garage.Controller;
using Garage.Manager;
using Garage.Props;
using Garage.Utils;
using UnityEngine;

namespace Garage.Actions
{
    [CreateAssetMenu(fileName = "ThrowAction", menuName = "SO/Prop Action/Throw Action")]
    public class ThrowAction : PropAction<WrenchProp>
    {
		public override void OnStart(WrenchProp prop)
		{
			Managers.Input.DisablePlayerMove();
			prop.Controller.GetComponent<PlayerController>().ChargeTireRoll();
		}

		public override void OnHolding(WrenchProp prop)
		{
			prop.Controller.GetComponent<PlayerController>().ChargeTireRoll();
		}

		public override void OnReleased(WrenchProp prop)
		{
			prop.Controller.GetComponent<PlayerController>().
				SetAnimParam((int)AnimationType.Throw);
		}

		public override void OnAnimationKey(WrenchProp prop)
		{
			UIManager.Game.CloseTireRollingUI();
			prop.Controller.GetComponent<PlayerController>().IsRollChargeStarted = false;
			Managers.Input.EnablePlayerMove();
		}
	}
}
