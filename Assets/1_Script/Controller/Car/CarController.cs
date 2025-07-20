using Garage.Utils;
using Garage.Structs;
using IUtil;
using UnityEngine;
using Garage.Manager;
using System.Collections.Generic;
using Unity.Netcode;
using System.Collections;
using DG.Tweening;
using Garage.Vehicle;

namespace Garage.Controller
{
	/// <summary>
	/// CarController 의 움직임 및 생명주기 관련 코드를 포함합니다.
	/// </summary>
	public partial class CarController : VehicleBase
	{
		[SerializeField] private MeshRenderer meshRenderer;
		[SerializeField] private List<MeshRenderer> wheelRenderers = new();
		[SerializeField] private List<Transform> partTransforms = new();	// 넣을 때 CarParts enum 순서 맞춰서 넣기

		[FoldoutGroup("Materials")]
		[SerializeField] private Material brokenCarMat;

		[FoldoutGroup("Particle Systems")]
		[SerializeField] private ParticleSystem smokePS;
		[SerializeField] private ParticleSystem allRepairedVFX;
		[SerializeField] private ParticleSystem firePS;
		[SerializeField] private ParticleSystem extinguishPS;
		[SerializeField] private ParticleSystem explosionPS;

        [FoldoutGroup("Move Parameters")]
		[SerializeField, Tooltip("Basic velocity for vehicle")] 
		private float moveSpeed = 5f;
		
		[SerializeField, Tooltip("Distance to start braking to avoid a crash (IsAnybroken)")] 
		private float stopDistance = 15f;

		[SerializeField, Tooltip("Distance to start braking to avoid a crash (!IsAnybroken)")] 
		private float tmpDistance = 7f;

		[SerializeField, Tooltip("")] 
		private float steeringStrength;

		[SerializeField, Tooltip("최대 steering 각도")] 
		private float maxSteerAngle;
		
		[SerializeField, Tooltip("Lane에 충분히 가까워졌는지 판단하는 Threshold")] 
		private float laneSnapThreshold;

		[FoldoutGroup("Overlap Parameters")]
		[SerializeField] private float boxLength = 10f;
		[SerializeField] private float boxWidth = 1f;
		[SerializeField] private float boxHeight = 1f;
		[SerializeField] private LayerMask obstacleLayer;


		private float targetLaneX = 0f;
		private float removeLaneLength;
		private bool isBeingControlled = false;
		private bool isStageEnded = false;
		private bool isAnyBroken = true;
		public bool IsAnyBroken => isAnyBroken;
        
		private Collider[] hitResults = new Collider[30];
		private Material[] instanceMats;
		private Tween[] transparencyTweens;

        private CarStatus carStatus;
		public CarStatus CarStatus { get => carStatus; }
		public List<Transform> PartTransforms => partTransforms;

		private float gameoverTime = 0f;
		public float GameoverTime { get => gameoverTime; set => gameoverTime = value; }

		public bool IsStopped { get => rigid.linearVelocity.sqrMagnitude < 0.01; }

		private int[] animIDs = new int[2];

		private void Awake()
		{
            rigid = GetComponent<Rigidbody>();
			carStatus = new CarStatus();

			animIDs[0] = Animator.StringToHash("IsKickedToLeft");
			animIDs[1] = Animator.StringToHash("IsKickedToRight");

			smokePS.Stop();
			firePS.Stop();
			extinguishPS.Stop();
			explosionPS.Stop();
			allRepairedVFX.Stop();

			var mats = meshRenderer.materials;
			instanceMats = new Material[mats.Length];
			transparencyTweens = new Tween[mats.Length];
            int n = mats.Length;
			for(int i = 0; i < n; i++)
			{
				instanceMats[i] = Instantiate(mats[i]);
            }
			meshRenderer.materials = instanceMats;
        }

		private void FixedUpdate()
        {
			OnUpdateFire();

            if (!IsHost) return;

			if (!OnUpdateMoveLogic()) return;
			CheckIfOutofBounds();

			if (isBeingControlled) return; // 다른 곳에서 통제되고 있을 때 (ex. 차이는 코루틴 실행 중일 때) return
			MoveForward();
		}

