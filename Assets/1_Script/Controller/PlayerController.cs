using Garage.Utils;
using IUtil;
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

		[FoldoutGroup("Player Speeds")]
		[SerializeField] private float walkSpeed;
		[SerializeField] private float runSpeed;
		[SerializeField] private float carrySpeed;

		[SerializeField] private Transform cameraTranform;

		private int animSpeedID;
		private int animCarryID;

		private void Awake()
		{
			animator = GetComponent<Animator>();
			rigid = GetComponent<Rigidbody>();
			capsule = GetComponent<CapsuleCollider>();

			rigid.maxLinearVelocity = runSpeed;

			animCarryID = Animator.StringToHash(Constants.ANIM_PARAM_CARRY);
			animSpeedID = Animator.StringToHash(Constants.ANIM_PARAM_SPEED);
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
				SetAnimParam(animCarryID, isCarrying);
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
			SetAnimParam(animSpeedID, speedParam);	

			OnUpdateSynchronization();
		}

		private void FixedUpdate()
		{
			if (!IsOwner) return;

		}
	}
}