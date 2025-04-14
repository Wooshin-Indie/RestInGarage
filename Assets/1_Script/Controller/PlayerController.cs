using Garage.Utils;
using IUtil;
using System.Collections.Generic;
using System.Linq;
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
		[FoldoutGroup("Player Speeds")]
		[SerializeField] private float walkSpeed;
		[SerializeField] private float runSpeed;
		[SerializeField] private float carrySpeed;

		[SerializeField] private Transform cameraTransform;

		[TabGroup("Main", "Rendering")]
		[SerializeField] private SkinnedMeshRenderer meshRenderer;
		[SerializeField] private List<Material> playerMaterial = new();

		[System.Serializable]
		enum AnimationType { Carry, Speed }
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

			float h = Input.GetAxisRaw("Horizontal");
			float v = Input.GetAxisRaw("Vertical");
			moveDir = new Vector3(h, 0f, v).normalized;

			if (Input.GetKeyDown(KeyCode.F))
			{
				isCarrying = !isCarrying;
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
	}
}