using Garage.Utils;
using Garage.Structs;
using IUtil;
using UnityEngine;
using Garage.Manager;
using System.Collections.Generic;
using Unity.Netcode;
using Garage.Props;
using System.Collections;
using UnityEngine.EventSystems;
using TMPro;
using Unity.VisualScripting;
using Garage.Structs.CarPart;

namespace Garage.Controller
{
	public class CarController : NetworkBehaviour
	{
        private Animator animator;

        [Header("Car Parts Transform")]
        [SerializeField] public List<Transform> PartTransforms = new List<Transform>(); // 넣을 때 CarParts enum 순서 맞춰서 넣기

		[SerializeField] private ParticleSystem smokePS;
		[SerializeField] private ParticleSystem allRepairedVFX;

        [FoldoutGroup("Move Parameters")]
		[SerializeField] private float moveSpeed = 5f;
		[SerializeField] private float stopDistance = 15f;
		[SerializeField] private float tmpDistance = 7f;
		[SerializeField] private float steeringStrength;
		[SerializeField] private float maxSteerAngle;
		[SerializeField] private float laneSnapThreshold;
		[SerializeField] private bool isAnyBroken = true; // 하나라도 고장난 부분이 있는지

		[FoldoutGroup("Overlap Parameters")]
		[SerializeField] private float boxLength = 10f;
		[SerializeField] private float boxWidth = 1f;
		[SerializeField] private float boxHeight = 1f;
		[SerializeField] private LayerMask obstacleLayer;

		private float targetLaneX = 0f;
		private float removeLaneLength;
		private bool isBeingForced = false;
        public bool IsBeingForced => isBeingForced;
        private bool isBeingControlled = false;
        private Rigidbody rigid;
		private Collider[] hitResults = new Collider[10];

		private CarStatus carStatus;
		public CarStatus CarStatus { get => carStatus; }

        private void Awake()
		{
            animator = GetComponent<Animator>();
            rigid = GetComponent<Rigidbody>();
			carStatus = new CarStatus();
            smokePS.Stop();
        }

		private void FixedUpdate()
        {
            if (!IsHost) return;

			if (isAnyBroken)
			{
				if ((direction == VehicleDirection.Up && transform.position.z > 0) ||
					(direction == VehicleDirection.Down && transform.position.z < 0))
				{
					BrakeVehicle();
					return;
				}
				if (IsObstacleAhead(out float distance) && distance < stopDistance)
				{
					BrakeVehicle();
					return;
				}
			}
			else
			{
				if (IsObstacleAhead(out float distance) && distance < tmpDistance)
				{
					BrakeVehicle();
					return;
				}
			}
			
			// 일정 범위 이상 넘어가면 차량 삭제
			if (direction == VehicleDirection.Up && transform.position.z > removeLaneLength)
				TrafficManager.Instance.DespawnCar(this);
			else if (direction == VehicleDirection.Down && transform.position.z < -removeLaneLength)
				TrafficManager.Instance.DespawnCar(this);


			if (isBeingControlled) return; // 다른 곳에서 통제되고 있을 때 (ex. 차이는 코루틴 실행 중일 때) return
			MoveForward();
		}

		private Vector3 moveVector = new Vector3(0f, 0f, 5f);
		private float currentSpeedVelocityRef = 0f; // smooth damp용
        private float accelerationTime = 1f; // 목표 속도까지 도달하는 데 걸리는 대략적인 시간
        private void MoveForward()
		{
			Debug.Log("MovingMoving");
			Vector3 pos = rigid.position;
			float xOffset = targetLaneX - pos.x;

			Quaternion targetRot;

			if (Mathf.Abs(xOffset) > laneSnapThreshold)
			{
				float steerAmount = Mathf.Clamp(xOffset * steeringStrength, -maxSteerAngle, maxSteerAngle);

				if (direction == VehicleDirection.Up)
				{
					targetRot = Quaternion.Euler(0f, steerAmount, 0f);
				}
				else
				{
					targetRot = Quaternion.Euler(0f, 180f - steerAmount, 0f);
				}
			}
			else
			{
				targetRot = Quaternion.Euler(0f, direction == VehicleDirection.Up ? 0f : 180f, 0f);

				pos.x = targetLaneX;
				rigid.position = pos;
			}

			rigid.MoveRotation(Quaternion.Slerp(rigid.rotation, targetRot, Time.fixedDeltaTime * 2f));

            float currentMagnitude = rigid.linearVelocity.magnitude;

            // 목표 속도(moveSpeed)까지 부드럽게 가속합니다.
            float newSpeedMagnitude = Mathf.SmoothDamp(
                currentMagnitude,      // 현재 속도 크기
                moveSpeed,           // 목표 속도 크기
                ref currentSpeedVelocityRef, // 현재 속도 변화율 (SmoothDamp가 업데이트)
                accelerationTime / 10,      // 목표 속도 도달 시간
                Mathf.Infinity,        // 최대 가속도 (제한 없음)
                Time.fixedDeltaTime    // FixedUpdate 시간 간격
            );

            Vector3 forwardDirection = direction == VehicleDirection.Up ? Vector3.forward : -Vector3.forward;

			rigid.linearVelocity = forwardDirection * newSpeedMagnitude;
        }

