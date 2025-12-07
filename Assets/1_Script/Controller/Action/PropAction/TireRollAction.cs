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
		}

		public override void OnHolding(TireProp prop)
		{
			prop.Controller.RotateToMousePos();
			prop.Controller.OnUpdatePlayerGage();
		}

		public override void OnCanceled(TireProp prop)
		{
			prop.Controller.CloseGageUI();
			Managers.Input.EnablePlayerMove();
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
			prop.TireRolling(prop.Controller.GetTireRollingForce());
			prop.Controller.CloseGageUI();
			prop.Controller.TryEndInteractWithProp();

			Managers.Input.EnablePlayerMove();
		}
	}
}
