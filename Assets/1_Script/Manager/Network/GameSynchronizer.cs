using Garage.Structs;
using Garage.Utils;
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
		public NetworkVariable<int> MapIdx = new();

		private void Start()
		{
			GameManagerEx.Instance.OnBeforeStageStartAction += (() =>
			{
				IsDay.Value = true;
				CurrentStage.Value++;
				OnStageStartClientRPC(GameSynchronizer.Instance.CurrentStage.Value);
			});

			GameManagerEx.Instance.OnBeforeStageEndAction += (() =>
			{
				IsDay.Value = false;
			});
		}

		public void SetGameTimer(float time)
		{
			RemainedTime.Value += time;
		}

		public override void OnNetworkSpawn()
		{
			RemainedTime.OnValueChanged -= UIManager.Game.OnTimerChanged;
			RemainedTime.OnValueChanged += UIManager.Game.OnTimerChanged;
			RemainedTime.OnValueChanged -= OnRemainedTimeChanged;
			RemainedTime.OnValueChanged += OnRemainedTimeChanged;
            Debug.Log("OnNetworkSpawn");
        }

		[ClientRpc]
		public void OnStageStartClientRPC(int idx)
		{
			nextLogTime = float.MaxValue;
			UIManager.Game.OnStartStage(idx);
		}


		private float nextLogTime = float.MaxValue;
		private void OnRemainedTimeChanged(float previous, float current)
		{
			if (!IsHost) return;

			if (current <= nextLogTime)
			{
				TrafficManager.Instance.SpawnCar();
				SetNextSpawnTime(current);
			}
		}
		private void SetNextSpawnTime(float currentTime)
		{
			float interval = Managers.Resource.GetData<MapData>(0).SpawnInterval[CurrentStage.Value].GetRandomValue();
			nextLogTime = currentTime - interval;
		}
	}
}