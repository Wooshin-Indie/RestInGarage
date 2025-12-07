using Garage.Props;
using UnityEngine;

namespace Garage.Actions
{
	public abstract class PropAction<T> : ActionBase 
		where T : OwnableProp
    {
		public abstract void OnStart(T prop);
		public abstract void OnHolding(T prop);
		public abstract void OnReleased(T prop);
		public abstract void OnCanceled(T prop);
		public abstract void OnAnimationKey(T prop);

		public sealed override void OnStart(Object obj)
		{
			if (!IsValid(obj)) return;
			OnStart((T)obj);
		}
		public sealed override void OnHolding(Object obj)
		{
			if (!IsValid(obj)) return;
			OnHolding((T)obj);
		}
		public sealed override void OnCanceled(Object obj)
		{
			if(!IsValid(obj)) return;
			OnCanceled((T)obj);
		}
		public sealed override void OnReleased(Object obj)
		{
			if (!IsValid(obj)) return;
			OnReleased((T)obj);
		}
		public sealed override void OnAnimationKey(Object obj)
		{
			if (!IsValid(obj)) return;
			OnAnimationKey((T)obj);
		}
		

		private bool IsValid(Object obj)
		{
			if (obj == null)
				throw new System.ArgumentNullException(nameof(obj), $"{GetType().Name}: obj is null");

			if (obj is not T prop)
				throw new System.InvalidCastException($"{GetType().Name}: Expected type {typeof(T).Name}");

			return true;
		}
	}
}
