using System.Runtime.CompilerServices;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Manager
{
	/// <summary>
	/// 게임 진행 동기화
	/// 오직 호스트만 호출 (ClientRPC만)
	/// </summary>
	public class GameSynchronizer : NetworkBehaviour
	{
		#region Singleton
		public static GameSynchronizer Instance { get => instance; }
		private static GameSynchronizer instance = null;

		private void Awake()
		{
			if (instance == null)
			{
				instance = this;
				DontDestroyOnLoad(gameObject);
			}
			else
			{
				Destroy(gameObject);
				return;
			}
		}
		#endregion

		public NetworkVariable<bool> IsDay = new();
		public NetworkVariable<int> CurrentStage = new();
		public NetworkVariable<float> RemainedTime = new();

		public void SetGameTimer(float time)
		{
			RemainedTime.Value += time;
		}

		public override void OnNetworkSpawn()
		{
			RemainedTime.OnValueChanged -= UIManager.Game.OnTimerChanged;
			RemainedTime.OnValueChanged += UIManager.Game.OnTimerChanged;
		}


	}
}