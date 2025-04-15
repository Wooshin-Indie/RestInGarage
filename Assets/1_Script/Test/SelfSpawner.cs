using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class SelfSpawner : MonoBehaviour
{
	private void Awake()
	{
		GetComponent<NetworkObject>().Spawn();
	}
}
