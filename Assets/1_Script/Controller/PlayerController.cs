using Garage.Props;
using Garage.Utils;
using IUtil;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using Unity.Netcode;
using Unity.Netcode.Components;
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
		[FoldoutGroup("Player Speeds")]
		[SerializeField] private float walkSpeed;
		[SerializeField] private float runSpeed;
		[SerializeField] private float carrySpeed;

		[SerializeField] private float interactRayLength;

		[SerializeField] private Transform cameraTransform;

		[TabGroup("Main", "Rendering")]
		[SerializeField] private SkinnedMeshRenderer meshRenderer;
		[SerializeField] private List<Material> playerMaterial = new();

		[SerializeField] private List<Transform> sockets = new();

		private int[] animIDs = new int[2];

		private void Awake()
		{
			animator = GetComponent<Animator>();
			rigid = GetComponent<Rigidbody>();
			capsule = GetComponent<CapsuleCollider>();

			rigid.maxLinearVelocity = runSpeed;

			animIDs[0] = Animator.StringToHash(Constants.ANIM_PARAM_CARRY);
			animIDs[1] = Animator.StringToHash(Constants.ANIM_PARAM_SPEED);
		}

		public NetworkVariable<int> PlayerID = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		private void OnPlayerIDChanged(int prev, int playerId)
		{
			var materials = meshRenderer.sharedMaterials.ToList();

			materials.Clear();
			materials.Add(playerMaterial[playerId]);

			meshRenderer.materials = materials.ToArray();
		}

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			// TODO - Anim : basic anim
			cameraTransform.gameObject.SetActive(IsOwner);

			PlayerID.OnValueChanged += OnPlayerIDChanged;
		}

		private bool isCarrying = false;
		private Vector3 moveDir = Vector3.zero;
		private void Update()
		{
			if (!IsOwner) return;

			if (!isAbleToMove) return;

			DrawRay();
			float h = Input.GetAxisRaw("Horizontal");
			float v = Input.GetAxisRaw("Vertical");
			moveDir = new Vector3(h, 0f, v).normalized;

			if (Input.GetKeyDown(KeyCode.F))
			{
				if (currentOwningProp != null)
				{
					isCarrying = false;
				}
				else if (isDetectInteractable)
				{
					recentlyDetectedProp.TryInteract();
					isCarrying = true;
				}
				SetAnimParam((int)AnimationType.Carry, isCarrying);
			}

			bool isRunning = Input.GetKey(KeyCode.LeftShift);

			float speed = walkSpeed;
			if (isRunning) speed = runSpeed;
			if (isCarrying) speed = carrySpeed;

			moveDir *= speed;

			rigid.linearVelocity = moveDir;
			if (moveDir.sqrMagnitude > .1f)
				rigid.MoveRotation(Quaternion.LookRotation(moveDir));

			float speedParam = moveDir.magnitude / (isCarrying ? carrySpeed : runSpeed);
			SetAnimParam((int)AnimationType.Speed, speedParam);	

			OnUpdateSynchronization();
		}

		private void FixedUpdate()
		{
			if (!IsOwner) return;
		}

		private bool isDetectInteractable = false;
		private OwnableProp recentlyDetectedProp = null;
		private void DrawRay()
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

			Debug.DrawRay(transform.position, transform.forward * interactRayLength, Color.red);
		}

		private OwnableProp currentOwningProp = null;

		public void StartInteraction(OwnableProp prop)
		{
			currentOwningProp = prop;
		}
		public Transform GetSocket(PropType type) 
		{
			return sockets[(int)type];
		}

		private bool isAbleToMove = true;
		public void OnStartPlace()
		{
			if (!IsOwner) return;

			isAbleToMove = false;
			rigid.linearVelocity = Vector3.zero;
		}
		public void OnEndPlace()
		{
			if (!IsOwner) return;

			currentOwningProp.EndInteraction(transform);
			currentOwningProp = null;
			isAbleToMove = true;
		}
	}
}