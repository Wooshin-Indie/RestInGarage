using Garage.Manager;
using Garage.Utils;
using IUtil;
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
		[SerializeField] private Image meetingProgress;

		[SerializeField, ReadOnly]
		private float elapsedTime = 0f;

		[SerializeField, ReadOnly]
		private bool isMeeting = false;


		private Collider[] hits = new Collider[4];

		private void Start()
		{
			if (!GetComponent<NetworkObject>().IsSpawned)
			{
				GetComponent<NetworkObject>().Spawn();
			}
		}

		public void StartMeet()
		{
			elapsedTime = 0f;
			gameObject.SetActive(true);
		}

		public void EndMeet()
		{
			gameObject.SetActive(false);
		}

		private void Update()
		{
			if (!IsHost) return;

			int playerCount  = Physics.OverlapBoxNonAlloc(transform.position + boxCenter, boxSize * 0.5f, hits, Quaternion.identity, Constants.LAYER_PLAYER);
			isMeeting = (playerCount == NetworkManager.Singleton.ConnectedClients.Count);

			if (isMeeting)
			{
				SunManager.Instance.SetTimePhase(TimePhase.Morning, maxTime);
				elapsedTime += Time.deltaTime;
				if(elapsedTime > maxTime)
				{
					GameManagerEx.Instance.StartNextStage();
				}
			}
			else
			{
				SunManager.Instance.SetTimePhase(TimePhase.Night, maxTime);
				if (elapsedTime > 0f) elapsedTime -= Time.deltaTime;
			}

			meetingProgress.fillAmount = elapsedTime / maxTime;
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
