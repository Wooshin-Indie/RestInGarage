
using DG.Tweening;
using Garage.Manager;
using Garage.Structs;
using Garage.Utils;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Controller
{
	/// <summary>
	/// CarController의 상태 동기화 관련 코드
	/// </summary>
	public partial class CarController
	{

		#region Fix Progress Synchronization

		[ServerRpc(RequireOwnership = false)]
		private void AddTireServerRPC(CarParts part)
		{
			AddTireLogic(part);
			AddTireClientRPC(part);
		}

		[ClientRpc]
		private void AddTireClientRPC(CarParts part)
		{
			if (IsHost) return;
			AddTireLogic(part);
		}

		[ServerRpc(RequireOwnership = false)]
		private void ProgressFixGageServerRPC(CarParts part, float deltaTime, ulong networkId)
		{
			if (carStatus.IsProgressFull(part))
			{
				OnPartRepairedClientRPC(part, networkId);
				Debug.Log(part + "is fulled");

				isAnyBroken = carStatus.IsThereAnyBroken();
				if (!isAnyBroken) // 모든 part 고쳐졌을 때
					OnAllPartsRepairedClientRPC();

				return;
			}

			carStatus.AddProgress(part, deltaTime / fixingTime);

			UIManager.Game.ApplyProgressToUI(part, carStatus.Progress[(int)part], this);
			ApplyProgressWithUIClientRPC(part, carStatus.GetProgress(part));
		}

		[ClientRpc]
		private void OnPartRepairedClientRPC(CarParts part, ulong networkId)
		{
			carStatus.SetIsBrokenAsFalse(part); // 비트마스킹 끔

			if (networkId == NetworkManager.Singleton.LocalClientId)
			{
				var pc = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().GetComponent<PlayerController>();
				pc.StateMachine.ChangeState(pc.carryState);
				Managers.Sound.PlaySfx(SFXType.Pop, .7f, .7f);
				Debug.Log("Repair Ended and Changed to CarryState");
			}
			Debug.Log("Part Repair Totally Ended");

			UIManager.Game.RemoveCarStatusUI(this, part);

			switch (part)
			{
				case CarParts.FLT:
				case CarParts.FRT:
				case CarParts.RLT:
				case CarParts.RRT:
					break;
				case CarParts.Engine:
					smokePS.Stop();
					break;
				case CarParts.Oil:
					break;
			}
		}

		private bool isAllPartsRepaired = false;
		[ClientRpc]
		private void OnAllPartsRepairedClientRPC()
		{
			if (isAllPartsRepaired) return;
			isAllPartsRepaired = true;

			if (!IsHost)
				isAnyBroken = false;

			EconomyManager.Instance.EarnMoney_HostOnly(Managers.Resource.GetData<StageData>(GameSynchronizer.Instance.CurrentStage.Value).EarnMoney.GetRandomValue());
			Managers.Sound.PlaySfx(SFXType.Complete, .8f, .9f);
			allRepairedVFX.Play();
		}

		#endregion

		#region Fire/Extinguish Progress Synchronization

		[ServerRpc(RequireOwnership = false)]
		private void ExtinguishFireServerRPC(float deltaTime, ulong clinetId)
		{
			if (carStatus.IsFiring())
			{
				carStatus.ExtinguishFire(deltaTime / -extinguishTime);

				if (!carStatus.IsFiring())
				{
					isAnyBroken = carStatus.IsThereAnyBroken();
					if (!isAnyBroken)
					{
						OnAllPartsRepairedClientRPC();
					}
				}
			}
		}

		[ClientRpc]
		private void UpdateFireProgressClientRPC(float progress)
		{
			UpdateFireProgressLogic(progress);
		}

		[ClientRpc]
		private void OnCarExplosionClientRPC()
		{
			isExploded = true;
			Managers.Sound.PlaySfx(SFXType.Boom, 1.2f, 1f);
			explosionPS.Play();
			firePS.gameObject.SetActive(false);
			UIManager.Game.RemoveAllCarStatusUI(this);

			Material mat = Instantiate(brokenCarMat);
			meshRenderer.materials = new Material[2]
			{
				mat,
				mat
			};
			for (int i = 0; i < wheelRenderers.Count; i++)
				wheelRenderers[i].material = mat;

			float currentValue = 1f;
			mat.SetInt("_TransparentEnabled", 1);
			DOTween.To(() => currentValue, x =>
			{
				currentValue = x;
				mat.SetFloat("_Tweak_transparency", x);
			}, -1f, 3f)
			.OnComplete(() =>
			{
				if (IsHost)
				{
					TrafficManager.Instance.DespawnCar(this);
				}
			});
		}

		#endregion

		#region Kick Sync

		Coroutine kickedCoroutine;

		[ServerRpc(RequireOwnership = false)]
		public void ApplyKickServerRPC(KickDirection kickDir)
		{
			float distanceByLane = TrafficManager.Instance.CurStageData.LaneWidth / 3f;
			float distance = distanceByLane > 0 ? distanceByLane : -distanceByLane; // distance는 절댓값으로 받음
																					// 맵 월드좌표는 오른쪽이 +X방향임
			float distanceX;

			if ((direction == VehicleDirection.Up && kickDir == KickDirection.Right) ||
				(direction == VehicleDirection.Down && kickDir == KickDirection.Left))
			{
				// 왼쪽으로 기우는 애니메이션 실행  (차량 기우는 기준은 운전자 시점)
				//SetAnimParam(0);
			}
			else
			{
				//SetAnimParam(1);
			}

			if (kickDir == KickDirection.Right)
			{
				distanceX = -distance;
			}
			else
			{
				distanceX = distance;
			}

			ApplyKickClientRPC(distanceX);
		}

		[ClientRpc]
		private void ApplyKickClientRPC(float distanceX)
		{
			if (kickedCoroutine == null)
				kickedCoroutine = StartCoroutine(MoveSideways(distanceX, 1f));
		}

		#endregion

		#region UI Synchronization
	
		[ClientRpc]
		public void ShowCountdownUIClientRPC(float elapsedTime, float maxTime)
		{
			UIManager.Game.ShowCountdownUI(this, elapsedTime, maxTime);
		}

		[ClientRpc]
		public void HideCountdownUIClientRPC()
		{
			UIManager.Game.HideCountdownUI(this);
		}
		
		[ClientRpc]
		private void ApplyProgressWithUIClientRPC(CarParts part, float progress)
		{
			if (IsHost) return;

			carStatus.Progress[(int)part] = progress;
			UIManager.Game.ApplyProgressToUI(part, carStatus.Progress[(int)part], this);
		}

		#endregion
	}
}