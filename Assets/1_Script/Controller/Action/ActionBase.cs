using Garage.Manager;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Garage.Actions
{
	public enum ActionEndCondition
    {
        OnKeyUp,            // Key Released -> Action End
        OnAnimationEnd      // Animation End -> Action End
    }
    public abstract class ActionBase : ScriptableObject
    {
        [Header("Action Settings")]
		[Tooltip("Action 도중 취소 기능을 사용할지 여부")]
		[SerializeField] private bool isAbleToCancel;

		[Tooltip("Action을 시작할 때 사용할 Input Action")]
		[SerializeField] private InputActionReference actionIARef;

        [Tooltip("Action을 취소할 때 사용할 Input Action")]
        [SerializeField] private InputActionReference cancelIARef;

		[Tooltip("해당 Action이 끝나는 시점 결정")]
		[SerializeField] private ActionEndCondition endCondition = ActionEndCondition.OnKeyUp;

		/** Properties **/
		public ActionEndCondition EndCondition => endCondition;

        public bool IsAbleToCancel => isAbleToCancel;
        public InputAction GetActionIA()
        {
            return actionIARef.action;
        }
        public InputAction GetCancelIA()
		{
            return cancelIARef.action;
		}

        public abstract void OnStart(Object obj);
        public abstract void OnHolding(Object obj);
        public abstract void OnReleased(Object obj);
        public abstract void OnCanceled(Object obj);
        public abstract void OnAnimationKey(Object obj);
    }
}
