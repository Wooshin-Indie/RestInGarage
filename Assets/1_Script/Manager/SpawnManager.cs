using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Manager
{
	public class SpawnManager
	{
		private List<GameObject> propList = new();

		public void Init()
		{

		}

		public GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
		{
			GameObject go = Object.Instantiate(prefab, position, rotation, parent);
			return go;
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
			propList.Add(go);
			return go;
		}

		public void OnStageEnd()
		{
			for(int i=0; i<propList.Count; i++)
			{
				if (propList[i] != null)
				{
					Object.Destroy(propList[i]);
				}
			}
			propList.Clear();
		}
	}
}