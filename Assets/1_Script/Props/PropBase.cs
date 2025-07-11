using Garage.Structs;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Props
{
	public class PropBase : NetworkBehaviour
	{
		public ItemData ItemData;
		protected Rigidbody rigid;

		[SerializeField] private int upgradeLevel = 0;
		public int UpgradeLevel => upgradeLevel;

		public virtual void Awake()
		{
			rigid = GetComponent<Rigidbody>();
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