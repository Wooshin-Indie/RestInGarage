using DG.Tweening;
using Garage.Manager;
using Garage.UI.Item;
using IUtil;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace Garage.UI.MainScene
{
	public enum PageEnum
    {
        None = 0,
        Main, Multi, Host, Settings,		// == 4
        Audio, Video, Control, Language,	// == 8
		Browse, Lobby 
    }

    public class MainSceneUI : MonoBehaviour
	{
		[SerializeField] private List<GameObject> pages = new List<GameObject>();
		[SerializeField] private GameObject title;

		[FoldoutGroup("Page1")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingButton;
        [SerializeField] private Button quitButton;

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
		[SerializeField] private Slider brightnessSlider;
		[SerializeField] private Button page6BackButton;

		[FoldoutGroup("Page7 - Control")]
		[SerializeField] private Button page7BackButton;

		[FoldoutGroup("Page8 - Language")]
		[SerializeField] private List<Button> languageButtons = new();
		[SerializeField] private Button page8BackButton;

		[FoldoutGroup("Page9 - Browse")]
		[SerializeField] private BrowsePageUI browsePage;
		[SerializeField] private Button page9BackButton;

        [FoldoutGroup("Page10 - Lobby")]
        [SerializeField] private LobbyPageUI lobbyPage;
		// 버튼은 LobbyPageUI에서 관리

		public LobbyPageUI LobbyPage => lobbyPage;

        private void Start()
		{
            /** Page 1 **/
            playButton.onClick.AddListener(() => { GoToPage(PageEnum.Multi); });
            settingButton.onClick.AddListener(() => { GoToPage(PageEnum.Settings); });
			quitButton.onClick.AddListener(() => { Application.Quit(); });

			/** Page 2 **/
			multyPlayButton.onClick.AddListener(() => { GoToPage(PageEnum.Host); });
			Page2BackButton.onClick.AddListener(() => { GoToPage(PageEnum.Main); });

			/** Page 3 **/
			hostButton.onClick.AddListener(() => // 로비 생성
			{
				// 로비 정보 셋업
				GameNetworkManager.Instance.CreateLobby();
				// TODO - MainUI 초기화 코드 필요
			});
			guestButton.onClick.AddListener(() =>
			{
				GoToPage(PageEnum.Browse);
				browsePage.StartLoading();
				GameNetworkManager.Instance.FindLobbiesWithCallback((lobbies) =>
				{
					browsePage.RevealLobbies(lobbies);
				});
			});

            Page3BackButton.onClick.AddListener(() => { GoToPage(PageEnum.Multi); });

            /** Page 4 **/
            audioButton.onClick.AddListener(() => { GoToPage(PageEnum.Audio); });
			videoButton.onClick.AddListener(() => { GoToPage(PageEnum.Video); });
			controlButton.onClick.AddListener(() => { GoToPage(PageEnum.Control); });
			languageButton.onClick.AddListener(() => { GoToPage(PageEnum.Language); });
            page4BackButton.onClick.AddListener(() => { GoToPage(PageEnum.Main); });

            AddListenersToSetting();
		}

		private void OnEnable()
		{
			GoToPage(PageEnum.Main);
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
			page5BackButton.onClick.AddListener(() => { GoToPage(PageEnum.Settings); });

			/** Page 6 **/
			resolutionButton.onClick.AddListener(() => { resolutionButton.GetComponent<OptionSelector>().ApplySetting(); });
			fullscreenButton.onClick.AddListener(() => { fullscreenButton.GetComponent<OptionSelector>().ApplySetting(); });
			brightnessSlider.onValueChanged.AddListener((value) =>
			{
				Managers.Data.BasicSettingData.brightness = value;
				Screen.brightness = value;
			});
			page6BackButton.onClick.AddListener(() => { GoToPage(PageEnum.Settings); });

			/** Page 7 **/
			page7BackButton.onClick.AddListener(() => { GoToPage(PageEnum.Settings); });

			/** Page 8 **/
			for (int i=0; i< languageButtons.Count; i++)
			{
				int t = i;
				languageButtons[i].onClick.AddListener(() => { 
					Managers.Data.BasicSettingData.languageIndex = t; 
					LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[t];
				});
			}
			page8BackButton.onClick.AddListener(() => { GoToPage(PageEnum.Settings); });

            /** Page 9 **/
            page9BackButton.onClick.AddListener(() => { GoToPage(PageEnum.Host); });
		}

        private void UpdateSettings(PageEnum page)
        {
            switch (page)
            {
				case PageEnum.Settings:
					Managers.Data.SaveAll();
					Managers.Data.ApplyBasicSettings();
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
					brightnessSlider.value = Managers.Data.BasicSettingData.brightness;
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

		public void GoToPage(PageEnum page)
		{
            InactiveAllPages();
            SetActivePage((int)page);
			WhetherSetActiveTitle(page);
            UpdateSettings(page);
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

		private void WhetherSetActiveTitle(PageEnum page)
		{
            switch (page)
            {
                case PageEnum.Main:
                case PageEnum.Multi:
                case PageEnum.Host:
                case PageEnum.Settings:
                    title.SetActive(true);
                    break;
                case PageEnum.Audio:
                case PageEnum.Video:
                case PageEnum.Control:
                case PageEnum.Language:
                case PageEnum.Browse:
                case PageEnum.Lobby:
					title.SetActive(false);
                    break;
            }
        }
    }
}