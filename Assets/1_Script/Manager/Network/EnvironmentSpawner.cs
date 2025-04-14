using Garage.Controller;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Manager
{
	public class EnvironmentSpawner : NetworkBehaviour
	{
		#region Singleton
		private static EnvironmentSpawner instance;
		public static EnvironmentSpawner Instance { get => instance; }

		private void Awake()
		{
			if (instance != null)
			{
				Destroy(gameObject);
			}
			else
			{
				instance = this;
				DontDestroyOnLoad(gameObject);
			}
		}
		#endregion


	}
}