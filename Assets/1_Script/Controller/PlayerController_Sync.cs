using Unity.Netcode;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

namespace Garage.Controller
{
	public partial class PlayerController
	{

		private void OnUpdateSynchronization()
		{
			if (IsHost)
			{
				UpdatePlayerPositionClientRPC(rigid.position);
				UpdatePlayerRotateClientRPC(rigid.rotation);
			}
			else
			{
				UpdatePlayerPositionServerRPC(rigid.position);
				UpdatePlayerRotateServerRPC(rigid.rotation);
			}
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

		#region Rigidbody RPC
		[ServerRpc(RequireOwnership = false)]
		private void SetKinematicServerRPC(bool isKinematic)
		{
			SetKinematicClientRPC(isKinematic);
		}

		[ClientRpc]
		private void SetKinematicClientRPC(bool isKinematic)
		{
			if (IsOwner) return;
			rigid.isKinematic = isKinematic;
			capsule.isTrigger = isKinematic;
		}
		#endregion

		#region Animator RPC

		public void SetAnimParam(int id, bool param)
		{
			animator.SetBool(id, param);
			if (IsHost)
			{
				ChangeAnimatorParamClientRPC(animSpeedID, param);
			}
			else
			{
				ChangeAnimatorParamServerRPC(animSpeedID, param);
			}
		}

		public void SetAnimParam(int id, float param)
		{
			animator.SetFloat(id, param);
			if (IsHost)
			{
				ChangeAnimatorParamClientRPC(animSpeedID, param);
			}
			else
			{
				ChangeAnimatorParamServerRPC(animSpeedID, param);
			}
		}
		[ServerRpc(RequireOwnership = false)]
		public void ChangeAnimatorParamServerRPC(int id, bool param)
		{
			ChangeAnimatorParamClientRPC(id, param);
		}
		[ServerRpc(RequireOwnership = false)]
		public void ChangeAnimatorParamServerRPC(int id, float param)
		{
			ChangeAnimatorParamClientRPC(id, param);
		}

		[ClientRpc]
		public void ChangeAnimatorParamClientRPC(int id, bool param)
		{
			if (IsOwner) return;
			animator.SetBool(id, param);
		}
		[ClientRpc]
		public void ChangeAnimatorParamClientRPC(int id, float param)
		{
			if (IsOwner) return;
			animator.SetFloat(id, param);
		}
		#endregion
	}
}