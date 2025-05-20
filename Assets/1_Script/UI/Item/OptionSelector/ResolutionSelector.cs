using Garage.Manager;
using UnityEngine;

namespace Garage.UI.Item
{
	public class ResolutionSelector : OptionSelector
	{
		private Vector2Int[] resolutions = {
			new Vector2Int(2560, 1440),
			new Vector2Int(1920, 1080),
			new Vector2Int(1280, 720)
		};

		public override void ApplySetting()
		{
			Screen.SetResolution(Managers.Data.BasicSettingData.resolutionWidth, Managers.Data.BasicSettingData.resolutionHeight,
				Managers.Data.BasicSettingData.isFullScreen);
		}

		public override void SetUIAsCurrentSetting()
		{
			for (int i = 0; i < resolutions.Length; i++)
			{
				if (Managers.Data.BasicSettingData.resolutionWidth == resolutions[i].x &&
					Managers.Data.BasicSettingData.resolutionHeight == resolutions[i].y)
				{
					currentIndex = i;
					UpdateLabel();
					ApplySetting();
					return;
				}
			}

			// DEFAULT
			currentIndex = 1;
			UpdateLabel();
			ApplySetting();
		}

		protected override void OnLeftButton()
		{
			currentIndex = (currentIndex - 1 + resolutions.Length) % resolutions.Length;
			UpdateLabel();
		}

		protected override void OnRightButton()
		{
			currentIndex = (currentIndex + 1) % resolutions.Length;
			UpdateLabel();
		}

		protected override void UpdateLabel()
		{
			optionLabel.text = resolutions[currentIndex].x + " x " + resolutions[currentIndex].y;

			Managers.Data.BasicSettingData.resolutionWidth = resolutions[currentIndex].x;
			Managers.Data.BasicSettingData.resolutionHeight = resolutions[currentIndex].y;
		}
	}
}