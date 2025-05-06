
namespace Garage.Controller.StateMachine
{
    public class StateBase
    {
        protected PlayerController controller;      // Needed to control player (ex. move)
        protected PlayerStateMachine stateMachine;

        public StateBase(PlayerController controller, PlayerStateMachine stateMachine)
        {
            this.controller = controller;
            this.stateMachine = stateMachine;
        }

        public virtual void Enter() { }             // Run once when Enter State
        public virtual void HandleInput() { }       // Manage Input in particular state
        public virtual void LogicUpdate()           // Logic Update  
		{
            controller.DetectInteractables();
		}           
        public virtual void PhysicsUpdate()         // Only Physics Update
		{
		}     
        public virtual void Exit() { }              // Run once when Exit State

    }
}
