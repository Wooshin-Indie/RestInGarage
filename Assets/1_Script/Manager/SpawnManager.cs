using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace Garage.Manager
{
	public class SpawnManager
	{
		private List<GameObject> stageGameobjects = new();

		public void Init()
		{

		}

		public void Start()
		{
			GameManagerEx.Instance.OnBeforeStageEndAction += DespawnStageGameobjects;
		}

		private void DespawnStageGameobjects()
		{
			for (int i = 0; i < stageGameobjects.Count; i++)
			{
				DespawnObject(stageGameobjects[i].GetComponent<NetworkObject>().NetworkObjectId);
			}
			stageGameobjects.Clear();
		}

		public GameObject SpawnInCurrentStage(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
		{
			GameObject go = SpawnInCurrentScene(prefab, position, rotation, parent);
			stageGameobjects.Add(go);
			return go;
		}
		
		public GameObject SpawnInCurrentScene(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
		{
			GameObject go = Object.Instantiate(prefab, position, rotation, parent);
			Managers.Scene.MoveGameObjectToCurrentScene(go);

			if (go.GetComponent<NetworkObject>() == null)
				Debug.LogWarning("There is no NetworkObject on prefab");
			else if (!NetworkManager.Singleton.IsHost)
				Debug.LogWarning("Only host can Spawn Objects.");

			go.GetComponent<NetworkObject>()?.Spawn();
			return go;
		}

		public GameObject SpawnInCurrentScene(GameObject prefab, Transform parent = null)
		{
			return SpawnInCurrentScene(prefab, Vector3.zero, Quaternion.identity, parent);
		}


		public void DespawnObject(ulong networkId)
		{
			if (!NetworkManager.Singleton.IsHost)
				Debug.LogWarning("Only host can Despawn Objects.");

			if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkId, out var netObj))
				Debug.LogWarning($"Can't Find : NetworkObject ID {networkId}.");

			netObj.Despawn(true);
		}
	}
}