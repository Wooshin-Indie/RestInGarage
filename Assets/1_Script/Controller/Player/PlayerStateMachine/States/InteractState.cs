using Garage.Manager;
using Garage.Utils;
using UnityEngine;

namespace Garage.Controller.StateMachine
{
	public class InteractState : StateBase
	{
		public InteractState(PlayerController controller, PlayerStateMachine stateMachine)
			: base(controller, stateMachine) { }

		public override void Enter()
		{
			base.Enter();

		}

		public override void Exit()
		{
			base.Exit();

			controller.SetAnimLayerWeight(Constants.ANIM_LAYER_INDEX_LOWERBODY, 0f);
			controller.SetAnimParam((int)AnimationType.Oil, false);
			controller.SetAnimParam((int)AnimationType.Fix, false);
			controller.SetAnimParam((int)AnimationType.WrenchRepair, false);
            controller.SetAnimParam((int)AnimationType.HammerRepair, false);
        }

		public override void HandleInput()
		{
			base.HandleInput();
		}

		private bool isInteractPressed = false;
		public override void LogicUpdate()
		{
			base.LogicUpdate();

			controller.Rigid.linearVelocity = Vector3.zero;

			if (controller.CurrentFixablePart == null)
			{
				stateMachine.ChangeState(controller.carryState);
				return;
			}

            if (Managers.Input.Control.Player.Interact.IsPressed())
			{
				controller.CurrentFixablePart.Interact(controller, controller.CurrentOwningProp);
			}
			else
			{
				stateMachine.ChangeState(controller.carryState);
				return;
			}
        }

		public override void PhysicsUpdate()
		{
			base.PhysicsUpdate();

        }
	}
}
