using Garage.Manager;
using Unity.Netcode;
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
		}

		public override void HandleInput()
		{
			base.HandleInput();
		}

		public override void LogicUpdate()
		{
			base.LogicUpdate();

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
