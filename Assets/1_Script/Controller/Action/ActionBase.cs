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
        [Header("Action Timing Settings")]
        [Tooltip("해당 Action이 끝나는 시점 결정")]
        [SerializeField] private ActionEndCondition endCondition = ActionEndCondition.OnKeyUp;

        [Tooltip("사용할 Input Action 이름 ex) Player/Action")]
        [SerializeField] private string actionName;

        /** Properties **/
        public ActionEndCondition EndCondition => endCondition;
        public string ActionName => actionName;

        private InputAction cachedAction = null;
        public InputAction GetInputAction()
        {
            if (cachedAction == null || cachedAction.actionMap == null)
                cachedAction = Managers.Input.Control.FindAction(actionName, true);

            return cachedAction;
        }

        public abstract void OnStart(Object obj);
        public abstract void OnHolding(Object obj);
        public abstract void OnReleased(Object obj);
        public abstract void OnAnimationKey(Object obj);
    }
}
