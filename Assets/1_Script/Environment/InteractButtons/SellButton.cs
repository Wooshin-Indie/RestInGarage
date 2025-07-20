using Garage.Manager;
using Garage.Utils;
using UnityEngine;

namespace Garage.Environment
{
	public class SellButton : InteractButton
	{
		private void Update()
		{
			progress.fillAmount = elapsedTime.Value / maxTime;
			if (!IsHost) return;

			playerCount = Physics.OverlapBoxNonAlloc(transform.position + boxCenter, boxSize * 0.5f, hits, Quaternion.identity, Constants.LAYER_PLAYER);
			isSomeoneDetected = playerCount > 0;

			if (isSomeoneDetected && BuildingNetworkManager.Instance.IsAbleToSell())
			{
				elapsedTime.Value += Time.deltaTime;
				if (elapsedTime.Value > maxTime)
				{
					BuildingNetworkManager.Instance.SellPropsServerRPC();
					elapsedTime.Value = 0f;
				}
			}
			else
			{
				if (elapsedTime.Value > 0f) elapsedTime.Value -= Time.deltaTime;
			}
		}
	}
}