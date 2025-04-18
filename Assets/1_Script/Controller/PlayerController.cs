using Garage.Interfaces;
using Garage.Manager;
using Garage.Props;
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

		public NetworkVariable<int> PlayerID = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

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

		private Vector3 moveDir = Vector3.zero;
		private bool isAbleToMove = true;


		private bool isDetectInteractable = false;
		private OwnableProp recentlyDetectedProp = null;
		private OwnableProp currentOwningProp = null;

		private void Awake()
		{
			animator = GetComponent<Animator>();
			rigid = GetComponent<Rigidbody>();
			capsule = GetComponent<CapsuleCollider>();

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
			OnUpdateSynchronization();

			DrawRay();
			float h = Input.GetAxisRaw("Horizontal");
			float v = Input.GetAxisRaw("Vertical");
			moveDir = new Vector3(h, 0f, v).normalized;

			if (Input.GetKeyDown(KeyCode.F))
			{
				if (GameManagerEx.Instance.IsDay)
				{
					if (currentOwningProp != null)
					{
						if (currentOwningProp.IsCarry)
						{
							SetAnimParam((int)AnimationType.Carry, false);
						}
						else
						{
							currentOwningProp.EndInteraction(transform);
							currentOwningProp = null;
						}
					}
					else if (isDetectInteractable)
					{
						recentlyDetectedProp.TryInteract(NetworkManager.Singleton.LocalClientId);
					}
				}
				else
				{
					if (currentOwningProp != null)
					{
						GameManagerEx.Instance.GetComponent<BuildingManager>().PlaceIfPossible(currentOwningProp);
						currentOwningProp.EndInteraction(transform);
						currentOwningProp = null;
					}
					else if (isDetectInteractable)
					{
						if (recentlyDetectedProp.GetComponent<IPlaceable>() != null)
						{
							recentlyDetectedProp.TryInteract(NetworkManager.Singleton.LocalClientId);
						}
					}
				}
			}

			if (!GameManagerEx.Instance.IsDay && currentOwningProp != null && currentOwningProp.GetComponent<IPlaceable>() != null)
			{
				GameManagerEx.Instance.GetComponent<BuildingManager>().UpdatePreviewArea(currentOwningProp, transform);
			}

			if (Input.GetKeyDown(KeyCode.Space))
			{
				SetAnimParam((int)AnimationType.Oil, true);
				isAbleToMove = false;
			}
			else if (Input.GetKeyUp(KeyCode.Space))
			{
				SetAnimParam((int)AnimationType.Oil, false);
				isAbleToMove = true;
			}

			if (!isAbleToMove)
			{
				rigid.linearVelocity = Vector3.zero;
				return;
			}

			bool isRunning = Input.GetKey(KeyCode.LeftShift);

			float speed = walkSpeed;
			bool isCarrying = (currentOwningProp != null && currentOwningProp.IsCarry);

			if (isRunning) speed = runSpeed;
			if (isCarrying) speed = carrySpeed;

			moveDir *= speed;


			rigid.linearVelocity = moveDir;
			if (moveDir.sqrMagnitude > .1f)
				rigid.MoveRotation(Quaternion.LookRotation(moveDir));

			float speedParam = moveDir.magnitude / (isCarrying ? carrySpeed : runSpeed);
			SetAnimParam((int)AnimationType.Speed, speedParam);	
		}


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

			Debug.DrawRay(transform.position + new Vector3(0f, .1f, 0f), transform.forward * interactRayLength, Color.red);
		}

		/// <summary>
		/// 개별 PlayerID를 부여해서 Spawn시 머터리얼을 변경합니다.
		/// </summary>
		private void OnPlayerIDChanged(int prev, int playerId)
		{
			var materials = meshRenderer.sharedMaterials.ToList();

			materials.Clear();
			materials.Add(playerMaterial[playerId]);

			meshRenderer.materials = materials.ToArray();
		}

		/// <summary>
		/// TryInteract 후에 상호작용 가능한 경우에만 Prop쪽에서 호출됩니다.
		/// </summary>
		public void StartInteraction(OwnableProp prop)
		{
			currentOwningProp = prop;
			if (prop.IsCarry)
			{
				SetAnimParam((int)AnimationType.Carry, true);
			}
		}

		public Transform GetSocket(PropType type) 
		{
			return sockets[(int)type];
		}

		[ClientRpc]
		public void EndInteractionClientRPC()
		{
			if (!IsOwner) return;
			if(currentOwningProp != null)
			{
				currentOwningProp.EndInteraction(transform);
				currentOwningProp = null;
			}
			// TODO - Anim 초기화
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

			currentOwningProp.EndInteraction(transform);
			currentOwningProp = null;
			isAbleToMove = true;
		}
		#endregion
	}
}