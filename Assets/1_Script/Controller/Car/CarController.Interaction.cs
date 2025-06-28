using Garage.Manager;
using Garage.Props;
using Garage.Structs;
using Garage.Utils;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Controller
{
	/// <summary>
	/// CarController 와의 상호작용 관련 함수
	/// </summary>
	public partial class CarController
	{
		private bool isFired = false;
		private bool isExploded = false;
		public bool IsExploded => isExploded;

		private float prevFireProgress = -1f;
		private float maxFireHeight = 1.5f;

		/// <summary>
		/// 현재 Player가 들고있는 prop과 part를 토대로 상호작용 가능한지 판단
		/// </summary>
		public bool IsAbleToInteract(CarParts part, OwnableProp prop)
		{
			if (!carStatus.IsBroken(part) || isExploded) return false;

			switch (part)
			{
				case CarParts.FLT:
				case CarParts.FRT:
				case CarParts.RLT:
				case CarParts.RRT:
					return (prop is TireProp && carStatus.IsTireEmpty(part))
						|| (prop is WrenchProp && !carStatus.IsTireEmpty(part));
				case CarParts.Oil:
					return prop is OilPump;
				case CarParts.Engine:
					return prop is WrenchProp;
			}

			return false;
		}
		/// <summary>
		/// 해당 part와 상호작용하는 함수
		/// </summary>
		public void InteractWithPart(CarParts part, PlayerController player, OwnableProp prop)
		{
			float wrenchSpeed = player.WrenchRepairSpeed;

            switch (part)
			{
				case CarParts.FLT:
				case CarParts.FRT:
				case CarParts.RLT:
				case CarParts.RRT:
					if (carStatus.IsTireEmpty(part) && prop is TireProp)
					{
						AddTireServerRPC(part);
					}
					else if (!carStatus.IsTireEmpty(part) && carStatus.IsBroken(part) && prop is WrenchProp)
					{
						ProgressFixGageServerRPC(part, Time.deltaTime * wrenchSpeed, NetworkManager.Singleton.LocalClientId);
					}
					break;
                // TODO - 여기 prop.Mult 랑 합연산으로 처리해야됨
				case CarParts.Oil:
                    ProgressFixGageServerRPC(part, Time.deltaTime, NetworkManager.Singleton.LocalClientId);
					break;
                case CarParts.Engine:
					ProgressFixGageServerRPC(part, Time.deltaTime * wrenchSpeed, NetworkManager.Singleton.LocalClientId);
					break;
				case CarParts.Fire:
					ExtinguishFireServerRPC(Time.deltaTime, NetworkManager.Singleton.LocalClientId);
					break;

			}
		}

		private void HideTire(CarParts part)
		{
			Renderer rend = partTransforms[(int)part].GetComponent<Renderer>();
			MeshCollider collid = partTransforms[(int)part].GetComponent<MeshCollider>();
			rend.enabled = false;
			collid.isTrigger = true;
		}
		private void RevealTire(CarParts part)
		{
			Renderer rend = partTransforms[(int)part].GetComponent<Renderer>();
			MeshCollider collid = partTransforms[(int)part].GetComponent<MeshCollider>();
			rend.enabled = true;
			collid.isTrigger = false;
			RestoreOriginRot(1f);
		}
		private void AddTireLogic(CarParts part)
		{
			carStatus.AddTire(part);
			UIManager.Game.OnTireInserted(this, part);
			RevealTire(part);
		}

		// TODO - 이건 VehicleData에 있어야될듯?
		private float boomRadius = 12f;
		private float fireTime = 20f;
		private float extinguishTime = 3f;
		private float fixingTime = 3f;

		/// <summary>
		/// Fire의 Progress를 지속적으로 증가시키는 함수
		/// </summary>
		private void OnUpdateFire()
		{
			if (IsHost)
			{
				if (carStatus.FireProgress > 1f)
					OnCarExplosion();
				else
				{
					if (carStatus.IsFiring() && IsInBoundary())
						carStatus.ExtinguishFire(Time.fixedDeltaTime / fireTime);
				}
				if (!isExploded)
					UpdateFireProgressClientRPC(carStatus.FireProgress);
			}
		}

		/// <summary>
		/// Fire Progress를 동기화하는 로직
		/// </summary>
		private void UpdateFireProgressLogic(float progress)
		{
			if (isExploded) return;

			carStatus.FireProgress = progress;

			if (carStatus.IsFiring())
			{
				isFired = true;

				UIManager.Game.UpdateCarFiringUI(this, carStatus.FireProgress);
				if (!firePS.isPlaying)
					firePS.Play();

				var main = firePS.main;
				main.startSizeY = maxFireHeight * Mathf.Clamp(carStatus.FireProgress, 0, 1);
			}
			else
			{
				if (isFired)
				{
					UIManager.Game.RemoveCarStatusUI(this, CarParts.Fire);
					isFired = false;
					firePS.Stop();
				}
			}

			if (prevFireProgress > carStatus.FireProgress)
			{
				if (!extinguishPS.isPlaying)
					extinguishPS.Play();
			}
			else
				extinguishPS.Stop();

			prevFireProgress = carStatus.FireProgress;
		}

		/// <summary>
		/// 차가 터질 때 호출하는 함수
		/// </summary>
		private void OnCarExplosion()
		{
			if (isExploded) return;
			isExploded = true;

			EconomyManager.Instance.EraseMoney_HostOnly(Managers.Resource.GetData<MapData>(GameSynchronizer.Instance.CurrentStage.Value).EraseMoney.GetRandomValue());
			OnCarExplosionClientRPC();
			Collider[] hits = Physics.OverlapSphere(transform.position, boomRadius, Constants.LAYER_VEHICLE);
			HashSet<CarController> processed = new HashSet<CarController>();

			for (int i = 0; i < hits.Length; i++)
			{
				CarController controller = hits[i].GetComponentInParent<CarController>();
				if (controller == null) continue;

				if (processed.Add(controller))
				{
					controller.OnFired();
				}
			}
		}
		
		/// <summary>
		/// 주변 차가 터져서 해당 차가 불타는 경우 호출
		/// </summary>
		public void OnFired()
		{
			if (!carStatus.IsFiring())
				carStatus.FireProgress = .3f;
			else
				carStatus.FireProgress += .3f;
		}
	}
}