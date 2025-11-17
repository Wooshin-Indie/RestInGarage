using Garage.Controller.StateMachine;
using Garage.Manager;
using Garage.Props;
using Garage.Structs;
using Garage.Structs.CarPart;
using Garage.Utils;
using IUtil;
using Manager;
using System;
using System.Collections;
using System.Collections.Generic;
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
		[SerializeField] private Transform hipTf;
		[SerializeField] private float knockbackStrength = 5f;
		//[SerializeField] private Transform cameraTransform;

		[FoldoutGroup("Player Speeds")]
		[SerializeField] private float walkSpeed;
		[SerializeField] private float runSpeed;
		[SerializeField] private float carrySpeed;

		[FoldoutGroup("Ray Settings")]
		[SerializeField] private float boxWidth;
		[SerializeField] private float boxHeight;

        [FoldoutGroup("Fire Extinguish")]
        [SerializeField] private float fireExLength = 5f;
		[SerializeField] private float fireExRadius = 1f;
		[SerializeField] private LayerMask fireExLayer;

        [FoldoutGroup("Tire Rolling")]
        [SerializeField] private float rollForce;
        [SerializeField] private float rollDuration; // rollDuration 만큼 지난 후에 gage가 max 찍음

        [TabGroup("Main", "Rendering")]
		[SerializeField] private SkinnedMeshRenderer meshRenderer;
		[SerializeField] private List<Material> playerMaterial = new();
		[SerializeField] private LayerMask transparentLayer;


		private int[] animIDs = new int[20];
		private RaycastHit[] hits = new RaycastHit[10];

		private float originWalkSpeed;
        private float originRunSpeed;
        private float originCarrySpeed;
		private float wrenchRepairSpeed = 1f;
    
        
		public Transform HipTf => hipTf;
		public bool IsRun { get => Managers.Input.IsAbleToRun ? Managers.Input.Control.Player.Run.IsPressed() : false; }

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
		public ActionState actionState;
		public InteractState interactState;

		private Vector3 camDir;

		#region Unity Methods
		private void Awake()
		{
			interactableHits = new Collider[10];

			animator = GetComponent<Animator>();
			rigid = GetComponent<Rigidbody>();
			capsule = GetComponent<CapsuleCollider>();

			stateMachine = new PlayerStateMachine();
			idleState = new IdleState(this, stateMachine);
			carryState = new CarryState(this, stateMachine);
			actionState = new ActionState(this, stateMachine);
			interactState = new InteractState(this, stateMachine);
			stateMachine.Init(idleState);

            rigid.maxLinearVelocity = 500f;

			animIDs[0] = Animator.StringToHash(Constants.ANIM_PARAM_CARRY);
			animIDs[1] = Animator.StringToHash(Constants.ANIM_PARAM_SPEED);
			animIDs[2] = Animator.StringToHash(Constants.ANIM_PARAM_OIL);
			animIDs[3] = Animator.StringToHash(Constants.ANIM_PARAM_PLACE);
			animIDs[4] = Animator.StringToHash(Constants.ANIM_PARAM_TIREPUT);
			animIDs[5] = Animator.StringToHash(Constants.ANIM_PARAM_HAMMER);
			animIDs[6] = Animator.StringToHash(Constants.ANIM_PARAM_KICK);
			animIDs[7] = Animator.StringToHash(Constants.ANIM_PARAM_KNOCKBACK);
			animIDs[8] = Animator.StringToHash(Constants.ANIM_PARAM_CARRY_MULT); 
			animIDs[9] = Animator.StringToHash(Constants.ANIM_PARAM_FIX); 
			animIDs[10] = Animator.StringToHash(Constants.ANIM_PARAM_TIREROLL); 
			animIDs[11] = Animator.StringToHash(Constants.ANIM_PARAM_THROW);

			originWalkSpeed = walkSpeed;
			originCarrySpeed = carrySpeed;
			originRunSpeed = runSpeed;
			originalConstraints = rigid.constraints;
        }
        private void Start()
		{
			GameManagerEx.Instance.OnStartGameAction += InitPlayerCamera;
		}
		private void Update()
        {
            if (!IsOwner) return;

			if (Managers.Input.IsInputEnabled)
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

		void OnDrawGizmos()
		{
			Gizmos.color = Color.cyan;

			Vector3 start = transform.position;
			Vector3 end = transform.position + transform.forward * fireExLength;

			Debugger.DrawCapsuleGizmo(transform, start, end, fireExRadius);
		}
		#endregion

		#region Setters
		private void InitPlayerCamera(int mapIdx)
		{
			Quaternion rot = Quaternion.Euler(TrafficManager.Instance.CurMapData.CamRotation);
			camDir = rot * Vector3.forward;
			camDir = camDir.normalized;
		}
		#endregion

		#region Getters
		public Transform GetSocket(PropType type)
		{
			return sockets[(int)type];
		}
		#endregion

		#region Basic Moves
		private Vector3 moveDir = Vector3.zero;
        /// <summary>
        /// move 방향으로 speed의 속도로 움직입니다.
        /// maxSpeed 로는 Animation의 BlendTree 값을 조절합니다.
        /// move의 x,y축은 인게임카메라를 기준으로 함
        /// </summary>
        public void MovePosition(Vector2 move, float speed, float maxSpeed)
		{
			if (!Managers.Input.IsAbleToMove)
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

		/// <summary>
		/// 마우스 방향으로 몸을 회전시킵니다.
		/// 기본적으로 Player.Move 입력이 막힌다는 가정하에 작성되었습니다.
		/// </summary>
		public void RotateToMousePos()
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			Vector3 rayOrigin = ray.origin;
			Vector3 rayDir = ray.direction;

			float t = -rayOrigin.y / rayDir.y;
			Vector3 lookDir = rayOrigin + rayDir * t - transform.position;
			lookDir.y = 0f;

			if (lookDir.sqrMagnitude > 0.001f)
			{
				Quaternion targetRot = Quaternion.LookRotation(lookDir);
				rigid.MoveRotation(targetRot);
			}
		}
		#endregion

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
            Managers.Input.DisablePlayerInputs();
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

			if (IsHost)
			{
				GameManagerEx.Instance.StartEvent_HostOnly();
			}
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

		public void OnUpdateInteractSpeedBoosts()
		{
            StatManager.Instance.UpdateInteractSpeedBoosts(currentOwningProp, Managers.Input.Control.Player.Interact.IsPressed());
        }


		private float gageValue = 0f;	// [0f, 1f]
        private bool isGageUpward = true;
		private bool isChargeStarted = false;
		public bool IsChargeStarted { get => isChargeStarted; set => isChargeStarted = value; }

        public void OnUpdatePlayerGage()
		{
			if (!isChargeStarted)
			{
				isChargeStarted = true;
				Managers.Input.DisablePlayerMove();
                gageValue = 0f;
                UIManager.Game.PopPlayerGageUI(transform);
                UIManager.Game.SetPlayerGageUI(gageValue);

                return;
			}

			// 게이지가 위아래로 왔다갔다 하도록
			float rollGageDelta = Time.deltaTime / rollDuration;
            if (isGageUpward) // 게이지 방향 위쪽
            {
                if (gageValue > 1f)
					isGageUpward = false;
            }
			else // 게이지 방향 아래쪽
			{
				rollGageDelta = -rollGageDelta;
                if (gageValue < 0f)
				{
					gageValue = 0f;
                    isGageUpward = true;
                }
            }
			gageValue += rollGageDelta;

            UIManager.Game.SetPlayerGageUI(gageValue);
        }
		public float GetTireRollingForce()
		{
			float overallRollingForce = rollForce * gageValue;

			if (overallRollingForce < 0f)
				overallRollingForce = 0f;

            return overallRollingForce;
        }
		public void CloseGageUI()
		{
			isChargeStarted = false;
			UIManager.Game.ClosePlayerGageUI();
		}
    }
}
