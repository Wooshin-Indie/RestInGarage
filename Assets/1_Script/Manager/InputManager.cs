

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

		public void EnablePlayerActions()
		{
			control.Player.Enable();
        }
		public void DisablePlayerActions()
		{
			control.Player.Disable();
        }
	}
}
