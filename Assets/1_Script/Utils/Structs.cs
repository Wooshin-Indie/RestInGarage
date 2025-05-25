using System;

namespace Garage.Utils
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
}
