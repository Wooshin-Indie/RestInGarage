using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class SelfSpawner : NetworkBehaviour
{
	private void Awake()
	{
		if (!IsHost) return;
		Debug.Log("SPAWNED");
		GetComponent<NetworkObject>().Spawn();
	}
}
