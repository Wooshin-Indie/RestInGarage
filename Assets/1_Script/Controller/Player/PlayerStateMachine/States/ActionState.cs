using Garage.Manager;

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
            Managers.Input.DisablePlayerMove();
            controller.OnActionKeyStart();
		}

        public override void Exit()
        {
            base.Exit();
            controller.OnActionKeyReleased();
            Managers.Input.EnablePlayerMove();
        }

        public override void HandleInput()
        {
            base.HandleInput();
        }

        public override void LogicUpdate()
        {
            base.LogicUpdate();
            controller.OnActionKeyHolding();
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();

        }
    }
}
