
using DG.Tweening;
using Garage.Manager;
using Garage.Structs;
using Garage.Utils;
using System.Collections;
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

			Managers.Record.RecordData(networkId, RuntimeRecordType.FixGage, deltaTime / fixingTime);
			// TODO - 여기 게이지 fixingTime 수정 필요할수도
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

			EconomyManager.Instance.EarnMoney_HostOnly(GameManagerEx.Instance.CurMapData.
				StageDatas[GameManagerEx.Instance.CurStageIdx].EarnMoney.GetRandomValue());
			StartCoroutine(FxsOnAllPartsRepaired());
		}
		private IEnumerator FxsOnAllPartsRepaired()
		{
            Managers.Sound.PlaySfx(SFXType.Complete, .6f, .9f);
            allRepairedVFX.Play();

			yield return new WaitForSeconds(0.5f);

            Managers.Sound.PlaySfx(SFXType.Voice_ThankYou, .8f);
            UIManager.Game.PopEmoteGoodUIOnCar(this);
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
        /// <summary>
        /// isToUpward는 차량이 위쪽으로 이동해야하면 true, 아래쪽으로 이동해야하면 false
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
		public void ApplyKickServerRPC(bool isToUpward)
		{
			float distanceByLane = TrafficManager.Instance.CurMapData.LaneWidth / 3f;
			float distance = distanceByLane > 0 ? distanceByLane : -distanceByLane; // distance는 절댓값으로 받음
																					// 맵 월드좌표는 오른쪽이 +X방향임
			float distanceX;

			if ((direction == VehicleDirection.Left && isToUpward == false) ||
				(direction == VehicleDirection.Right && isToUpward == true))
			{
                // 왼쪽으로 기우는 애니메이션 실행  (차량 기우는 기준은 운전자 시점)
                //SetAnimParam(0);
                PlayCarDustVfxClientRPC(LocalFourDirection.Left);
            }
			else
			{
                //SetAnimParam(1);
                PlayCarDustVfxClientRPC(LocalFourDirection.Right);
            }

			if (isToUpward == false)
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

        #region Vfx, Sfx
        [ClientRpc]
        private void PlayCarDustVfxClientRPC(LocalFourDirection direction)
        {
			Vector3 localRotation = VFXManager.Instance.GetVFXRotation(VFXType.CarImpulseDust);
			Transform parent = null;

            switch (direction)
            {
                case LocalFourDirection.Front:
                    localRotation.y = 0f;
                    parent = frontMiddleTf;
                    break;
                case LocalFourDirection.Right:
                    localRotation.y = 90f;
                    parent = rightMiddleTf;
                    break;
                case LocalFourDirection.Rear:
                    localRotation.y = 180f;
                    parent = rearMiddleTf;
                    break;
                case LocalFourDirection.Left:
                    localRotation.y = 270f;
                    parent = leftMiddleTf;
                    break;
            }

            VFXManager.Instance.PlayVFX(VFXType.CarImpulseDust, Vector3.zero, Quaternion.Euler(localRotation), parent);
        }
        [ClientRpc]
        private void PlayCarDrivingSfxClientRPC()
		{
            Managers.Sound.PlayCarDrivingSfx(this, 1f, 1f);
        }
		[ClientRpc]
		private void StopCarDrivingSfxClientRPC()
		{
            Managers.Sound.StopCarDrivingSfx(this);
        }
        #endregion
    }
}