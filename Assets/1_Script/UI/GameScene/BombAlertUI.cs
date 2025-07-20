using Garage.Interfaces;
using UnityEngine;
using UnityEngine.UI;

public class BombAlertUI : MonoBehaviour
{
    [SerializeField] private Image blinkingIconImage;
    private Transform target = null;
    private Color blinkColor1;
    private Color blinkColor2;

    private void Awake()
    {
        blinkColor1 = new Color(224f / 255f, 37f / 255f, 37f / 255f, 0.9f);
        blinkColor2 = new Color(255f / 255f, 238f / 255f, 124f / 255f, 0.9f);
    }

    public void Init(Transform target)
    {
        this.target = target;
    }

    private Color tmpColor = Color.white;
    private float elapsedTime = 0f;
    private float blinkDuration = 0.1f;
    public void OnUpdateBombAlertUI()
    {
        if (elapsedTime < blinkDuration)
        {
            tmpColor = Color.Lerp(blinkColor1, blinkColor2, elapsedTime / blinkDuration);
            tmpColor.a = Mathf.Lerp(0.5f, 0.95f, elapsedTime / blinkDuration);
            blinkingIconImage.color = tmpColor;
        }
        else if (elapsedTime < 2 * blinkDuration)
        {
            tmpColor = Color.Lerp(blinkColor2, blinkColor1, elapsedTime / blinkDuration - 1);
            tmpColor.a = Mathf.Lerp(0.95f, 0.5f, (elapsedTime / blinkDuration) - 1);
            blinkingIconImage.color = tmpColor;
        }
        else elapsedTime = 0f;

        elapsedTime += Time.deltaTime;
    }

    public void UpdateUIScreenPos()
    {
        if (target != null)
            transform.position = Camera.main.WorldToScreenPoint(target.transform.position);
    }
}
