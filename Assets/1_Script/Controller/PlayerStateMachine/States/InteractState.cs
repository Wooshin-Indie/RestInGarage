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

		}

		public override void PhysicsUpdate()
		{
			base.PhysicsUpdate();

        }
	}
}
