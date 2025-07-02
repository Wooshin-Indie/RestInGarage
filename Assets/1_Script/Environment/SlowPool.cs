using Garage.Controller;
using Unity.Netcode;
using UnityEngine;


namespace Garage.Environment
{
    public class SlowPool : MonoBehaviour
	{
		private void OnTriggerEnter(Collider other)
		{
			if (other.GetComponent<PlayerController>() == null) return;
			if (!other.GetComponent<NetworkObject>().IsLocalPlayer) return;

			Debug.Log("Stat manager에 slow효과 추가");
		}
		
		private void OnTriggerExit(Collider other)
		{
			if (other.GetComponent<PlayerController>() == null) return;
			if (!other.GetComponent<NetworkObject>().IsLocalPlayer) return;

			Debug.Log("Stat manager에 slow효과 제거");
		}
	}
}