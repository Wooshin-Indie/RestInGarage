using Garage.Manager;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using IUtil;
using Garage.UI.Item;

namespace Garage.UI.MainScene
{
    public enum PageEnum
    {
        None = 0,
        Main, Multi, Host, Settings,    // == 4
        Audio, Video, Control, Language // == 8
    }

    public class MainSceneUI : MonoBehaviour
	{
		[SerializeField] private List<GameObject> pages = new List<GameObject>();

		[FoldoutGroup("Page1")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingButton;

		[FoldoutGroup("Page2")]
		[SerializeField] private Button multyPlayButton;
        [SerializeField] private Button singlePlayButton;
        [SerializeField] private Button Page2BackButton;

		[FoldoutGroup("Page3")]
		[SerializeField] private Button hostButton;
        [SerializeField] private Button guestButton;
        [SerializeField] private Button Page3BackButton;

        [FoldoutGroup("Page4 - Settings")]
        [SerializeField] private Button audioButton;
        [SerializeField] private Button videoButton;
        [SerializeField] private Button controlButton;
        [SerializeField] private Button languageButton;
        [SerializeField] private Button page4BackButton;

        [FoldoutGroup("Page5 - Audio")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider ambientSlider;
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Button page5BackButton;


		[FoldoutGroup("Page6 - Video")]
		[SerializeField] private Button resolutionButton;
		[SerializeField] private Button fullscreenButton;
		[SerializeField] private Button page6BackButton;



		private void Start()
		{
            /** Page 1 **/
            playButton.onClick.AddListener(() => { GoToPage(2); });
            settingButton.onClick.AddListener(() => { GoToPage(4); });

            /** Page 2 **/
            multyPlayButton.onClick.AddListener(() => { GoToPage(3); });
			Page2BackButton.onClick.AddListener(() => { GoToPage(1); });

			/** Page 3 **/
			hostButton.onClick.AddListener(() =>
			{
				UIManager.Transition.StartTransition(.5f);
                DOVirtual.DelayedCall(.5f, () =>
                {
                    GameNetworkManager.Instance.StartHost();
                });
				// TODO - MainUI 초기화 코드 필요
			});
            Page3BackButton.onClick.AddListener(() => { GoToPage(2); });

            /** Page 4 **/
            audioButton.onClick.AddListener(() => { GoToPage(5); });
			videoButton.onClick.AddListener(() => { GoToPage(6); });
			controlButton.onClick.AddListener(() => { GoToPage(7); });
			languageButton.onClick.AddListener(() => { GoToPage(8); });
            page4BackButton.onClick.AddListener(() => { GoToPage(1); });

            AddListenersToSetting();
		}

        private void AddListenersToSetting()
		{
			/** Page 5 **/
			masterSlider.onValueChanged.AddListener((value) =>
			{
				Managers.Data.BasicSettingData.masterVolume = value;
				Managers.Sound.MasterVolume = value;
			});
			ambientSlider.onValueChanged.AddListener((value) =>
			{
				Managers.Data.BasicSettingData.ambientVolume = value;
				Managers.Sound.AmbientVolume = value;
			});
			bgmSlider.onValueChanged.AddListener((value) =>
			{
				Managers.Data.BasicSettingData.bgmVolume = value;
				Managers.Sound.BgmVolume = value;
			});
			sfxSlider.onValueChanged.AddListener((value) =>
			{
				Managers.Data.BasicSettingData.sfxVolume = value;
				Managers.Sound.SfxVolume = value;
				// TODO - SFX소리 나게 하면 좋을듯
			});
			page5BackButton.onClick.AddListener(() => { GoToPage(4); });

			/** Page 6 **/

			resolutionButton.onClick.AddListener(() => { resolutionButton.GetComponent<OptionSelector>().ApplySetting(); });
			fullscreenButton.onClick.AddListener(() => { fullscreenButton.GetComponent<OptionSelector>().ApplySetting(); });
			page6BackButton.onClick.AddListener(() => { GoToPage(4); });
		}

        private void UpdateSettings(int idx)
        {
            switch ((PageEnum)idx)
            {
				case PageEnum.Settings:
					Managers.Data.SaveAll();
					break;
                case PageEnum.Audio:
					masterSlider.value = Managers.Data.BasicSettingData.masterVolume;
					ambientSlider.value = Managers.Data.BasicSettingData.ambientVolume;
					bgmSlider.value = Managers.Data.BasicSettingData.bgmVolume;
					sfxSlider.value = Managers.Data.BasicSettingData.sfxVolume;
					break;
				case PageEnum.Video:
					resolutionButton.GetComponent<OptionSelector>().SetUIAsCurrentSetting();
					fullscreenButton.GetComponent<OptionSelector>().SetUIAsCurrentSetting();
					break;
				case PageEnum.Control:
					break;
				case PageEnum.Language:
					break;
			}
        }

		private void OnDestroy()
		{

		}

		private void GoToPage(int n)
		{
            InactiveAllPages();
            SetActivePage(n);
			UpdateSettings(n);
		}

        private void InactiveAllPages()
        {
            foreach (GameObject page in pages)
            {
                page.SetActive(false);
            }
        }

        private void SetActivePage(int n)
        {
            pages[n - 1].SetActive(true);
        }

    }
}