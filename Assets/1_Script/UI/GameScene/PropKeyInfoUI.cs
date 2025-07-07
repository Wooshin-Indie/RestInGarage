using DG.Tweening;
using Garage.Interfaces;
using Garage.Props;
using Garage.Structs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.U2D.ScriptablePacker;

public class PropKeyInfoUI : MonoBehaviour, IWorldSpaceUI, IPopupUI
{
    [SerializeField] private GameObject keyInfoUIPrefab;
    [SerializeField] private bool isAnimated;
    [SerializeField] private bool isWorldSpaceUI;
    [SerializeField] private int initialNumberOfUI;
    private List<KeyInfoUI> keyInfoUIList = new();
    private Transform targetTransform = null;
    private RectTransform rect;
    private CanvasGroup canvasGroup;
    private float animateDuration = 0.2f;

    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        for (int i = 0; i < initialNumberOfUI; i++)
        {
            KeyInfoUI tmpKeyInfoUI = Instantiate(keyInfoUIPrefab, transform).GetComponent<KeyInfoUI>();
            keyInfoUIList.Add(tmpKeyInfoUI);

            tmpKeyInfoUI.gameObject.SetActive(false);
        }
        rect = GetComponent<RectTransform>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (isAnimated)
        {
            canvasGroup.alpha = 0f;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    public void OnUpdate()
    {
        if (targetTransform == null) return;
        if (!isWorldSpaceUI) return;

        UpdateUIScreenPos();
    }
    public void UpdateUIScreenPos()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(targetTransform.transform.position);
        transform.position = screenPos;
    }
    public void SetPropKeyInfoUI(Transform target, List<KeyData> propKeyDataList)
    {
        // KeyData띄울 KeyInfoUI가 부족하면 더 Instantiate
        if (propKeyDataList.Count > keyInfoUIList.Count)
        {
            int gap = propKeyDataList.Count - keyInfoUIList.Count;
            for (int i = 0; i < gap; i++)
            {
                KeyInfoUI tmpKeyInfoUI = Instantiate(keyInfoUIPrefab, transform).GetComponent<KeyInfoUI>();
                keyInfoUIList.Add(tmpKeyInfoUI);
            }
        }

        // Key갯수만큼만 켜서 정보 Update하고 나머지는 끄기
        int idx = 0;
        foreach (var keyData in propKeyDataList)
        {
            keyInfoUIList[idx].gameObject.SetActive(true);
            keyInfoUIList[idx++].SetKeyInfoUI(keyData);
        }
        for (int i = propKeyDataList.Count; i < keyInfoUIList.Count; i++)
        {
            keyInfoUIList[i].gameObject.SetActive(false);
        }

        targetTransform = target;
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
    }
    public void PopUI()
    {
        if (isAnimated)
        {
            // TODO - 애니메이션
            canvasGroup.DOFade(1f, animateDuration).SetEase(Ease.OutCubic);
        }
        else
        {
            gameObject.SetActive(true);
        }

    }
    public void CloseUI()
    {
        if (isAnimated)
        {
            // TODO - 애니메이션
            canvasGroup.DOFade(0f, animateDuration).SetEase(Ease.OutCubic).OnComplete(() =>
            {
                targetTransform = null;
            });
        }
        else
        {
            gameObject.SetActive(false);
            targetTransform = null;
        }
    }
}
