using Garage.Controller.StateMachine;
using Garage.Interfaces;
using Garage.Manager;
using Garage.Props;
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

		[TabGroup("Main", "Movements")]
		[SerializeField] private List<Transform> sockets = new();
		[SerializeField] private Transform cameraTransform;

		[FoldoutGroup("Player Speeds")]
		[SerializeField] private float walkSpeed;
		[SerializeField] private float runSpeed;
		[SerializeField] private float carrySpeed;

		[FoldoutGroup("Ray Settings")]
		[SerializeField] private float interactRayLength;

		[TabGroup("Main", "Rendering")]
		[SerializeField] private SkinnedMeshRenderer meshRenderer;
		[SerializeField] private List<Material> playerMaterial = new();


		private int[] animIDs = new int[3];

		
		private bool isAbleToMove = true;
		public float WalkSpeed => walkSpeed;
		public float RunSpeed => runSpeed;
		public float CarrySpeed => carrySpeed;

		private bool isDetectInteractable = false;
		private OwnableProp recentlyDetectedProp = null;
		private OwnableProp currentOwningProp = null;

		public OwnableProp CurrentOwningProp => currentOwningProp;
		public OwnableProp RecentlyDetectedProp => recentlyDetectedProp;


		/** Player State Machine **/
		private PlayerStateMachine stateMachine;
		public PlayerStateMachine StateMachine { get => stateMachine; }

		public IdleState idleState;
		public CarryState carryState;
		public InteractState interactState;


		private void Awake()
		{
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
			if (!isDetectInteractable) return;

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
				if (!currentOwningProp.IsCarry)
				{
					currentOwningProp.OnEndInteraction(transform);
					currentOwningProp = null;
				}
				else
				{
					SetAnimParam((int)AnimationType.Carry, false);
				}
			}
			else
			{
				BuildingManager.Instance.TryPlaceBuilding(currentOwningProp);
				currentOwningProp.OnEndInteraction(transform);
				currentOwningProp = null;
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

		/// <summary>
		/// Player의 forward 근처의 물체를 탐지합니다.
		/// </summary>
		public void DrawRay()
		{
			RaycastHit hit;
			int targetLayer = Constants.LAYER_INTERACTABLE;

			// HACK - Raycast로 못찾는게 많을듯. overlap으로 변경 필요
			if (UnityEngine.Physics.Raycast(transform.position + new Vector3(0f, .1f, 0f), transform.forward, out hit, interactRayLength, targetLayer))
			{
				isDetectInteractable = true;
				recentlyDetectedProp = hit.transform.GetComponent<OwnableProp>();
			}
			else
			{
				isDetectInteractable = false;
				recentlyDetectedProp = null;
			}

			Debug.DrawRay(transform.position + new Vector3(0f, .1f, 0f), transform.forward * interactRayLength, Color.red);
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

			currentOwningProp.OnEndInteraction(transform);
			currentOwningProp = null;
			isAbleToMove = true;
		}
		#endregion
	}
}