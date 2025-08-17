using DG.Tweening;
using Garage.Controller.StateMachine;
using Garage.Manager;
using Garage.Props;
using Garage.Structs;
using Garage.Structs.CarPart;
using Garage.Utils;
using Garage.Vehicle;
using IUtil;
using Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Controller
{
	[RequireComponent(typeof(Animator))]
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(CapsuleCollider))]
	public partial class PlayerController : NetworkBehaviour
	{
		/** Components **/
		private Animator animator;
		private Rigidbody rigid;
		private CapsuleCollider capsule;

		public Rigidbody Rigid => rigid;

		[TabGroup("Main", "Movements")]
		[SerializeField] private List<Transform> sockets = new();
		[SerializeField] private float knockbackStrength = 5f;
		//[SerializeField] private Transform cameraTransform;

		[FoldoutGroup("Player Speeds")]
		[SerializeField] private float walkSpeed;
		[SerializeField] private float runSpeed;
		[SerializeField] private float carrySpeed;

		[FoldoutGroup("Ray Settings")]
		[SerializeField] private float boxWidth;
		[SerializeField] private float boxHeight;

		[SerializeField] private float fireExLength = 5f;
		[SerializeField] private float fireExRadius = 1f;
		[SerializeField] private LayerMask fireExLayer;

		[TabGroup("Main", "Rendering")]
		[SerializeField] private SkinnedMeshRenderer meshRenderer;
		[SerializeField] private List<Material> playerMaterial = new();


		private int[] animIDs = new int[10];

		
		private bool isAbleToMove = true;
		private bool isAbleToRun = true;
		private bool isInputLocked = false;
        
		public bool IsAbleToRun { get => isAbleToRun; set => isAbleToRun = value; }
		public bool IsRun { get => IsAbleToRun ? Managers.Input.Control.Player.Run.IsPressed() : false; }


        private float originWalkSpeed;
        private float originRunSpeed;
        private float originCarrySpeed;
		private float wrenchRepairSpeed = 1f;
    
        
		public float WalkSpeed => walkSpeed;
		public float RunSpeed => runSpeed;
		public float CarrySpeed => carrySpeed;
		public float WrenchRepairSpeed => wrenchRepairSpeed;

		private Collider[] interactableHits = null;
		
		private OwnableProp recentlyDetectedProp = null;
		private OwnableProp currentOwningProp = null;
		private CarPartBase currentFixablePart = null;
		private CarController currentKickableCar = null;
		private CarPartBase preEnlargedFixablePart = null;

		public OwnableProp CurrentOwningProp => currentOwningProp;
		public OwnableProp RecentlyDetectedProp => recentlyDetectedProp;
		public CarPartBase CurrentFixablePart => currentFixablePart;

		/** Player State Machine **/
		private PlayerStateMachine stateMachine;
		public PlayerStateMachine StateMachine { get => stateMachine; }

		public IdleState idleState;
		public CarryState carryState;
		public InteractState interactState;

		private Vector3 camDir;

		private void Awake()
		{
			interactableHits = new Collider[10];

			animator = GetComponent<Animator>();
			rigid = GetComponent<Rigidbody>();
			capsule = GetComponent<CapsuleCollider>();

			stateMachine = new PlayerStateMachine();
			idleState = new IdleState(this, stateMachine);
			carryState = new CarryState(this, stateMachine);
			interactState = new InteractState(this, stateMachine);
			stateMachine.Init(idleState);

            rigid.maxLinearVelocity = 500f;

			Debug.Log("game" + gameObject.layer);


			animIDs[0] = Animator.StringToHash(Constants.ANIM_PARAM_CARRY);
			animIDs[1] = Animator.StringToHash(Constants.ANIM_PARAM_SPEED);
			animIDs[2] = Animator.StringToHash(Constants.ANIM_PARAM_OIL);
			animIDs[3] = Animator.StringToHash(Constants.ANIM_PARAM_PLACE);
			animIDs[4] = Animator.StringToHash(Constants.ANIM_PARAM_TIREPUT);
			animIDs[5] = Animator.StringToHash(Constants.ANIM_PARAM_HAMMER);
			animIDs[6] = Animator.StringToHash(Constants.ANIM_PARAM_CROUCH);
			animIDs[7] = Animator.StringToHash(Constants.ANIM_PARAM_KICK);
			animIDs[8] = Animator.StringToHash(Constants.ANIM_PARAM_KNOCKBACK);
			animIDs[9] = Animator.StringToHash(Constants.ANIM_PARAM_CARRY_MULT);
      

			originWalkSpeed = walkSpeed;
			originCarrySpeed = carrySpeed;
			originRunSpeed = runSpeed;
			originalConstraints = rigid.constraints;
        }

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			//cameraTransform.gameObject.SetActive(IsOwner);
			if (IsOwner)
			{
				Debug.Log("NetworkSpawned");
				// OnNetworkSpawn 이 SceneManager.sceneLoaded 이벤트보다 먼저 실행됨
				CameraManager.Instance.SetTargetPlayer(this.transform);
			}

            PlayerID.OnValueChanged += OnPlayerIDChanged;
		}

		public override void OnNetworkDespawn()
		{
			base.OnNetworkDespawn();
			
			Debug.Log("Character On NetworkDeSpawn , " + System.Environment.StackTrace);
        }


        private void OnDestroy()
        {
			Debug.Log("Character On Destroy , " + System.Environment.StackTrace);
        }
        private void Start()
		{
			GameManagerEx.Instance.OnStartGameAction += SetMapInfo;
		}

		private void Update()
		{
			if (!IsOwner) return;

			UpdateSizeOfFireUIs();
			if (!isInputLocked)
			{
				stateMachine.CurState.HandleInput();
				stateMachine.CurState.LogicUpdate();
			}

			OnUpdateSynchronization();

			// HACK
			if (Input.GetKeyDown(KeyCode.T))
			{
				GameNetworkManager.Instance.OpenInviteWindow();
			}
		}

		private Vector3 moveDir = Vector3.zero;
        /// <summary>
        /// move 방향으로 speed의 속도로 움직입니다.
        /// maxSpeed 로는 Animation의 BlendTree 값을 조절합니다.
        /// move의 x,y축은 인게임카메라를 기준으로 함
        /// </summary>
        public void MovePosition(Vector2 move, float speed, float maxSpeed)
		{
			if (!isAbleToMove)
			{
				rigid.linearVelocity = Vector3.zero;
				SetAnimParam((int)AnimationType.Speed, 0);
				return;
			}

			if (isKnockedBack) return;

			if(Mathf.Approximately(Mathf.Abs(move.x) + Mathf.Abs(move.y), 0f)) 
				speed = 0;

			moveDir = new Vector3(move.y, 0f, -move.x).normalized;
            // 이렇게 게임 축에 맞게 바꿔놨기때문에 transform좌표로 직접 메소드를 호출해서 쓰려면 move 인자를
			// Vector2(transform.position.z - targetPos.z, targetPos.x - transform.position.x)
			// 이렇게 보내야됨
            moveDir *= speed;
			rigid.linearVelocity = moveDir;

			if (moveDir.sqrMagnitude > .1f)
				rigid.MoveRotation(Quaternion.LookRotation(moveDir));

			SetAnimParam((int)AnimationType.Speed, speed / maxSpeed);
		}

		private void SetMapInfo(int mapIdx)
		{
			Quaternion rot = Quaternion.Euler(TrafficManager.Instance.CurStageData.CamRotation);
			camDir = rot * Vector3.forward;
			camDir = camDir.normalized;
		}
		public Transform GetSocket(PropType type) 
		{
			return sockets[(int)type];
		}

		// 키 바인딩 필요
        public void KickCar()
		{
            if (currentKickableCar == null) return;
			// HACK - if (currentKickableCar.CarStatus.IsThereAnyBroken()) return;

            // 차는 애니메이션 실행
            Managers.Input.DisablePlayerActions();
            SetAnimParam((int)AnimationType.Kick);
        }


		private bool isFireUIsEnlarged = false;
		private void UpdateSizeOfFireUIs()
		{
			if (currentOwningProp is not Extinguisher)
			{
				if (isFireUIsEnlarged)
				{
					isFireUIsEnlarged = false;
					UIManager.Game.ReduceAllFireUIs();
				}
				return;
			}

			isFireUIsEnlarged = true;
			UIManager.Game.EnlargeAllFireUIs();
		}
        
        private RigidbodyConstraints originalConstraints;
		private bool isKnockedBack = false;
		[ClientRpc]
        public void KnockBackClientRPC(Vector3 knockbackDirection, float force)
		{
			if (!IsOwner) return;
			if (isKnockedBack) return;
            // 튕겨나갈 방향 계산
            knockbackDirection = knockbackDirection.normalized;
            Debug.Log("Collision to Player2");

            Vector3 targetRotVector3 = -knockbackDirection;
            targetRotVector3.y = 0;
            Quaternion targetRot = Quaternion.LookRotation(targetRotVector3);
            rigid.rotation = targetRot;

			// 플레이어 움직임 Lock걸기
            Managers.Input.DisablePlayerActions();
            rigid.constraints = RigidbodyConstraints.FreezeRotation | originalConstraints;
            EndAllInteraction();

            rigid.AddForce(knockbackDirection * force, ForceMode.Impulse);
			isKnockedBack = true;

            // 애니메이션 실행
            SetAnimParam((int)AnimationType.KnockBack);
        }
		private IEnumerator OnKnockbackCoroutine()
		{
            AnimationClip clip = animator.runtimeAnimatorController.animationClips[(int)AnimationType.KnockBack];
			float clipLength = clip.length;
            Vector3 startPosition = transform.position; // 현재 시작 위치
            float elapsedTime = 0f; // 경과 시간

            while (elapsedTime < clipLength)
            {
                // 1. 시간에 따른 진행도(t) 계산 (0.0에서 1.0까지)
                float t = elapsedTime / clipLength;

                // 2. 이징 함수를 적용하여 진행도를 변환합니다.
                //    원하는 Ease 함수로 바꿔보세요!
                //    float easedT = t; // 선형(Linear) 이동
                //    float easedT = EasingUtils.EaseInQuad(t);
                float easedT = t * (2 - t); ; // 부드럽게 감속
                                                           //    float easedT = EasingUtils.EaseInOutSine(t); // 더 부드럽게 가감속

                // 3. Vector3.Lerp를 사용하여 변환된 진행도(easedT)에 따라 위치를 계산
                //    Lerp(시작, 끝, 진행도)
                //transform.position = Vector3.Lerp(startPosition, targetPosition.position, easedT);

                // 4. 경과 시간을 증가시키고 다음 프레임까지 대기
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
		void OnDrawGizmos()
		{
			Gizmos.color = Color.cyan;

			Vector3 start = transform.position;
			Vector3 end = transform.position + transform.forward * fireExLength;

			Debugger.DrawCapsuleGizmo(transform, start, end, fireExRadius);
		}

		private CarController curTransparentCar = null;
		private CarController tmpRaycastedCar = null;
		private RaycastHit[] hits = new RaycastHit[10];
		[SerializeField] private LayerMask transparentLayer;
		public void DetectFrontCarAndMakeTransparent()
		{
			int count = Physics.RaycastNonAlloc(transform.position, -camDir, hits, 3f, transparentLayer);
			Debug.DrawRay(transform.position, -camDir * 3f, Color.red);

            tmpRaycastedCar = null; // raycast 된거 없을 때 처리
            for (int i = 0; i < count; i++)
			{
                tmpRaycastedCar = hits[i].transform.GetComponent<CarController>();

                if (tmpRaycastedCar != null)
				{
					break;
				}
            }

			if (curTransparentCar == tmpRaycastedCar)
			{
                return;
            }

			if (curTransparentCar == null && tmpRaycastedCar != null) // 원래 투명화된 차량 없었을 때
			{
				curTransparentCar = tmpRaycastedCar;
				curTransparentCar.MakeCarBodyTransparent(); // 차량 투명화 함수 실행
				Debug.Log("새로 투명화");
            }
            else if (curTransparentCar != null && tmpRaycastedCar == null) // 투명화된 차량 있는데 밖으로 벗어났을 때
			{
				curTransparentCar.RestoreCarBodyTransparency(); // 차량 복원 함수 실행
                curTransparentCar = null;
                Debug.Log("차량 복원");
            }
			else // 투명화된 차량이 있는데 새로운 차량이 raycast 됐을 때
			{
                curTransparentCar.RestoreCarBodyTransparency(); // 차량 복원 함수 실행
                curTransparentCar = tmpRaycastedCar;
                curTransparentCar.MakeCarBodyTransparent(); // 차량 투명화 함수 실행
                Debug.Log("기존 차량 복원 및 새로 투명화");
            }
        }

		public void AwayFromLanesOnStageEnd_HostOnly(float awayMoveTime)
		{
			int curMapIdx = GameSynchronizer.Instance.MapIdx.Value;

			List<LaneData> spawnPoints = Managers.Resource.GetData<MapData>(curMapIdx).SpawningPoints;
            int laneNum = spawnPoints.Count;
			float laneWidthHalf = Managers.Resource.GetData<MapData>(curMapIdx).LaneWidth / 2;

            Vector2[] laneXwidths = new Vector2[laneNum]; // 차선 폭 계산해서 거기서 벗어나게 함
			for (int i = 0; i < laneNum; i++)
			{
				float laneX = spawnPoints[i].SpawnPointX;
                laneXwidths[i].x = laneX - laneWidthHalf;
				laneXwidths[i].y = laneX + laneWidthHalf;
            }

            AwayFromLanesOnStageEndClientRPC(awayMoveTime, laneXwidths);
        }

		[ClientRpc]
		private void AwayFromLanesOnStageEndClientRPC(float awayMoveTime, Vector2[] laneXwidths)
		{
			if (!IsOwner) return;

			Vector3 curPos = transform.position;
			Vector3 targetPos = curPos;

			for (int i = 0; i < laneXwidths.Length; i++)
			{
				if (laneXwidths[i].IsBetween(curPos.x))
				{
					targetPos.x = laneXwidths[i].GetCloserValue(curPos.x);

					break;
                }
            }

			if (targetPos.x != curPos.x)
				StartCoroutine(RunToTargetPosCoroutine(awayMoveTime, targetPos, () =>
				{
					Debug.Log("-transform.forward: " + -transform.rotation.eulerAngles);
					transform.rotation = Quaternion.Euler(-transform.rotation.eulerAngles);
				}));

            DOVirtual.DelayedCall(awayMoveTime + 3f, () =>
			{
				isInputLocked = false;
			});
        }

        private IEnumerator RunToTargetPosCoroutine(float maxTime, Vector3 targetPos, Action onComplete)
		{
			float elapsedTime = 0f;

            Vector2 moveDir = new Vector2(transform.position.z - targetPos.z, targetPos.x - transform.position.x);

			while (elapsedTime < maxTime)
			{
				if (Vector3.Distance(targetPos, transform.position) < 0.5f)
                {
                    break;
                }
                MovePosition(moveDir, runSpeed, runSpeed);

				elapsedTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            MovePosition(Vector2.zero, runSpeed, runSpeed);

			onComplete?.Invoke();
        }

        public void InputLockToPlayer_HostOnly()
        {
			InputLockToPlayerClientRPC();
        }

        [ClientRpc]
        private void InputLockToPlayerClientRPC()
        {
            if (!IsOwner) return;

            isInputLocked = true;
            rigid.linearVelocity = Vector3.zero;
            SetAnimParam((int)AnimationType.Speed, 0);
        }

        [ClientRpc]
        public void ApplyStatsClientRPC(StatEnum[] statEnums, float[] values)
        {
            for (int i = 0; i < statEnums.Length; i++)
			{
				ApplyStat(statEnums[i], values[i]);
			}
			Debug.Log("Apply Stats: " + statEnums + values );
        }

		private void ApplyStat(StatEnum statEnum, float value)
		{
			switch (statEnum)
			{
				case StatEnum.PlayerSpeed:
					// TODO - 스탯 적용
					walkSpeed = originWalkSpeed * value;
					runSpeed = originRunSpeed * value;
					break;
				case StatEnum.CarrySpeed:
					carrySpeed = originCarrySpeed * value;
					break;
				case StatEnum.WrenchRepairSpeed:
					wrenchRepairSpeed = value;
                    break;
			}
		}

		private float interactPropKeyInfoUITimer = 0f;
		private float idlePropKeyInfoUITimer = 0f;
		private float propKeyInfoUIDelay = 1.5f;
        public void UpdatePropKeyInfoUIs()
        {
			if (stateMachine.CurState == interactState)
			{
                UIManager.Game.ClosePropKeyInfoUI();
                interactPropKeyInfoUITimer = 0f;
                idlePropKeyInfoUITimer = 0f;

				return;
            }

            // interactPropKeyInfoUI condition
            if (currentFixablePart != null && currentFixablePart.IsAbleToInteract(currentOwningProp))
            {
                idlePropKeyInfoUITimer = 0f;

                interactPropKeyInfoUITimer += Time.deltaTime;
                if (interactPropKeyInfoUITimer > propKeyInfoUIDelay)
                {
					interactPropKeyInfoUITimer = 0f;
                    UIManager.Game.PopPropKeyInfoUI(currentOwningProp, currentFixablePart.transform, PlayerState.Interact);
                }
                return;
            }
            // carryPropKeyInfoUI condition
            else if (currentOwningProp != null)
            {
                interactPropKeyInfoUITimer = 0f;
                idlePropKeyInfoUITimer = 0f;

                UIManager.Game.PopPropKeyInfoUI(currentOwningProp, PlayerState.Carry);
				return;
            }
            // idlePropKeyInfoUI condition
            else if (recentlyDetectedProp != null && recentlyDetectedProp == prevDetectedProp)
            {
                interactPropKeyInfoUITimer = 0f;

                idlePropKeyInfoUITimer += Time.deltaTime;
                if (idlePropKeyInfoUITimer > propKeyInfoUIDelay)
                {
                    idlePropKeyInfoUITimer = 0f;
                    UIManager.Game.PopPropKeyInfoUI(recentlyDetectedProp, PlayerState.Idle);
                }
				return;
            }

            UIManager.Game.ClosePropKeyInfoUI();
            interactPropKeyInfoUITimer = 0f;
            idlePropKeyInfoUITimer = 0f;
        }
    }
}