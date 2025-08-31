using DG.Tweening;
using System.Collections;
using UnityEngine;

public class BossWarningUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup UIgroup;
    [SerializeField] private float warningDuration;

    public void StartBossWarningVFX()
    {
        gameObject.SetActive(true);
        StartBlinking();
    }

    private void StartBlinking()
    {
        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < 3; i++)
        {
            seq.Append(UIgroup.DOFade(1f, 0.4f))
               .Append(UIgroup.DOFade(0.5f, 0.8f));
        }

        seq.OnComplete(() => gameObject.SetActive(false));
    }
}
