using Garage.Controller;
using Garage.Manager;
using Garage.Props;
using Garage.Utils;
using UnityEngine;

namespace Garage.Actions
{
	[CreateAssetMenu(fileName = "TireRollAction", menuName = "SO/Prop Action/Tire Roll Action")]
    public class TireRollAction : PropAction<TireProp>
    {
		public override void OnStart(TireProp prop)
		{
			Managers.Input.DisablePlayerMove();
			prop.Controller.ChargeTireRoll();
		}

		public override void OnHolding(TireProp prop)
		{
			prop.Controller.GetComponent<PlayerController>().ChargeTireRoll();
		}

		public override void OnReleased(TireProp prop)
		{
			prop.Controller.GetComponent<PlayerController>().
				SetAnimParam((int)AnimationType.Carry, false);
			prop.Controller.GetComponent<PlayerController>().
				SetAnimParam((int)AnimationType.TireRoll);
		}

		public override void OnAnimationKey(TireProp prop)
		{
			UIManager.Game.CloseTireRollingUI();
			prop.Controller.GetComponent<PlayerController>().IsRollChargeStarted = false;
			Managers.Input.EnablePlayerMove(); // 0.1초 뒤에 enable 하면 좋을 듯
		}
	}
}
