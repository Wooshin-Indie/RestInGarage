using Garage.Controller;
using Garage.Manager;
using Garage.Props;
using UnityEngine;

namespace Garage.Actions
{
	[CreateAssetMenu(fileName = "SwingAction", menuName = "SO/Prop Action/Swing Action")]
    public class SwingAction : PropAction<WrenchProp>
    {
		public override void OnStart(WrenchProp prop)
		{
			Managers.Input.DisablePlayerMove();
			prop.Controller.OnUpdatePlayerGage();
		}

		public override void OnHolding(WrenchProp prop)
		{

		}

		public override void OnCanceled(WrenchProp prop)
		{

		}

		public override void OnReleased(WrenchProp prop)
		{
			//SetAnimParam((int)AnimationType.Swing, true);
			Managers.Input.EnablePlayerInputs();
		}

		public override void OnAnimationKey(WrenchProp prop)
		{
			prop.Controller.GetComponent<PlayerController>().TryEndInteractWithProp();
			prop.OnEndInteraction(prop.Controller.transform);

			//Managers.Input.DisablePlayerInputs();
			//SetAnimParam((int)AnimationType.Swing, true);
			//애니메이션 끝날 때 Managers.Input.EnablePlayerInputs();
		}
	}
}
