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
		[SerializeField] private Transform cameraTransform;

		[FoldoutGroup("Player Speeds")]
		[SerializeField] private float walkSpeed;
		[SerializeField] private float runSpeed;
		[SerializeField] private float carrySpeed;

		[FoldoutGroup("Ray Settings")]
		[SerializeField] private float boxWidth;
		[SerializeField] private float boxHeight;

		[TabGroup("Main", "Rendering")]
		[SerializeField] private SkinnedMeshRenderer meshRenderer;
		[SerializeField] private List<Material> playerMaterial = new();


		private int[] animIDs = new int[7];

		
		private bool isAbleToMove = true;
		public float WalkSpeed => walkSpeed;
		public float RunSpeed => runSpeed;
		public float CarrySpeed => carrySpeed;

		private Collider[] interactableHits = null;
		
		private OwnableProp recentlyDetectedProp = null;
		private OwnableProp currentOwningProp = null;
		private CarPartBase currentFixablePart = null;
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


		private void Awake()
		{
			interactableHits = new Collider[5];

			animator = GetComponent<Animator>();
			rigid = GetComponent<Rigidbody>();
			capsule = GetComponent<CapsuleCollider>();

			stateMachine = new PlayerStateMachine();
			idleState = new IdleState(this, stateMachine);
			carryState = new CarryState(this, stateMachine);
			interactState = new InteractState(this, stateMachine);
			stateMachine.Init(idleState);

            rigid.maxLinearVelocity = runSpeed;

			animIDs[0] = Animator.StringToHash(Constants.ANIM_PARAM_CARRY);
			animIDs[1] = Animator.StringToHash(Constants.ANIM_PARAM_SPEED);
			animIDs[2] = Animator.StringToHash(Constants.ANIM_PARAM_OIL);
			animIDs[3] = Animator.StringToHash(Constants.ANIM_PARAM_PLACE);
			animIDs[4] = Animator.StringToHash(Constants.ANIM_PARAM_TIREPUT);
			animIDs[5] = Animator.StringToHash(Constants.ANIM_PARAM_HAMMER);
			animIDs[6] = Animator.StringToHash(Constants.ANIM_PARAM_CROUCH);
		}

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			cameraTransform.gameObject.SetActive(IsOwner);
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

			if(Mathf.Approximately(Mathf.Abs(move.x) + Mathf.Abs(move.y), 0f)) 
				speed = 0;

			moveDir = new Vector3(move.x, 0f, move.y).normalized;
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
		public void DetectInteractables()
		{
			fixablePartDistance = 1000f;

			Vector3 boxSize = new Vector3(boxWidth, boxHeight, boxWidth);
			Vector3 boxCenter = transform.position + transform.forward * (boxSize.z / 2f + 0.5f) + new Vector3(0f, boxSize.y/2, 0f);

			int targetLayer = Constants.LAYER_INTERACTABLE;
			int hitCount = Physics.OverlapBoxNonAlloc(boxCenter, boxSize * 0.5f, interactableHits, transform.rotation, targetLayer);

			recentlyDetectedProp = null;
			currentFixablePart = null;

			for (int i = 0; i < hitCount; i++)
			{
				if (currentOwningProp != null && interactableHits[i].GetComponent<OwnableProp>() == currentOwningProp) 
					continue;

				if (recentlyDetectedProp == null && interactableHits[i].GetComponent<OwnableProp>() != null)
					recentlyDetectedProp = interactableHits[i].GetComponent<OwnableProp>();

				if (interactableHits[i].GetComponent<CarPartBase>() != null
					&& interactableHits[i].GetComponent<CarPartBase>().IsAbleToInteract(currentOwningProp))
				{
					if (transform.position.ManhatanDistance(interactableHits[i].transform.position) < fixablePartDistance)
					{
						fixablePartDistance = transform.position.ManhatanDistance(interactableHits[i].transform.position);
						currentFixablePart = interactableHits[i].GetComponent<CarPartBase>();
                    }
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
        public Transform GetSocket(PropType type) 
		{
			return sockets[(int)type];
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

			if (currentOwningProp.GetComponent<IActionable>() != null)
			{
				currentOwningProp.GetComponent<IActionable>().OnStopPropAction(transform);
			}
			currentOwningProp = null;
			isAbleToMove = true;
		}

		private void OnPutTire()
		{
			if (!IsOwner) return;

			SoundManager.Instance.PlaySfx(SFXType.Put, 1.3f, 1f);

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
			SoundManager.Instance.PlaySfx(SFXType.Walk, .7f, 1f);
		}
		private void OnCrouch()
		{
			SoundManager.Instance.PlaySfx(SFXType.Wrench, .5f, 1.1f);
		}

		private void OnHammer()
		{
			SoundManager.Instance.PlaySfx(SFXType.Hammer, .8f, 1.2f);

			Vector3 VFXpos = currentOwningProp.transform.position;
			// VFXManager.Instance.PlayVFX(VFXType.RepairHammering, VFXpos);
		}

		private void OnOiling()
		{
			if(currentOwningProp is OilPump)
			{
				SoundManager.Instance.PlaySfx(SFXType.Glug, .9f, Random.Range(.85f, 1.15f));
			}
		}

		#endregion
	}
}