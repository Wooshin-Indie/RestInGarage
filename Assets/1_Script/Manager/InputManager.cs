using UnityEngine;

namespace Garage.Manager
{
	public class InputManager
	{
		private PlayerControl control;
		public PlayerControl Control => control;
        public bool IsInputEnabled => control.Player.enabled;
        public bool IsAbleToMove => control.Player.Move.enabled;
		public bool IsAbleToRun => control.Player.Run.enabled;

        public void Init()
		{
			control = new();
			control.Enable();
		}

		public void EnablePlayerInputs()
		{
            Debug.Log("PlayerInputs Enabled");
			control.Player.Enable();
        }
		public void DisablePlayerInputs()
        {
            Debug.Log("PlayerInputs Enabled");
            control.Player.Disable();
        }

		public void EnablePlayerMove()
        {
            Debug.Log("PlayerMove Enabled");
            control.Player.Move.Enable();
        }
        public void DisablePlayerMove()
        {
            Debug.Log("PlayerMove Disabled");
            control.Player.Move.Disable();
        }

		public void EnablePlayerRun()
        {
            Debug.Log("PlayerRun Enabled");
            control.Player.Run.Enable();
        }
        public void DisablePlayerRun()
        {
            Debug.Log("PlayerRun Disabled");
            control.Player.Run.Disable();
        }
    }
}
