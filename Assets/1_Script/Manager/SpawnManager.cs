using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace Garage.Manager
{
	public class SpawnManager
	{

		private Dictionary<ulong, GameObject> propDict = new();		// stage마다 초기화할 Prop들

		public void Init()
		{

		}

		public void Start()
		{
			GameManagerEx.Instance.OnBeforeStageEndAction += OnStageEnd;
		}

		public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
		{
			GameObject go = Object.Instantiate(prefab, position, rotation, parent);
			if (go.GetComponent<NetworkObject>() == null)
			{
				Object.Destroy(go);
				Debug.LogError("SpawnManager - There is no NetworkObect on prefab");
				return null;
			}

			go.GetComponent<NetworkObject>().Spawn();
			propDict.Add(go.GetComponent<NetworkObject>().NetworkObjectId, go);
			return go;
		}

		public void Despawn(ulong netId)
		{
			if(propDict.TryGetValue(netId, out GameObject obj))
			{
				obj.GetComponent<NetworkObject>().Despawn(true);
				propDict.Remove(netId);
			}
		}

		public void OnStageEnd()
		{
			foreach (var entry in propDict)
			{
				if (entry.Value != null)
				{
					entry.Value.GetComponent<NetworkObject>().Despawn(true);
				}
			}
			propDict.Clear();
		}
	}
}