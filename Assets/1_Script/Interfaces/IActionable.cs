
using UnityEngine;

namespace Garage.Interfaces
{
	public interface IActionable
	{
		void OnStartPropAction(Transform controller);
		void OnStopPropAction(Transform controller);
	}
}
