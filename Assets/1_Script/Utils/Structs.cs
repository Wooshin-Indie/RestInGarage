using Garage.Manager;
using System;
using System.Collections.Generic;

namespace Garage.Utils
{

	[System.Serializable]
	public class SettingData
	{
		public SettingData()
		{
			sfxVolume = 1f;
			bgmVolume = 1f;
			masterVolume = 1f;
			ambientVolume = 1f;
			brightness = 1f;
			languageIndex = 0;
			resolutionWidth = 1920;
			resolutionHeight = 1080;
			isFullScreen = true;
		}


		public float masterVolume;
		public float ambientVolume;
		public float sfxVolume;
		public float bgmVolume;
		public float brightness;
		public int languageIndex;
		public int resolutionWidth;
		public int resolutionHeight;
		public bool isFullScreen;

		// etc...

		public void Clear()
		{
			sfxVolume = 1f;
			bgmVolume = 1f;
			masterVolume = 1f;
			ambientVolume = 1f;
			brightness = 1f;
			languageIndex = 0;
			resolutionWidth = 1920;
			resolutionHeight = 1080;
			isFullScreen = true;
		}
	}


	/// <summary>
	/// 한 사람의 게임 플레이데이터
	/// </summary>
	[System.Serializable]
	public class GameplayRecordData
	{
		private ulong netId;
		private Dictionary<RuntimeRecordType, float> records = new();

		public ulong NetId => netId;

		public GameplayRecordData(ulong netId)
		{
			this.netId = netId;

			foreach (RuntimeRecordType type in Enum.GetValues(typeof(RuntimeRecordType)))
			{
				records[type] = 0f;
			}
		}

		public void AddValue(RuntimeRecordType type, float value)
		{
			if (!records.ContainsKey(type))
				records[type] = 0f;

			records[type] += value;
		}

		public float GetValue(RuntimeRecordType type)
		{
			return records.TryGetValue(type, out var val) ? val : 0f;
		}

		public void Reset()
		{
			foreach (RuntimeRecordType type in Enum.GetValues(typeof(RuntimeRecordType)))
			{
				records[type] = 0f;
			}
		}
	}

	// HACK - 임시로 Result UI 테스트용
	[System.Serializable]
	public class GameResultData
	{
		private ulong netId;
		private string tmpText;

		public ulong NetId => netId;
		public string TmpText => tmpText;

		public GameResultData(ulong netId, string tmpText)
		{
			this.netId = netId;
			this.tmpText = tmpText;
		}
	}
}
