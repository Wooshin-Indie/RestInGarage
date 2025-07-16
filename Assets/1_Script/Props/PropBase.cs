using Garage.Structs;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Props
{
	public class PropBase : NetworkBehaviour
	{
		public ItemData ItemData;
		protected Rigidbody rigid;

		[SerializeField] private NetworkVariable<int> upgradeLevel = new();
		public int UpgradeLevel => upgradeLevel.Value;

		public virtual void Awake()
		{
			rigid = GetComponent<Rigidbody>();
		}

		public void UpgradeItem_HostOnly()
		{
			if (!IsHost) return;
			if (!IsAbleToUpgrade()) return;

			// TODO - 업그레이드 돈 사용
			upgradeLevel.Value += 1;
		}

		public bool IsAbleToUpgrade()
		{
			return upgradeLevel.Value < ItemData.UpgradeDatas.Count - 1;
		}

		#region Transform RPC

		[ServerRpc(RequireOwnership = false)]
		protected void UpdatePlayerVelocityServerRPC(Vector3 velocity, ulong clientId)
		{
			UpdatePropVelocityClientRPC(velocity, clientId);
		}
		[ClientRpc]
		protected void UpdatePropVelocityClientRPC(Vector3 velocity, ulong clientId)
		{
			if (clientId == NetworkManager.Singleton.LocalClientId) return;
			rigid.linearVelocity = velocity;
		}

		[ServerRpc(RequireOwnership = false)]
		protected void UpdatePropPositionServerRPC(Vector3 playerPosition, ulong clientId)
		{
			UpdatePropPositionClientRPC(playerPosition, clientId);
		}

		[ClientRpc]
		protected void UpdatePropPositionClientRPC(Vector3 playerPosition, ulong clientId)
		{
			if (clientId == NetworkManager.Singleton.LocalClientId) return;
			rigid.MovePosition(playerPosition);
		}

		[ServerRpc(RequireOwnership = false)]
		protected void UpdatePropRotateServerRPC(Quaternion playerQuat, ulong clientId)
		{
			UpdatePropRotateClientRPC(playerQuat, clientId);
		}

		[ClientRpc]
		protected void UpdatePropRotateClientRPC(Quaternion playerQuat, ulong clientId)
		{
			if (clientId == NetworkManager.Singleton.LocalClientId) return;
			rigid.MoveRotation(playerQuat);
		}
		#endregion
	}
}