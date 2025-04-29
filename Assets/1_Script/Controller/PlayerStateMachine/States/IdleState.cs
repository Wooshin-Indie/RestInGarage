using Garage.Manager;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace Garage.Controller.StateMachine
{
	public class IdleState : StateBase
	{
		public IdleState(PlayerController controller, PlayerStateMachine stateMachine)
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

			// Move
			Vector2 move = Managers.Input.Control.Player.Move.ReadValue<Vector2>();
			bool isRun = Managers.Input.Control.Player.Run.IsPressed();
			controller.MovePosition(move, isRun ? controller.RunSpeed : controller.WalkSpeed, controller.RunSpeed);

			// Interact
			if (Managers.Input.Control.Player.Interact.WasPressedThisFrame())
			{
				controller.TryStartInteract();
			}
		}

		public override void PhysicsUpdate()
		{
			base.PhysicsUpdate();

        }
	}
}
