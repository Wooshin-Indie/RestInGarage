using Garage.Controller;
using UnityEngine;

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
        public abstract void OnStart(Transform controller);
        public abstract void OnHolding(Transform controller);
        public abstract void OnReleased(Transform controller);
        public abstract void OnAnimationKey(Transform controller);
    }
}
