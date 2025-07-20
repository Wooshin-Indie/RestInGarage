using DG.Tweening;
using System.Collections;
using UnityEngine;

public class BossWarningUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup UIgroup;
    [SerializeField] private float warningDuration;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void StartBossWarningVFX()
    {
        gameObject.SetActive(true);
        StartCoroutine(BlinkingCoroutine());
    }

    private IEnumerator BlinkingCoroutine()
    {
        UIgroup.DOFade(1f, 0.4f);
        yield return new WaitForSeconds(0.4f);
        UIgroup.DOFade(0.5f, 0.8f);
        yield return new WaitForSeconds(0.8f);
        UIgroup.DOFade(1f, 0.4f);
        yield return new WaitForSeconds(0.4f);
        UIgroup.DOFade(0.5f, 0.8f);
        yield return new WaitForSeconds(0.8f);
        UIgroup.DOFade(1f, 0.4f);
        yield return new WaitForSeconds(0.4f);
        UIgroup.DOFade(0.5f, 0.8f);
        yield return new WaitForSeconds(0.8f);

        gameObject.SetActive(false);
    }
}
