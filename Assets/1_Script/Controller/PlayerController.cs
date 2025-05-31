using Garage.Controller.StateMachine;
using Garage.Interfaces;
using Garage.Manager;
using Garage.Props;
using Garage.Structs.CarPart;
using Garage.Utils;
using IUtil;
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


		private int[] animIDs = new int[9];

		
		private bool isAbleToMove = true;
		private bool isBeingForced = false;
		public bool IsBeingForced => isBeingForced;
		public float WalkSpeed => walkSpeed;
		public float RunSpeed => runSpeed;
		public float CarrySpeed => carrySpeed;

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

			Quaternion rot = Quaternion.Euler(TrafficManager.Instance.CurStageData.CamRotation);
			camDir = rot * Vector3.forward;
			camDir = camDir.normalized;

			animIDs[0] = Animator.StringToHash(Constants.ANIM_PARAM_CARRY);
			animIDs[1] = Animator.StringToHash(Constants.ANIM_PARAM_SPEED);
			animIDs[2] = Animator.StringToHash(Constants.ANIM_PARAM_OIL);
			animIDs[3] = Animator.StringToHash(Constants.ANIM_PARAM_PLACE);
			animIDs[4] = Animator.StringToHash(Constants.ANIM_PARAM_TIREPUT);
			animIDs[5] = Animator.StringToHash(Constants.ANIM_PARAM_HAMMER);
			animIDs[6] = Animator.StringToHash(Constants.ANIM_PARAM_CROUCH);
			animIDs[7] = Animator.StringToHash(Constants.ANIM_PARAM_KICK);
			animIDs[8] = Animator.StringToHash(Constants.ANIM_PARAM_KNOCKBACK);
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

		private void Update()
		{
			if (!IsOwner) return;

			stateMachine.CurState.HandleInput();
			stateMachine.CurState.LogicUpdate();

			OnUpdateSynchronization();
		}

		/// <summary>
		/// Controller가 Interact를 시작하고 싶을 때 사용합니다.
		/// </summary>
		public void TryStartInteract()
		{
			if (recentlyDetectedProp == null) return;

			if (GameManagerEx.Instance.IsDay)
			{
				recentlyDetectedProp.TryInteract(NetworkManager.Singleton.LocalClientId);
			}
			else
			{
				if (recentlyDetectedProp.GetComponent<IPlaceable>() == null) return;
				recentlyDetectedProp.TryInteract(NetworkManager.Singleton.LocalClientId);
			}
		}	

		/// <summary>
		/// Controller가 Interact를 끊고싶을때 사용합니다.
		/// </summary>
		public void TryEndInteract()
		{
			if (currentOwningProp == null) return;

			if (GameManagerEx.Instance.IsDay)
			{
				currentOwningProp.OnEndInteraction(transform);
				currentOwningProp = null;
				SetAnimParam((int)AnimationType.Carry, false);
			}
			else
			{
				BuildingManager.Instance.TryPlaceBuilding(currentOwningProp);
				currentOwningProp.OnEndInteraction(transform);
				currentOwningProp = null;
			}
		}

		/// <summary>
		/// 들고있는 Prop의 Action을 수행합니다.
		/// ex. 타이어 -> 굴림, 소화기 -> 분사
		/// </summary>
		public void TryAction()
		{
			if (currentOwningProp == null) return;
			if (currentOwningProp.GetComponent<IActionable>() == null) return;

			switch (currentOwningProp)
			{
				case TireProp _:
					break;
				case Extinguisher ex:
					isAbleToMove = false;
					ex.GetComponent<IActionable>().OnStartPropAction(transform);
					SetAnimParam((int)AnimationType.Oil, true);
					break;
			}
		}

		public void TryEndAction()
		{
			if (currentOwningProp == null) return;
			if (currentOwningProp.GetComponent<IActionable>() == null) return;

			switch (currentOwningProp)
			{
				case TireProp _:
					SetAnimParam((int)AnimationType.Carry, false);
					SetAnimParam((int)AnimationType.Place);
					break;
				case Extinguisher ex:
					isAbleToMove = true;
					ex.GetComponent<IActionable>().OnStopPropAction(transform);
					SetAnimParam((int)AnimationType.Oil, false);
					break;
			}
		}

		/// <summary>
		/// 수리를 시작할 때 호출
		/// 
		/// </summary>
		public void TryStartFix()
		{
			if (currentFixablePart == null) return;

			if (currentFixablePart.IsAbleToInteract(currentOwningProp))
			{
				if(currentOwningProp is TireProp)
				{
					SetAnimParam((int)AnimationType.Carry, false);
					SetAnimParam((int)AnimationType.Tire);
				}
				else
				{
					switch (currentFixablePart.PartType)
					{
						case CarParts.FLT:
						case CarParts.RLT:
						case CarParts.FRT:
						case CarParts.RRT:
							SetAnimParam((int)AnimationType.Crouch, true);
							break;
						case CarParts.Oil:
							SetAnimParam((int)AnimationType.Oil, true);
							break;
						case CarParts.Engine:
							SetAnimParam((int)AnimationType.Hammer, true);
							break;
					}
					stateMachine.ChangeState(interactState);
				}
			}
		}

		/// <summary>
		/// TryInteract 후에 상호작용 가능한 경우에만 Prop쪽에서 호출됩니다.
		/// </summary>
		public void OnInteractionGranted(OwnableProp prop)
		{
			currentOwningProp = prop;

			if (currentOwningProp.GetComponent<IPlaceable>() == null)
			{
				stateMachine.ChangeState(carryState);
			}
			else
			{
				stateMachine.ChangeState(carryState);
			}
		}

		private Vector3 moveDir = Vector3.zero;
		/// <summary>
		/// move 방향으로 speed의 속도로 움직입니다.
		/// maxSpeed 로는 Animation의 BlendTree 값을 조절합니다.
		/// </summary>
		public void MovePosition(Vector2 move, float speed, float maxSpeed)
		{
			if (!isAbleToMove)
			{
				rigid.linearVelocity = Vector3.zero;
				SetAnimParam((int)AnimationType.Speed, 0);
				return;
			}

			if (isBeingForced) return;

			if(Mathf.Approximately(Mathf.Abs(move.x) + Mathf.Abs(move.y), 0f)) 
				speed = 0;

			moveDir = new Vector3(move.y, 0f, -move.x).normalized;
			moveDir *= speed;
			rigid.linearVelocity = moveDir;

			if (moveDir.sqrMagnitude > .1f)
				rigid.MoveRotation(Quaternion.LookRotation(moveDir));

			SetAnimParam((int)AnimationType.Speed, speed / maxSpeed);
		}

		private float fixablePartDistance = 100f;
		/// <summary>
		/// Player의 forward 근처의 물체를 탐지합니다.
		/// </summary>
		public void DetectInteractableParts()
		{
			fixablePartDistance = 1000f;

			Vector3 boxSize = new Vector3(boxWidth, boxHeight, boxWidth);
			Vector3 boxCenter = transform.position + transform.forward * (boxSize.z / 2f + 0.5f) + new Vector3(0f, boxSize.y/2, 0f);

			int targetLayer = Constants.LAYER_INTERACTABLE;
			int hitCount = Physics.OverlapBoxNonAlloc(boxCenter, boxSize * 0.5f, interactableHits, transform.rotation, targetLayer);

			recentlyDetectedProp = null;
			currentFixablePart = null;
			currentKickableCar = null;

			for (int i = 0; i < hitCount; i++)
			{
				if (currentOwningProp != null && interactableHits[i].GetComponent<OwnableProp>() == currentOwningProp) 
					continue;

				if (recentlyDetectedProp == null && interactableHits[i].GetComponent<OwnableProp>() != null)
					recentlyDetectedProp = interactableHits[i].GetComponent<OwnableProp>();

				// CarParts탐지
				if (interactableHits[i].GetComponent<CarPartBase>() != null
					&& interactableHits[i].GetComponent<CarPartBase>().IsAbleToInteract(currentOwningProp))
				{
					if (transform.position.ManhatanDistance(interactableHits[i].transform.position) < fixablePartDistance)
					{
						fixablePartDistance = transform.position.ManhatanDistance(interactableHits[i].transform.position);
						currentFixablePart = interactableHits[i].GetComponent<CarPartBase>();
                    }
				}

				// 찰 수 있는 차량 탐지
                if (interactableHits[i].GetComponent<CarSideDoor>() != null)
				{
					currentKickableCar = interactableHits[i].GetComponent<CarSideDoor>().Car;
                }
            }


			if (currentFixablePart == null || currentFixablePart != preEnlargedFixablePart)
			{
                UIManager.Game.TryToReducePreCarPartUI(preEnlargedFixablePart);
            }
			if (currentFixablePart != null)
            {
                UIManager.Game.TryToEnlargeCurCarPartUI(currentFixablePart);
				preEnlargedFixablePart = currentFixablePart;
            }
			


			UIManager.Game.PopupItemInfo(recentlyDetectedProp == null ? null : recentlyDetectedProp.ItemData);
			Debugger.DebugDrawBox(boxCenter, boxSize, transform.rotation, Color.green);
		}


        public void ExtinguishFire(Vector3 position)
		{
			Vector3 sprayEndPosition = position + transform.forward * fireExLength;

			int counts = Physics.OverlapCapsuleNonAlloc(position, sprayEndPosition, fireExRadius, interactableHits, fireExLayer);

			for (int i = 0; i<counts; i++)
			{
				CarPartBase part = interactableHits[i].GetComponent<CarPartBase>();
				if (part == null) continue;

				part.Interact(this, currentOwningProp);
			}
		}

		public Transform GetSocket(PropType type) 
		{
			return sockets[(int)type];
		}

		// 키 바인딩 필요
        public void KickCar()
		{
            if (currentKickableCar == null) return;
			if (currentKickableCar.CarStatus.IsThereAnyBroken()) return;

			// 차는 애니메이션 실행
			isBeingForced = true;
            SetAnimParam((int)AnimationType.Kick);
        }

		[SerializeField] private float knockbackStrength = 5f;
        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer != Constants.INT_VEHICLE) return;

            Rigidbody carRigid = collision.collider.GetComponentInParent<Rigidbody>();
			if (carRigid == null)
			{
				Debug.LogWarning("Car Rigidbody can't be found");
                return;
            }
			CarController car = carRigid.GetComponent<CarController>();

			// 힘 안받고있으면 return
			if (!car.IsBeingForced) return;

            // 튕겨나갈 방향 계산
            Vector3 knockbackDirection = Vector3.zero;
            if (collision.contactCount > 0)
            {
                knockbackDirection = collision.contacts[0].normal; // 충돌지점에서 플레이어쪽 방향
                knockbackDirection = knockbackDirection.normalized;

                // 아니면 차량에서 플레이어 방향으로
                //knockbackDirection = (transform.position - collision.transform.position).normalized;
            }

			Vector3 targetRotVector3 = -knockbackDirection;
			targetRotVector3.y = 0;
            Quaternion targetRot = Quaternion.LookRotation(targetRotVector3);
			rigid.rotation = targetRot;
			isBeingForced = true;
            rigid.AddForce(knockbackDirection * knockbackStrength, ForceMode.Impulse);

			// 애니메이션 실행
			SetAnimParam((int)AnimationType.KnockBack);
        }

		[Button]
		private void IsBeingForcedFalse()
		{
			isBeingForced = false;
		}

        [Button]
        private void AddForceToPlayer()
        {
            isBeingForced = true;
            rigid.AddForce(new Vector3(1,0,0)* knockbackStrength, ForceMode.Impulse);
        }

        #region Animation Events

        private void OnStartPlace()
		{
			if (!IsOwner) return;

			isAbleToMove = false;
			rigid.linearVelocity = Vector3.zero;
		}
		private void OnEndPlace()
		{
			if (!IsOwner) return;

			isAbleToMove = true;
			if (currentOwningProp == null) return;
			
			if (currentOwningProp.GetComponent<IActionable>() != null)
			{
				currentOwningProp.GetComponent<IActionable>().OnStopPropAction(transform);
			}
			currentOwningProp = null;
		}

		private void OnPutTire()
		{
			if (!IsOwner) return;
			if (currentOwningProp == null) return;

			Managers.Sound.PlaySfx(SFXType.Put, 1.3f, 1f);

			currentFixablePart?.Interact(this, currentOwningProp);
			DespawnPropServerRPC(currentOwningProp.NetworkObjectId);
			currentOwningProp = null;
			isAbleToMove = true;
		}

		[ServerRpc(RequireOwnership = false)]
		private void DespawnPropServerRPC(ulong networkId)
		{
			if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkId, out var networkObject))
			{
				if (networkObject.IsSpawned)
				{
					networkObject.Despawn(true);
				}
			}
		}

		private void OnFootstep()
		{
			// TODO - 바닥 텍스쳐에 따라 소리 다르게 하면 좋을듯?
			// 지금은 자갈 밟는 소리임
			Managers.Sound.PlaySfx(SFXType.Walk, .7f, 1f);
		}
		private void OnCrouch()
		{
			Managers.Sound.PlaySfx(SFXType.Wrench, .5f, 1.1f);
		}

		private void OnHammer()
		{
			Managers.Sound.PlaySfx(SFXType.Hammer, .8f, 1.2f);

            //Vector3 VFXpos = currentFixablePart.transform.position;
            Vector3 VFXpos = currentOwningProp.transform.position;
			//VFXManager.Instance.PlayVFX(VFXType.RepairHammering, VFXpos);
		}

		private void OnOiling()
		{
			if(currentOwningProp is OilPump)
			{
				Managers.Sound.PlaySfx(SFXType.Glug, .9f, Random.Range(.85f, 1.15f));
			}
		}

		private void OnKick()
		{
            if (currentKickableCar == null) return;

            Vector3 fromMeToCar = currentKickableCar.transform.position - transform.position;

            if (fromMeToCar.x < 0) // <-
            {
                currentKickableCar.ApplyKickServerRPC(KickDirection.Right);
                transform.rotation = Quaternion.Euler(new Vector3(0f, -90f, 0f));
            }
            else if (fromMeToCar.x > 0) // ->
            {
                currentKickableCar.ApplyKickServerRPC(KickDirection.Left);
                transform.rotation = Quaternion.Euler(new Vector3(0f, 90f, 0f));
            }
		}

		private void OnKickEnd()
		{
			isBeingForced = false;

        }

		private void OnGettingUp()
		{
			isBeingForced = false;
		}

		#endregion

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
                Debug.Log("똑같으므로 리턴");
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
	}
}