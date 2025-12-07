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

			if (controller.CurrentOwningProp == null)
			{
				stateMachine.ChangeState(controller.idleState);
				return;
			}
			
			if (GameManagerEx.Instance.IsDay && controller.CurrentOwningProp.IsCarry)
			{
				controller.SetAnimParam((int)AnimationType.CarryMult,
					controller.CarrySpeed * controller.CurrentOwningProp.CarrySpeedMultiplier);
				controller.SetAnimParam((int)AnimationType.Carry, true);
			}

			Managers.Sound.PlaySfx(SFXType.PropHold);
		}

		public override void Exit()
		{
			base.Exit();
			Managers.Sound.PlaySfx(SFXType.PropPutdown);
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
			bool isRun = controller.IsRun;
			float speed = (GameManagerEx.Instance.IsDay && controller.CurrentOwningProp.IsCarry) ?
				controller.CarrySpeed * controller.CurrentOwningProp.CarrySpeedMultiplier :
				((isRun ? controller.RunSpeed : controller.WalkSpeed));
			float maxSpeed = (GameManagerEx.Instance.IsDay && controller.CurrentOwningProp.IsCarry) ?
				controller.CarrySpeed :
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
				controller.TryEndInteractWithProp();
				return;
			}

			controller.GetActionInput();

			if (controller.CurrentFixablePart != null)
			{
				return;
			}


			if (Managers.Input.Control.Player.Kick.WasPressedThisFrame())
            {
                controller.KickCar();
                return;
            }
        }

        public override void PhysicsUpdate()
		{
			base.PhysicsUpdate();

        }

	}
}
