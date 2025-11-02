

namespace Garage.Manager
{
	public class InputManager
	{
		private PlayerControl control;
		public PlayerControl Control => control;

		public void Init()
		{
			control = new();
			control.Enable();
		}

		public void EnablePlayerInputs()
		{
			control.Player.Enable();
        }
		public void DisablePlayerInputs()
		{
			control.Player.Disable();
        }

		public void EnablePlayerMove()
        {
            control.Player.Move.Enable();
        }
        public void DisablePlayerMove()
        {
            control.Player.Move.Disable();
        }

		public void EnablePlayerRun()
        {
            control.Player.Run.Enable();
        }
        public void DisablePlayerRun()
        {
			control.Player.Run.Disable();
        }
    }
}
