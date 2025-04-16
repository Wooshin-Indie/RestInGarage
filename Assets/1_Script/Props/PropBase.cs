using Unity.Netcode;
using UnityEngine;

namespace Garage.Props
{
	public class PropBase : NetworkBehaviour
	{
		protected Rigidbody rigid;

		public virtual void Awake()
		{
			rigid = GetComponent<Rigidbody>();
		}

		#region Transform RPC

		[ServerRpc(RequireOwnership = false)]
		protected void UpdatePlayerVelocityServerRPC(Vector3 velocity, ulong clientId)
		{
			UpdatePlayerVelocityClientRPC(velocity, clientId);
		}
		[ClientRpc]
		protected void UpdatePlayerVelocityClientRPC(Vector3 velocity, ulong clientId)
		{
			if (clientId == NetworkManager.Singleton.LocalClientId) return;
			rigid.linearVelocity = velocity;
		}

		[ServerRpc(RequireOwnership = false)]
		protected void UpdatePlayerPositionServerRPC(Vector3 playerPosition, ulong clientId)
		{
			UpdatePlayerPositionClientRPC(playerPosition, clientId);
		}

		[ClientRpc]
		protected void UpdatePlayerPositionClientRPC(Vector3 playerPosition, ulong clientId)
		{
			if (clientId == NetworkManager.Singleton.LocalClientId) return;
			rigid.MovePosition(playerPosition);
		}

		[ServerRpc(RequireOwnership = false)]
		protected void UpdatePlayerRotateServerRPC(Quaternion playerQuat, ulong clientId)
		{
			UpdatePlayerRotateClientRPC(playerQuat, clientId);
		}

		[ClientRpc]
		protected void UpdatePlayerRotateClientRPC(Quaternion playerQuat, ulong clientId)
		{
			if (clientId == NetworkManager.Singleton.LocalClientId) return;
			rigid.MoveRotation(playerQuat);
		}
		#endregion
	}
}