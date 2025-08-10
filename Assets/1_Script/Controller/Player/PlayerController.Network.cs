using Garage.Manager;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Controller
{
	public partial class PlayerController
	{

		public NetworkVariable<int> PlayerID = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		/// <summary>
		/// 개별 PlayerID를 부여해서 Spawn시 머터리얼을 변경합니다.
		/// </summary>
		private void OnPlayerIDChanged(int prev, int playerId)
		{
			var materials = meshRenderer.sharedMaterials.ToList();

			materials.Clear();
			materials.Add(playerMaterial[playerId]);

			meshRenderer.materials = materials.ToArray();
		}

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
				UpdatePlayerVelocityServerRPC(rigid.linearVelocity);
			}
		}

		[ServerRpc(RequireOwnership = false)]
		private void DespawnPropServerRPC(ulong networkId)
		{
			Managers.Spawn.DespawnObject(networkId);
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

		public void SetAnimParam(int id)
		{
			if (id == 8)
				Debug.Log("knockback!!!!!!!!!!!");
			animator.SetTrigger(animIDs[id]);
			if (IsHost)
			{
				ChangeAnimatorParamClientRPC(id);
			}
			else
			{
				ChangeAnimatorParamServerRPC(id);
			}
		}
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
		private void ChangeAnimatorParamServerRPC(int id)
		{
			ChangeAnimatorParamClientRPC(id);
		}
		[ServerRpc(RequireOwnership = false)]
		private void ChangeAnimatorParamServerRPC(int id, bool param)
		{
			ChangeAnimatorParamClientRPC(id, param);
		}
		[ServerRpc(RequireOwnership = false)]
		private void ChangeAnimatorParamServerRPC(int id, float param)
		{
			ChangeAnimatorParamClientRPC(id, param);
		}

		[ClientRpc]
		private void ChangeAnimatorParamClientRPC(int id)
		{
			if (IsOwner) return;
			animator.SetTrigger(animIDs[id]);
		}
		[ClientRpc]
		private void ChangeAnimatorParamClientRPC(int id, bool param)
		{
			if (IsOwner) return;
			animator.SetBool(animIDs[id], param);
		}
		[ClientRpc]
		private void ChangeAnimatorParamClientRPC(int id, float param)
		{
			if (IsOwner) return;
			animator.SetFloat(animIDs[id], param);
		}
		#endregion
	}
}