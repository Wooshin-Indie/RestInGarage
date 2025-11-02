using Garage.Controller;
using UnityEngine;

namespace Garage.Actions
{
    public abstract class ActionBase : ScriptableObject
    {
        public abstract void OnStart(Transform controller);
        public abstract void OnHolding(Transform controller);
        public abstract void OnReleased(Transform controller);
    }
}