        private float stopThreshold = 0.05f;
		[SerializeField] private float decelerationRate = 1f;
        private void BrakeVehicle()
		{
			Debug.Log("Braking");
			if (rigid.linearVelocity.magnitude > stopThreshold)
			{
				rigid.linearVelocity = Vector3.Lerp(rigid.linearVelocity, Vector3.zero, Time.fixedDeltaTime * decelerationRate);
            }
			else
			{
				rigid.linearVelocity = Vector3.zero;
				rigid.angularVelocity = Vector3.zero;
			}
		}

        public void InteractWithPart(CarParts part, PlayerController player, OwnableProp prop)
		{
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
					else if(!carStatus.IsTireEmpty(part) && carStatus.IsBroken(part) && prop is WrenchProp)
					{
						ProgressFixGageServerRPC(part, Time.deltaTime, NetworkManager.Singleton.LocalClientId);
					}
					break;
				case CarParts.Oil:
				case CarParts.Engine:
					ProgressFixGageServerRPC(part, Time.deltaTime, NetworkManager.Singleton.LocalClientId);
					break;
			}
		}

        private void AddTireLogic(CarParts part)
        {
            carStatus.AddTire(part);
            UIManager.Game.OnTireInserted(this, part);
            RevealTire(part);
        }
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

		private float fixingTime = 3f; // 고치는데 걸리는 시간
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

			//여기서 UI 게이지 업데이트
			UIManager.Game.ApplyProgressToUI(part, carStatus.Progress[(int)part], this);

