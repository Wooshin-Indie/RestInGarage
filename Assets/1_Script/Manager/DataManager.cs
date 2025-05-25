using System.IO;
using UnityEngine;
using Garage.Utils;
using UnityEngine.Localization.Settings;

namespace Garage.Manager
{
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

		public void ApplyBasicSettings()
		{
			Managers.Sound.MasterVolume = settingData.masterVolume;
			Managers.Sound.BgmVolume = settingData.bgmVolume;
			Managers.Sound.SfxVolume = settingData.sfxVolume;
			Managers.Sound.AmbientVolume = settingData.ambientVolume;
			LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[settingData.languageIndex];

			Screen.SetResolution(settingData.resolutionWidth, settingData.resolutionHeight, settingData.isFullScreen);
		}
	}
}
