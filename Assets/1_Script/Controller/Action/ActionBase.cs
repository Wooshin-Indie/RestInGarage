using Garage.Manager;
using IUtil;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Garage.Actions
{
	public enum ActionEndTrigger
    {
        OnKeyUp,            // 액션 끝나는 시점 = 키에서 손을 떼는 시점
        OnAnimationEnd      // 액션 끝나는 시점 = 애니메이션 이벤트가 호출할 때
    }
    public abstract class ActionBase : ScriptableObject
    {
        [Header("Action Timing Settings")]
        public ActionEndTrigger releaseTrigger = ActionEndTrigger.OnKeyUp; // 기본값은 떼는 즉시 실행

        [HelpBox("ex) Player/Action 이런 식으로 키 값을 넣어야 합니다.")]
        public string actionName;

        private InputAction cachedAction = null;

        public InputAction GetInputAction()
        {
            if(cachedAction == null)
                cachedAction = Managers.Input.Control.FindAction(actionName, true);
            return cachedAction;
        }

        public abstract void OnStart(Object obj);
        public abstract void OnHolding(Object obj);
        public abstract void OnReleased(Object obj);
        public abstract void OnAnimationKey(Object obj);
    }
}