            ApplyProgressWithUIClientRPC(part, carStatus.GetProgress(part));
		}
		[ClientRpc]
		private void ApplyProgressWithUIClientRPC(CarParts part, float progress)
		{
			if (IsHost) return;

			carStatus.Progress[(int)part] = progress;
            UIManager.Game.ApplyProgressToUI(part, carStatus.Progress[(int)part], this);
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
        [ClientRpc]
        private void OnAllPartsRepairedClientRPC()
        {
			if (!IsHost)
				isAnyBroken = false;

			Managers.Sound.PlaySfx(SFXType.Complete, .8f, .9f);
            allRepairedVFX.Play();
			VFXManager.Instance.PlayVFX(VFXType.PopEmoteGood, Vector3.up * 2, Quaternion.identity, transform);
        }
		private IEnumerator OnAllPartsRepairedCoroutine()
		{
			// material 값 만져서 빤짝거리게 해야되는데 toonShader적용하고 나서 바꿀 예정
			yield return null;
		}

        public bool IsAbleToInteract(CarParts part, OwnableProp prop)
		{
			if (!carStatus.IsBroken(part)) return false;

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
		private bool IsObstacleAhead(out float hitDistance)
		{
			Vector3 boxCenter = transform.position + Vector3.up * (boxHeight * 0.5f) + transform.forward * (boxLength * 0.5f);
			Vector3 halfExtents = new Vector3(boxWidth * 0.5f, boxHeight * 0.5f, boxLength * 0.5f);
			Quaternion orientation = transform.rotation;

			int hitCount = Physics.OverlapBoxNonAlloc(boxCenter, halfExtents, hitResults, orientation, obstacleLayer);

			if (hitCount > 0)
			{
				float closestDist = float.MaxValue;

				for (int i = 0; i < hitCount; i++)
				{
					if (hitResults[i].transform.IsChildOf(transform)) continue;

					float dist = Vector3.Distance(transform.position, hitResults[i].ClosestPoint(transform.position));
					if (dist < closestDist)
						closestDist = dist;
				}

				if (closestDist < float.MaxValue)
				{
					hitDistance = closestDist;
					return true;
				}
			}

			hitDistance = Mathf.Infinity;
			return false;
		}

		private VehicleDirection direction = VehicleDirection.None;
		public VehicleDirection Direction { get => direction; }
		private Quaternion originRot;
        public void SetLane(float laneX, float removeLength, VehicleDirection dir)
		{
			SetLaneClientRPC(laneX, removeLength, dir);
        }

		[ClientRpc]
		private void SetLaneClientRPC(float laneX, float removeLength, VehicleDirection dir)
		{
            targetLaneX = laneX;
			removeLaneLength = removeLength;
            direction = dir;
			switch (direction)
			{
				case VehicleDirection.Up:
                    originRot = Quaternion.Euler(0f, 0f, 0f);
                    break;
				case VehicleDirection.Down:
					originRot = Quaternion.Euler(0f, -180f, 0f);
                    break;
			}
        }

		private void OnDrawGizmosSelected()
		{
			Vector3 boxCenter = transform.position + Vector3.up * (boxHeight * 0.5f) + transform.forward * (boxLength * 0.5f);
			Vector3 halfExtents = new Vector3(boxWidth * 0.5f, boxHeight * 0.5f, boxLength * 0.5f);
			Quaternion orientation = transform.rotation;

			Gizmos.color = Color.red;
			Gizmos.matrix = Matrix4x4.TRS(boxCenter, orientation, Vector3.one);
			Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2);
		}
        public void InitCarStatusServer()
        {
			InitCarStatusLogic();
            InitCarStatusClientRPC(carStatus.isBroken);
        }
		private void InitCarStatusLogic()
		{
			for(int i = 0; i < 4; i++)
			{
				if (carStatus.IsBroken((CarParts)i))
				{
					HideTire((CarParts)i);
                }
			}

            if (carStatus.IsBroken(CarParts.Engine))
                smokePS.Play();

            UIManager.Game.GenerateCarStatusUIs(this, carStatus);
        }
		[ClientRpc]
		private void InitCarStatusClientRPC(int carStatusIsBroken)
        {
            if (IsHost) return;
			carStatus.isBroken = carStatusIsBroken;

            InitCarStatusLogic();
        }

		private void HideTire(CarParts part)
		{
            Renderer rend = PartTransforms[(int)part].GetComponent<Renderer>();
			MeshCollider collid = PartTransforms[(int)part].GetComponent<MeshCollider>();
            rend.enabled = false;
			collid.isTrigger = true;
        }

		private void RevealTire(CarParts part)
		{
            Renderer rend = PartTransforms[(int)part].GetComponent<Renderer>();
            MeshCollider collid = PartTransforms[(int)part].GetComponent<MeshCollider>();
            rend.enabled = true;
            collid.isTrigger = false;
			RestoreOriginRot(1f);
        }

        private Coroutine rotationCoroutine;
        private void RestoreOriginRot(float rotationDuration)
        {
            if (rotationCoroutine != null)
            {
                StopCoroutine(rotationCoroutine);
            }

            rotationCoroutine = StartCoroutine(RotateOverTime(rotationDuration));
        }
        private IEnumerator RotateOverTime(float time)
        {
			float originRotY = originRot.eulerAngles.y;
            float elapsedTime = 0f;
            while (elapsedTime < time)
            {
                rigid.rotation = Quaternion.Slerp(
					rigid.rotation, 
					Quaternion.Euler(rigid.rotation.eulerAngles.x, originRotY, rigid.rotation.eulerAngles.z),
					elapsedTime / time
					);
				elapsedTime += Time.fixedDeltaTime;

                yield return new WaitForFixedUpdate();
            }
			// 위치도 이참에 targetLaneX로 옮겨줄까 싶은데 일단 보류
            rigid.rotation = Quaternion.Euler(rigid.rotation.eulerAngles.x, originRotY, rigid.rotation.eulerAngles.z);
            rotationCoroutine = null;
        }

        Coroutine kickedCoroutine;
        public void ApplyKick(KickDirection kickDir)
        {
			float distanceByLane = TrafficManager.Instance.CurMapLaneWidth / 3f;
			float distance = distanceByLane > 0 ? distanceByLane : -distanceByLane; // distance는 절댓값으로 받음
			// 맵 월드좌표는 오른쪽이 +X방향임
			float distanceX;

            if ((direction == VehicleDirection.Up && kickDir == KickDirection.Right) ||
				(direction == VehicleDirection.Down && kickDir == KickDirection.Left))
			{
                // 왼쪽으로 기우는 애니메이션 실행  (차량 기우는 기준은 운전자 시점)
                // IsKickedToLeft(animationParameter) = true
            }
			else
			{
                // IsKickedToRight(animationParameter) = true
            }

            if (kickDir == KickDirection.Right)
            {
                distanceX = -distance;
            }
            else
            {
                distanceX = distance;
            }

			if (kickedCoroutine == null)
				kickedCoroutine = StartCoroutine(MoveSideways(distanceX, 1f));
        }
        private IEnumerator MoveSideways(float distanceX, float time)
        {
			isBeingForced = true;
			isBeingControlled = true;
            Vector3 startPos = new Vector3(rigid.position.x, rigid.position.y, rigid.position.z);
			Vector3 targetPos = startPos;
			targetPos.x += distanceX;

            float elapsedTime = 0f;

            while (elapsedTime < time)
            {
                float normalizedT = elapsedTime / time;

                float easeOutQuintT = 1 - Mathf.Pow(1 - normalizedT, 5);
				rigid.MovePosition(Vector3.Lerp(startPos, targetPos, easeOutQuintT));

                elapsedTime += Time.deltaTime;
                yield return null;
            }
            isBeingForced = false;
            rigid.MovePosition(targetPos);

			// 여기에 emote말풍선 vfx
			yield return new WaitForSeconds(1f);
			isBeingControlled = false;

            kickedCoroutine = null;
            Debug.Log("Moving complete.");
        }

		[SerializeField] float kickDistance = 3f;
		[Button]
		public void MovingTest()
		{
            kickedCoroutine = StartCoroutine(MoveSideways(kickDistance, 1f));
        }
    }
}