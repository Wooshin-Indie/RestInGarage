

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
	}
}
