using Garage.Interfaces;
using Garage.Manager;
using Garage.Utils;
using UnityEngine;

namespace Garage.Controller.StateMachine
{
	public class CarryState : StateBase
	{
		public CarryState(PlayerController controller, PlayerStateMachine stateMachine)
			: base(controller, stateMachine) { }

		public override void Enter()
		{
			base.Enter();

			if(controller.CurrentOwningProp == null)
			{
				stateMachine.ChangeState(controller.idleState);
				return;
			}

			if (controller.CurrentOwningProp.IsCarry)
			{
				controller.SetAnimParam((int)AnimationType.Carry, true);
			}
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
			if (controller.CurrentOwningProp == null)
			{
				stateMachine.ChangeState(controller.idleState);
                return;
			}

			// Move
			Vector2 move = Managers.Input.Control.Player.Move.ReadValue<Vector2>();
			bool isRun = Managers.Input.Control.Player.Run.IsPressed();
			controller.MovePosition(move, isRun ? controller.RunSpeed : controller.WalkSpeed, controller.RunSpeed);

			float speed = (controller.CurrentOwningProp.IsCarry) ? controller.CarrySpeed :
				((isRun ? controller.RunSpeed : controller.WalkSpeed));
			float maxSpeed = (controller.CurrentOwningProp.IsCarry) ? controller.CarrySpeed : 
				controller.RunSpeed;

			controller.MovePosition(move, speed, maxSpeed);

			// During 
			if (!GameManagerEx.Instance.IsDay && controller.CurrentOwningProp.GetComponent<IPlaceable>() != null)
			{
				BuildingManager.Instance.UpdatePreviewArea(controller.CurrentOwningProp, controller.transform);
			}

			// End Interact
			if (Managers.Input.Control.Player.Interact.WasPressedThisFrame())
			{
				if (controller.CurrentFixablePart != null)
				{
					controller.TryStartFix();
				}
				else
				{
					controller.TryEndInteract();
					return;
				}
			}

		}

		public override void PhysicsUpdate()
		{
			base.PhysicsUpdate();

        }
	}
}
