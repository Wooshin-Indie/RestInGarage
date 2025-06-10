using Garage.Manager;
using Garage.Utils;
using IUtil;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Garage.Environment
{
	public class MeetingPoint : NetworkBehaviour
	{
		[SerializeField] private Vector3 boxCenter;
		[SerializeField] private Vector3 boxSize;
		[SerializeField] private float maxTime = 5f;

		[Header("Progress Bar")]
		[SerializeField] private TextMeshProUGUI stageIdxTmp; 
		[SerializeField] private Image meetingProgress;
		[SerializeField] private Image floorImage;

		private NetworkVariable<float> elapsedTime = new();

		[SerializeField, ReadOnly]
		private bool isMeeting = false;


		private Collider[] hits = new Collider[4];

		private void Awake()
		{

		}

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			GameManagerEx.Instance.OnBeforeStageStartAction += (() =>
			{
				EndMeetClientRPC();
			});
			GameManagerEx.Instance.OnAfterStageEndAction += ((curStage) =>
			{
				StartMeetClientRPC(curStage + 1);
			});
		}

		[ClientRpc]
		public void StartMeetClientRPC(int idx)
		{
			if (IsHost)
				elapsedTime.Value = 0f;

			gameObject.SetActive(true);
			stageIdxTmp.text = $"Stage {idx.ToString()}";
		}

		[ClientRpc]
		public void EndMeetClientRPC()
		{
			gameObject.SetActive(false);
		}

		private int playerCount;
		private bool isLocalPlayerDetected = false;
		private void Update()
		{
			meetingProgress.fillAmount = elapsedTime.Value / maxTime;
			playerCount  = Physics.OverlapBoxNonAlloc(transform.position + boxCenter, boxSize * 0.5f, hits, Quaternion.identity, Constants.LAYER_PLAYER);
			isLocalPlayerDetected = false;
			for (int i = 0; i < playerCount; i++)
			{
				var networkBehaviour = hits[i].GetComponent<NetworkBehaviour>();
				if (networkBehaviour != null && networkBehaviour.IsLocalPlayer)
				{
					isLocalPlayerDetected = true;
					break;
				}
			}

			if (isLocalPlayerDetected)
			{
				floorImage.color = Color.white;
			}
			else
			{
				floorImage.color = Color.black;
			}

			if (!IsHost) return;


			isMeeting = (playerCount == NetworkManager.Singleton.ConnectedClients.Count);
			if (isMeeting)
			{
				SunManager.Instance.SetTimePhase(TimePhase.Morning, maxTime);
				elapsedTime.Value+= Time.deltaTime;
				if(elapsedTime.Value > maxTime)
				{
					GameManagerEx.Instance.StartNextStage();
				}
			}
			else
			{
				SunManager.Instance.SetTimePhase(TimePhase.Night, maxTime);
				if (elapsedTime.Value > 0f) elapsedTime.Value -= Time.deltaTime;
			}
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			Gizmos.matrix = Matrix4x4.TRS(transform.position + boxCenter, Quaternion.identity, Vector3.one);
			Gizmos.DrawWireCube(Vector3.zero, boxSize);
			Gizmos.color = new Color(0f, 1f, 0f, 0.1f);
			Gizmos.DrawCube(Vector3.zero, boxSize);
		}
	}
}
