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
		}

		public override void OnHolding(WrenchProp prop)
		{
			prop.Controller.RotateToMousePos();
			prop.Controller.OnUpdatePlayerGage();
		}

		public override void OnCanceled(WrenchProp prop)
		{
			prop.Controller.CloseGageUI();
			Managers.Input.EnablePlayerMove();
		}

		public override void OnReleased(WrenchProp prop)
		{
			prop.Controller.SetAnimParam((int)AnimationType.Throw);
		}

		public override void OnAnimationKey(WrenchProp prop)
		{
			prop.ThrowWrench(prop.Controller.GetTireRollingForce());
			prop.Controller.CloseGageUI();
			prop.Controller.TryEndInteractWithProp();

			Managers.Input.EnablePlayerMove();
		}
	}
}
