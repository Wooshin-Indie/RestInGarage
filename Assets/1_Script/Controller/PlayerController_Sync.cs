using Unity.Netcode;
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
				UpdatePlayerVelocityClientRPC(rigid.linearVelocity);
			}
			else
			{
				UpdatePlayerPositionServerRPC(rigid.position);
				UpdatePlayerRotateServerRPC(rigid.rotation);
				UpdatePlayerVelocityClientRPC(rigid.linearVelocity);
			}
		}

		#region Transform RPC


		[ServerRpc(RequireOwnership = false)]
		public void UpdatePlayerVelocityServerRPC(Vector3 velocity)
		{
			UpdatePlayerVelocityClientRPC(velocity);
		}
		[ClientRpc]
		public void UpdatePlayerVelocityClientRPC(Vector3 velocity)
		{
			if(IsOwner) return;
			rigid.linearVelocity = velocity;
		}

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
			animator.SetBool(animIDs[id], param);
			if (IsHost)
			{
				ChangeAnimatorParamClientRPC(id, param);
			}
			else
			{
				ChangeAnimatorParamServerRPC(id, param);
			}
		}
		public void SetAnimParam(int id, float param)
		{
			animator.SetFloat(animIDs[id], param);
			if (IsHost)
			{
				ChangeAnimatorParamClientRPC(id, param);
			}
			else
			{
				ChangeAnimatorParamServerRPC(id, param);
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
			animator.SetBool(animIDs[id], param);
		}
		[ClientRpc]
		public void ChangeAnimatorParamClientRPC(int id, float param)
		{
			if (IsOwner) return;
			animator.SetFloat(animIDs[id], param);
		}
		#endregion
	}
}