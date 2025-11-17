using DG.Tweening;
using UnityEngine;

public class PlayerGageUI : MonoBehaviour
{
    [SerializeField] private RectTransform greenFillMask;

    private Vector3 initialMaskScale = new Vector3(1f, 0f, 1f);
    private Vector3 targetScale;
    private RectTransform rect;
    private Vector3 originRectScale;
    private Vector3 reducedRectScale = new Vector3(1f, 0f, 1f);
    private void Awake()
    {
        targetScale = initialMaskScale;
        greenFillMask.localScale = initialMaskScale;
        rect = GetComponent<RectTransform>();
        originRectScale = rect.localScale;
    }

    public void ApplyRollGage(float gage) // gage -> 0f~1f
    {
        targetScale.y = gage;
        greenFillMask.localScale = targetScale;
    }

    private bool isPopped = false;
    private float uiExpandDuration = 0.2f;
    public void PopUI(Transform tf)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(tf.position);
        transform.position = screenPos;
        if (isPopped == true) return;

        gameObject.SetActive(true);
        isPopped = true;
        rect.DOScale(1.5f * originRectScale, uiExpandDuration).SetEase(Ease.OutCubic);
    }
    public void CloseUI()
    {
        if (isPopped == false) return;

        isPopped = false;
        rect.DOScale(reducedRectScale, uiExpandDuration).SetEase(Ease.OutCubic).
            OnComplete(() =>
            gameObject.SetActive(false));
    }
}
