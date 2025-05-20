using System.IO;
using System;
using UnityEngine;

namespace Garage.Manager
{
	[Serializable]
	public class SettingData
	{
		public SettingData()
		{
			sfxVolume = 1f;
			bgmVolume = 1f;
			masterVolume = 1f;
			ambientVolume = 1f;
			languageIndex = 0;
			resolutionWidth = 1920;
			resolutionHeight = 1080;
			isFullScreen = true;
		}


		public float masterVolume;
		public float ambientVolume;
		public float sfxVolume;
		public float bgmVolume;
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
			languageIndex = 0;
			resolutionWidth = 1920;
			resolutionHeight = 1080;
			isFullScreen = true;
		}
	}
	public class DataManager
	{
		private string settingDataPath;

		public SettingData BasicSettingData { get { return settingData; } }

		private SettingData settingData;

		public void Init()
		{
			settingDataPath = Application.persistentDataPath + "/SettingData.json";

			LoadAll();
			ApplyBasicSettings();
		}
		private void LoadAll()
		{
			LoadData<SettingData>(ref settingData, settingDataPath);
		}

		public void SaveAll()
		{
			SaveData<SettingData>(ref settingData, settingDataPath);
		}


		private void SaveData<T>(ref T data, string path)
		{
			string json = JsonUtility.ToJson(data, true);
			File.WriteAllText(path, json);
		}
		private void LoadData<T>(ref T data, string path) where T : new()
		{
			if (File.Exists(path))
			{
				string json = File.ReadAllText(path);
				data = JsonUtility.FromJson<T>(json);
			}
			else
			{
				data = new T();
			}
		}

		private void ApplyBasicSettings()
		{
			Managers.Sound.BgmVolume = settingData.bgmVolume;
			Managers.Sound.SfxVolume = settingData.sfxVolume;
			Managers.Sound.MasterVolume = settingData.masterVolume;

			// LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[settingData.languageIndex];

			Screen.SetResolution(settingData.resolutionWidth, settingData.resolutionHeight, settingData.isFullScreen);
		}
	}
}
