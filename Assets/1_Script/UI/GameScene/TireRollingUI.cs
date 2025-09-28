using UnityEngine;

public class TireRollingUI : MonoBehaviour
{
    [SerializeField] private RectTransform greenFillMask;

    private Vector3 initialScale = new Vector3(1f, 0f, 1f);
    private Vector3 targetScale;
    private void Awake()
    {
        targetScale = initialScale;
        greenFillMask.localScale = initialScale;
    }

    public void ApplyRollGage(float gage) // gage -> 0f~1f
    {
        targetScale.y = gage;
        greenFillMask.localScale = targetScale;
    }

    public void PopUI(Transform tf)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(tf.position);
        transform.position = screenPos;
        gameObject.SetActive(true);
    }
    public void CloseUI()
    {
        gameObject.SetActive(false);
    }
}
