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
    }
}
