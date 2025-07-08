using Garage.Structs;
using Garage.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.UI;

public class KeyInfoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI keyText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Sprite spacebarSprite;
    private LocalizedString descriptionString;
    private KeyCode? boundKey;

    public void SetKeyInfoUI(KeyData keyData)
    {
        descriptionString = keyData.LocalizedDescription;

        keyText.text = keyData.KeyDisplayName;
        descriptionText.text = descriptionString.GetLocalizedString();

        boundKey = keyData.Action.action.GetFirstKeyboardBinding();
        if (boundKey.HasValue && boundKey.Value == KeyCode.Space)
        {
            keyText.gameObject.SetActive(false);
            iconImage.gameObject.SetActive(true);
            iconImage.sprite = spacebarSprite;
        }
        else
        {
            keyText.gameObject.SetActive(true);
            iconImage.gameObject.SetActive(false);
        }

        descriptionString.StringChanged += UpdateDescriptionString;
    }
    private void UpdateDescriptionString(string localizedString)
    {
        descriptionText.text = localizedString;
    }

    private void OnDestroy()
    {
        descriptionString.StringChanged -= UpdateDescriptionString;
    }
}
