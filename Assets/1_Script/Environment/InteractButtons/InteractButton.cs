using Garage.Utils;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Garage.Environment
{
    public class InteractButton : NetworkBehaviour
	{
		[SerializeField] protected Vector3 boxCenter;
		[SerializeField] protected Vector3 boxSize;
		[SerializeField] protected float maxTime = 5f;

		[Header("Progress Bar")]
		[SerializeField] protected TextMeshProUGUI stageIdxTmp;
		[SerializeField] protected Image progress;

		protected int playerCount;
		protected NetworkVariable<float> elapsedTime = new();
		protected Collider[] hits = new Collider[4];
		protected bool isSomeoneDetected = false;

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			Gizmos.matrix = Matrix4x4.TRS(transform.position + boxCenter, Quaternion.identity, Vector3.one);
			Gizmos.DrawWireCube(Vector3.zero, boxSize);
			Gizmos.color = new Color(0f, 1f, 0f, 0.1f);
			Gizmos.DrawCube(Vector3.zero, boxSize);
		}

		private void OnEnable()
		{
			progress.fillAmount = 0f;
		}
	}
}