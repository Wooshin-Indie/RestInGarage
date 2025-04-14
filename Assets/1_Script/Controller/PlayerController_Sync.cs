using Unity.Netcode;
using UnityEngine;

namespace Garage.Controller
{
	public partial class PlayerController
	{

		private void OnUpdateSynchronization()
		{
			UpdatePlayerPositionServerRPC(rigid.position);
			UpdatePlayerRotateServerRPC(rigid.rotation);
		}

		#region Transform RPC

		[ServerRpc(RequireOwnership = false)]
		public void UpdatePlayerPositionServerRPC(Vector3 playerPosition)
		{
			UpdatePlayerPositionClientRPC(playerPosition);
		}

		[ClientRpc]
		private void UpdatePlayerPositionClientRPC(Vector3 playerPosition)
		{
			if (IsOwner) return;
			rigid.MovePosition(playerPosition);
		}

		[ServerRpc(RequireOwnership = false)]
		private void UpdatePlayerRotateServerRPC(Quaternion playerQuat)
		{
			UpdatePlayerRotateClientRPC(playerQuat);
		}

		[ClientRpc]
		private void UpdatePlayerRotateClientRPC(Quaternion playerQuat)
		{
			if (IsOwner) return;
			rigid.MoveRotation(playerQuat);
		}
		#endregion
	}
}