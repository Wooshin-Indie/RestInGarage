using Garage.Manager;
using UnityEngine;

namespace Garage.Controller.StateMachine
{
	// 플레이어가 맘대로 움직일 수 없는 행동 취할 때
	public class ActionState : StateBase
    {
        public ActionState(PlayerController controller, PlayerStateMachine stateMachine)
            : base(controller, stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            Debug.Log("ENTER ACTION");
			controller.OnActionKeyStart();
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
            controller.OnActionKeyHolding();

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
		}

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();

        }
    }
}
