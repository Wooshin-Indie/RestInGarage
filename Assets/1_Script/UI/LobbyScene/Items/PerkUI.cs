using Garage.Manager;
using Manager;
using Steamworks.Data;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PerkUI : MonoBehaviour
{
    [SerializeField] public StatEnum Stat;
    [SerializeField] public float Value;
    [SerializeField] private bool isLocked;
    private KeyValuePair<StatEnum, float> perk;
    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        perk = new(Stat, Value);
        
        button.onClick.AddListener(() =>
        {
            UIManager.Main.LobbyPage.SetCurrentPerk(perk);
        });

        if (isLocked)
        {
            LockPerk();
        }
        else
        {
            UnlockPerk();
        }
    }

    public void LockPerk()
    {
        button.enabled = false;
        GetComponent<UnityEngine.UI.Image>().color = UnityEngine.Color.black;
    }
    public void UnlockPerk()
    {
        button.enabled = true;
        GetComponent<UnityEngine.UI.Image>().color = UnityEngine.Color.grey;
    }
}