		private bool OnUpdateMoveLogic()
		{
			if (isStageEnded)
			{
				MoveForward();
			}
			else if (isAnyBroken)
			{
				if ((direction == VehicleDirection.Up && transform.position.z > 0) ||
					(direction == VehicleDirection.Down && transform.position.z < 0))
				{
					BrakeVehicle();
					return false;
				}
				if (IsObstacleAhead(out float distance) && distance < stopDistance)
				{
					BrakeVehicle();
					return false;
				}
			}
			else
			{
				if (IsObstacleAhead(out float distance) && distance < tmpDistance)
				{
					BrakeVehicle();
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// 일정 범위 이상 넘어가면 차량 삭제
		/// </summary>
		private void CheckIfOutofBounds()
		{
			if (direction == VehicleDirection.Up && transform.position.z > removeLaneLength)
				TrafficManager.Instance.DespawnCar(this);
			else if (direction == VehicleDirection.Down && transform.position.z < -removeLaneLength)
				TrafficManager.Instance.DespawnCar(this);
		}


		private float currentSpeedVelocityRef = 0f;		// smooth damp용
        private float accelerationTime = 1f;            // 목표 속도까지 도달하는 데 걸리는 대략적인 시간
		private float stopThreshold = 0.05f;
		[SerializeField] private float decelerationRate = 1f;

		private VehicleDirection direction = VehicleDirection.None;
		public VehicleDirection Direction { get => direction; }
		private Quaternion originRot;

		/// <summary>
		/// Lane을 따라 움직이는 함수
		/// </summary>
		private void MoveForward()
		{
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
                currentMagnitude,				// 현재 속도 크기
                moveSpeed,						// 목표 속도 크기
                ref currentSpeedVelocityRef,	// 현재 속도 변화율 (SmoothDamp가 업데이트)
                accelerationTime / 10,			// 목표 속도 도달 시간
                Mathf.Infinity,					// 최대 가속도 (제한 없음)
                Time.fixedDeltaTime				// FixedUpdate 시간 간격
            );

            Vector3 forwardDirection = direction == VehicleDirection.Up ? Vector3.forward : -Vector3.forward;

			rigid.linearVelocity = forwardDirection * newSpeedMagnitude;
        }

		/// <summary>
		/// 앞에 장애물이 있을 경우, 자연스럽게 멈추는 함수
		/// </summary>
        private void BrakeVehicle()
		{
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

		/// <summary>
		/// 앞에 장애물이 있는지 판단하는 함수
		/// </summary>
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

		/// <summary>
		/// Spawn 직후에 차를 초기화하기 위한 함수
		/// </summary>
		public void InitCarController(VehicleSpawnPoint spawnPoint)
		{
			SetLaneClientRPC(spawnPoint.transform.position.x, Managers.Resource.GetData<MapData>(0).RemoveLength, spawnPoint.Direction);

			int vehicleDataIdx = UnityEngine.Random.Range(0, Managers.Resource.GetDataLength<VehicleData>());
			InitCarStatusLogic(vehicleDataIdx);
			InitCarStatusClientRPC(carStatus.isBroken, vehicleDataIdx);
		}

		[ClientRpc]
		private void SetLaneClientRPC(float laneX, float removeLength, VehicleDirection dir)
		{
			SetLaneLogic(laneX, removeLength, dir);
		}
		private void SetLaneLogic(float laneX, float removeLength, VehicleDirection dir)
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

		[ClientRpc]
		private void InitCarStatusClientRPC(int carStatusIsBroken, int vehicleDataIdx)
		{
			if (IsHost) return;
			carStatus.isBroken = carStatusIsBroken;
			InitCarStatusLogic(vehicleDataIdx);
		}
		private void InitCarStatusLogic(int vehicleDataIdx)
		{
			VehicleData data = Managers.Resource.GetData<VehicleData>(vehicleDataIdx);

			meshRenderer.materials = new Material[]
			{
				data.CarMaterial,
				data.CarMaterial
			};

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
        private IEnumerator MoveSideways(float distanceX, float time)
        {
			IsKnockbackOnHumanCollision = true;
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
            IsKnockbackOnHumanCollision = false;
            rigid.MovePosition(targetPos);

            targetLaneX += distanceX;

            yield return new WaitForSeconds(1f);
			isBeingControlled = false;

            kickedCoroutine = null;
        }

		public void MakeCarBodyTransparent()
		{
            Debug.Log("투명화 메소드 실행");
            int n = instanceMats.Length;
            for(int i = 0; i < n; i++)
            {
				int idx = i;
                if (transparencyTweens[idx] != null && transparencyTweens[idx].IsActive())
				{
                    transparencyTweens[idx].Kill();
                    Debug.Log("원래 restoreTweens 뒤짐");
                }
				
                float currentValue = instanceMats[idx].GetFloat("_Tweak_transparency");

                instanceMats[idx].SetInt("_TransparentEnabled", 1);
                instanceMats[idx].SetInt("_TransparentZWrite", 0);
                instanceMats[idx].SetFloat("_ARSampler_AlphaOn", 1f);
                instanceMats[idx].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                instanceMats[idx].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

                //instanceMats[idx].EnableKeyword("");
                transparencyTweens[idx] = DOTween.To(() => currentValue, x =>
                {
                    currentValue = x;
                    instanceMats[idx].SetFloat("_Tweak_transparency", x);
                }, -0.8f, 1f);
            }
        }
		public void RestoreCarBodyTransparency()
		{
            
            Debug.Log("복원 메소드 실행");
            int n = instanceMats.Length;
            for (int i = 0; i < n; i++)
            {
				int idx = i;
				if (transparencyTweens[idx] != null && transparencyTweens[idx].IsActive())
				{
                    transparencyTweens[idx].Kill();
                    Debug.Log("원래 makeTweens 뒤짐");
                }

                float currentValue = instanceMats[idx].GetFloat("_Tweak_transparency");

                transparencyTweens[idx] = DOTween.To(() => currentValue, x =>
                {
                    currentValue = x;
                    instanceMats[idx].SetFloat("_Tweak_transparency", x);
                }, 0f, 1f).OnComplete(() =>
				{
                    instanceMats[idx].SetInt("_TransparentEnabled", 0);
                });
            }
        }

		public void OnStageEnd()
		{
			moveSpeed = moveSpeed * 4;
			isStageEnded = true;
		}
		public bool IsInBoundary()
		{
			if (Mathf.Abs(transform.position.z) < 20f)
				return true;
			else
				return false;
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
	}
}