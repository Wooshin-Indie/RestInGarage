using Garage.Manager;
using Garage.Utils;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Garage.UI.MainScene
{
    public class MainSceneUI : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingButton;
        [SerializeField] private Button multyPlayButton;
        [SerializeField] private Button singlePlayButton;
        [SerializeField] private Button hostButton;
        [SerializeField] private Button guestButton;
        [SerializeField] private Button Page2BackButton;
        [SerializeField] private Button Page3BackButton;
        [SerializeField] private List<GameObject> pages = new List<GameObject>();

        private void Start()
		{
            playButton.onClick.AddListener(() =>
            {
                GoToPage(2);
            });
            multyPlayButton.onClick.AddListener(() =>
            {
                GoToPage(3);
            });
            hostButton.onClick.AddListener(() =>
			{
				GameNetworkManager.Instance.StartHost(Constants.MAX_PLAYERS);
			});
            Page2BackButton.onClick.AddListener(() =>
            {
                GoToPage(1);
            });
            Page3BackButton.onClick.AddListener(() =>
            {
                GoToPage(2);
            });
        }

		private void OnDestroy()
		{
			hostButton.onClick.RemoveAllListeners();
		}

		private void GoToPage(int n)
		{
            InactiveAllPages();
            SetActivePage(n);
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