using Garage.Manager;
using UnityEngine;

namespace Garage.UI.Item
{
	public class FullscreenSelector : OptionSelector
	{
		private bool[] isFullscreen = {
			true, false
		};

		public override void ApplySetting()
		{
			Screen.SetResolution(Managers.Data.BasicSettingData.resolutionWidth, Managers.Data.BasicSettingData.resolutionHeight, 
				Managers.Data.BasicSettingData.isFullScreen);
		}

		public override void SetUIAsCurrentSetting()
		{
			for (int i = 0; i < isFullscreen.Length; i++)
			{
				if (Managers.Data.BasicSettingData.isFullScreen == isFullscreen[i])
				{
					currentIndex = i;
					UpdateLabel();
					return;
				}
			}

			// DEFAULT
			currentIndex = 0;
			UpdateLabel();
			ApplySetting();
		}

		protected override void OnLeftButton()
		{
			currentIndex = (currentIndex - 1 + isFullscreen.Length) % isFullscreen.Length;
			UpdateLabel();
		}

		protected override void OnRightButton()
		{
			currentIndex = (currentIndex + 1) % isFullscreen.Length;
			UpdateLabel();
		}

		protected override void UpdateLabel()
		{
			optionLabel.text = isFullscreen[currentIndex] ? "Full Screen" : "Window Screen";

			Managers.Data.BasicSettingData.isFullScreen = isFullscreen[currentIndex];
		}
	}
}