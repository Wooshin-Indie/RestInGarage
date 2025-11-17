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
		
        [Tooltip("Action을 시작할 때 사용할 Input Action 이름 ex) Player/Action")]
        [SerializeField] private string actionIAName;

        [Tooltip("Action을 취소할 때 사용할 Input Action 이름 ex) Player/Action")]
        [SerializeField] private string cancelIAName;

		[Tooltip("해당 Action이 끝나는 시점 결정")]
		[SerializeField] private ActionEndCondition endCondition = ActionEndCondition.OnKeyUp;

		/** Properties **/
		public ActionEndCondition EndCondition => endCondition;

        private InputAction cachedActionIA = null;
        private InputAction cachedCancelIA = null;

        public bool IsAbleToCancel => isAbleToCancel;
        public InputAction GetActionIA()
        {
            if (cachedActionIA == null || cachedActionIA.actionMap == null)
                cachedActionIA = Managers.Input.Control.FindAction(actionIAName, true);

            return cachedActionIA;
        }
        public InputAction GetCancelIA()
		{
            if (!isAbleToCancel)
            {
                Debug.LogError("This ActionBase is not able to Cancel.");
                return null;
            }

			if (cachedCancelIA == null || cachedCancelIA.actionMap == null)
				cachedCancelIA = Managers.Input.Control.FindAction(cancelIAName, true);

			return cachedCancelIA;
		}

        public abstract void OnStart(Object obj);
        public abstract void OnHolding(Object obj);
        public abstract void OnReleased(Object obj);
        public abstract void OnCanceled(Object obj);
        public abstract void OnAnimationKey(Object obj);
    }
}
