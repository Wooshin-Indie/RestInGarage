using DG.Tweening;
using Garage.Controller;
using Garage.Manager;
using Garage.Utils;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Garage.UI.GameScene.Items
{
    public class CarStatusUI : MonoBehaviour
    {
        [Header("Bubble UI")]
        [SerializeField] private RectTransform bubbleUIRect;
        [SerializeField] private RectTransform maskToFill;
        [SerializeField] private Image iconImageInBubble;
        [SerializeField] private RectTransform bubbleTailRect;
        [SerializeField] private RectTransform tailPivotRect;

        [Header("Blinking UI")]
        [SerializeField] private RectTransform blinkingUIRect;
        [SerializeField] private Image blinkingIconImage;

        [Header("Car Part Images")]
        [SerializeField] private Sprite tireEmptyImage;
        [SerializeField] private Sprite tireImage;
        [SerializeField] private Sprite engineImage;
        [SerializeField] private Sprite oilImage;

        [Header("Blinking Images")]
        [SerializeField] private Sprite wranchBlinkImage;
        [SerializeField] private Sprite tireBlinkImage;
        [SerializeField] private Sprite oilBlinkImage;
        [SerializeField] private Sprite fireBlinkImage;

        private CarController car;
        private Transform partTransform;
        private Vector3 bubbleUIScale = Vector3.one;
        private bool isEnlarged = false;
        private Color blinkOriginColor;
        private Color fireBlinkColor1;
        private Color fireBlinkColor2;
        private CarParts curPart = CarParts.FLT;

        private Transform localPlayerHipTf;
        private GraphicRaycaster uiRaycaster;
        private PointerEventData clickData;
        private List<RaycastResult> clickResults;

        private Vector3 playerHeadupOffset = new Vector3(0, 2f, 0);

        private void Awake()
        {
            blinkingUIRect.localScale = Vector3.one;
            bubbleUIRect.localScale = Vector3.zero;
            blinkOriginColor = blinkingIconImage.color;
            fireBlinkColor1 = new Color(224f / 255f, 37f / 255f, 37f / 255f, 0.9f);
            fireBlinkColor2 = new Color(255f / 255f, 238f / 255f, 124f / 255f, 0.9f);

            // Pivot Y 를 0으로 강제설정 (아래에서부터 채우기 위해)
            if (!Mathf.Approximately(maskToFill.pivot.y, 0f))
            {
                maskToFill.pivot = new Vector2(maskToFill.pivot.x, 0f);
            }

            ApplyFill(0f); // 처음 mask가 비어있게 설정
            transform.SetAsFirstSibling();


            // 씬에 있는 Canvas를 찾아 GraphicRaycaster 컴포넌트를 가져옵니다.
            uiRaycaster = FindFirstObjectByType<GraphicRaycaster>();
            if (uiRaycaster == null)
            {
                Debug.LogError("씬에 GraphicRaycaster 컴포넌트를 가진 Canvas가 없습니다!");
            }

            // 레이캐스트에 필요한 이벤트 데이터를 초기화합니다.
            clickData = new PointerEventData(EventSystem.current);
            clickResults = new List<RaycastResult>();
        }

        public void OnUpdate()
        {
            if (partTransform == null) return;

            if (!isEnlarged)
            {
                if (curPart != CarParts.Fire)
                {
                    OnUpdateBlinking();
                    OnUpdateScreenPos();
                }
                else
                {
                    OnUpdateFireBlinking();
                    OnUpdateFireScreenPos();
                }
                return;
            }

            if (curPart != CarParts.Fire)
                OnUpdateScreenPos();
            else
                OnUpdateFireScreenPos();
        }

        private float elapsedTime = 0f;
        private float blinkDuration = 0.7f;
        private void OnUpdateBlinking()
        {
            if (elapsedTime < blinkDuration)
            {
                blinkOriginColor.a = Mathf.Lerp(0.3f, 0.8f, elapsedTime/ blinkDuration);
            }
            else if (elapsedTime < 2 * blinkDuration)
            {
                blinkOriginColor.a = Mathf.Lerp(0.8f, 0.3f, (elapsedTime / blinkDuration) - 1);
            }
            else elapsedTime = 0f;

            blinkingIconImage.color = blinkOriginColor;
            elapsedTime += Time.deltaTime;
        }

        private Color tmpColor = Color.white;
        private float screenEdgeMargin = 80f;
        private void OnUpdateFireBlinking()
        {
            blinkDuration = Mathf.Lerp(1f, 0.05f, maskToFill.anchorMax.y);

            if (maskToFill.anchorMax.y < 0.7f)
            {
                if (elapsedTime < blinkDuration)
                {
                    blinkOriginColor.a = Mathf.Lerp(0.4f, 0.9f, elapsedTime / blinkDuration);
                }
                else if (elapsedTime < 2 * blinkDuration)
                {
                    blinkOriginColor.a = Mathf.Lerp(0.9f, 0.4f, (elapsedTime / blinkDuration) - 1);
                }
                else elapsedTime = 0f;

                blinkingIconImage.color = blinkOriginColor;
                elapsedTime += Time.deltaTime;
            }
            else // 터지기 직전에는 색바뀌면서 점멸
            {
                if (elapsedTime < blinkDuration)
                {
                    tmpColor = Color.Lerp(fireBlinkColor1, fireBlinkColor2, elapsedTime / blinkDuration);
                    tmpColor.a = Mathf.Lerp(0.5f, 0.95f, elapsedTime / blinkDuration);
                    blinkingIconImage.color = tmpColor;
                }
                else if (elapsedTime < 2 * blinkDuration)
                {
                    tmpColor = Color.Lerp(fireBlinkColor2, fireBlinkColor1, elapsedTime / blinkDuration - 1);
                    tmpColor.a = Mathf.Lerp(0.95f, 0.5f, (elapsedTime / blinkDuration) - 1);
                    blinkingIconImage.color = tmpColor;
                }
                else elapsedTime = 0f;

                elapsedTime += Time.deltaTime;
            }
        }

        private void OnUpdateScreenPos()
        {
            Vector3 screenPos = Vector3.zero;
            
            screenPos = Camera.main.WorldToScreenPoint(!isEnlarged ? 
                partTransform.position : 
                NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().transform.position + playerHeadupOffset);
            transform.position = screenPos;

            // ChangeBubbleRotation(0f);
			// CheckAndAdjustBubbleRotation();
		}

        private bool isFirstInBoundary = false;
        private void OnUpdateFireScreenPos()
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(partTransform.position);

            if (car.IsInBoundary())
            {
                if (screenPos.x <= 0)
                {
                    screenPos.x = screenEdgeMargin;
                    EnlargeCarPartUI();
                }
                else if (screenPos.x >= Camera.main.pixelWidth)
                {
                    screenPos.x = Camera.main.pixelWidth - screenEdgeMargin;
                    EnlargeCarPartUI();
                }
                else
                {
                    if (!isFirstInBoundary)
                    {
                        isFirstInBoundary = true;
                        ReduceCarPartUI();
                    }
                }
            }

            transform.position = screenPos;
        }

        public void InitCarStatusUI(CarController carCtr, CarParts carPart, GameSceneUI gameScene)
        {
            car = carCtr;
            curPart = carPart;
            uiRaycaster = gameScene.GetComponent<GraphicRaycaster>();
            SetUI(carPart);
            localPlayerHipTf = NetworkTransmission.instance.GetLocalPlayerController().HipTf;
        }

        private void SetUI(CarParts carPart)
        {
            switch (carPart)
            {
                // 이미지, 사이즈, 좌우반전, 위치 초기화
                case CarParts.FLT:
                    iconImageInBubble.sprite = tireEmptyImage;
                    blinkingIconImage.sprite = tireBlinkImage;
                    //SetUIScale(carPart);
                    break;
                case CarParts.FRT:
                    iconImageInBubble.sprite = tireEmptyImage;
                    blinkingIconImage.sprite = tireBlinkImage;
                    break;
                case CarParts.RLT:
                    iconImageInBubble.sprite = tireEmptyImage;
                    blinkingIconImage.sprite = tireBlinkImage;
                    break;
                case CarParts.RRT:
                    iconImageInBubble.sprite = tireEmptyImage;
                    blinkingIconImage.sprite = tireBlinkImage;
                    break;
                case CarParts.Engine:
                    iconImageInBubble.sprite = engineImage;
                    blinkingIconImage.sprite = wranchBlinkImage;
                    break;
                case CarParts.Oil:
                    iconImageInBubble.sprite = oilImage;
                    blinkingIconImage.sprite = oilBlinkImage;
                    break;
				case CarParts.Fire:
					iconImageInBubble.sprite = oilImage;
                    blinkingIconImage.sprite = fireBlinkImage;
                    maskToFill.GetComponent<Image>().color = Color.red;
					break;
			}

            partTransform = car.PartTransforms[(int)carPart];
        }

        private void SetUIScale(CarParts carPart) // 차량의 방향과 부품위치에 따라 스케일(좌우반전) 및 피봇 조정
        {
            if (car.Direction == VehicleDirection.Left)
            {
                switch (carPart)
                {
                    case CarParts.FLT:
                        bubbleUIScale = new Vector3(1f, -1f, 1f);
                        break;
                    case CarParts.FRT:
                        bubbleUIScale = new Vector3(1f, 1f, 1f);
                        break;
                    case CarParts.Oil:
                        bubbleUIScale = new Vector3(1f, -1f, 1f);
                        break;
                    case CarParts.Engine:
                        bubbleUIScale = new Vector3(-1f, 1f, 1f);
                        break;
                    case CarParts.RLT:
                        bubbleUIScale = new Vector3(-1f, -1f, 1f);
                        break;
                    case CarParts.RRT:
                        bubbleUIScale = new Vector3(-1f, 1f, 1f);
                        break;
                    case CarParts.Fire:
                        bubbleUIScale = new Vector3(-1f, 1f, 1f);
                        break;
                }
            }
            else // 아래쪽
            {
                switch (carPart)
                {
                    case CarParts.FLT:
                        bubbleUIScale = new Vector3(-1f, 1f, 1f);
                        break;
                    case CarParts.FRT:
                        bubbleUIScale = new Vector3(-1f, -1f, 1f);
                        break;
                    case CarParts.Oil:
                        bubbleUIScale = new Vector3(-1f, 1f, 1f);
                        break;
                    case CarParts.Engine:
                        bubbleUIScale = new Vector3(1f, 1f, 1f);
                        break;
                    case CarParts.RLT:
                        bubbleUIScale = new Vector3(1f, 1f, 1f);
                        break;
                    case CarParts.RRT:
                        bubbleUIScale = new Vector3(1f, -1f, 1f);
                        break;
                    case CarParts.Fire:
                        bubbleUIScale = new Vector3(1f, 1f, 1f);
                        break;
                }
            }
        }

        private float startScalingRatio = 0.05f;
        private float endScalingRatio = 0.95f;
        private float bubbleScalingAmount = 0.2f;
        // bubbleUI 에서 꼬리쪽 Mask는 좀 빠른 속도로 증가하도록 보이게 하기 위해서 구간별로 값 보정
        public void ApplyFill(float progress)
        {
            float fillAmount = 0f;
            if(progress < startScalingRatio)
            {
                fillAmount = Mathf.Lerp(0f, bubbleScalingAmount, progress / startScalingRatio);
            }
            else if (progress > endScalingRatio)
            {
                fillAmount = Mathf.Lerp(1f - bubbleScalingAmount, 1f, (progress - endScalingRatio) / startScalingRatio);
            }
            else
            {
                fillAmount = Mathf.Lerp(bubbleScalingAmount, 1f - bubbleScalingAmount, (progress - startScalingRatio) / endScalingRatio);
            }

            maskToFill.localScale = new Vector3(maskToFill.localScale.x, fillAmount, maskToFill.localScale.z);
        }

        private float uiExpandDuration = 0.2f;
        public void EnlargeCarPartUI()
        {
            if (isEnlarged == true) return;

            isEnlarged = true;
            bubbleUIRect.DOScale(1.5f * bubbleUIScale, uiExpandDuration).SetEase(Ease.OutCubic);
            blinkingUIRect.DOScale(Vector2.zero, uiExpandDuration).SetEase(Ease.OutCubic);
        }
        public void ReduceCarPartUI()
        {
            if (isEnlarged == false) return;

            isEnlarged = false;
            bubbleUIRect.DOScale(Vector2.zero, uiExpandDuration).SetEase(Ease.OutCubic);
            blinkingUIRect.DOScale(Vector2.one, uiExpandDuration).SetEase(Ease.OutCubic);
        }

        public void ChangeTireImage(bool inserted = true)
        {
            if(inserted)
            {
                iconImageInBubble.sprite = tireImage;
                blinkingIconImage.sprite = wranchBlinkImage;
            }
            else
            {
                iconImageInBubble.sprite = tireEmptyImage;
                blinkingIconImage.sprite = tireBlinkImage;
            }
        }

        private void ChangeBubbleImage()
        {

        }

        private bool IsPlayerBehindBubble()
        {
            // 2. 변환된 스크린 좌표를 포인터 이벤트 데이터의 위치로 설정합니다.
            clickData.position = Camera.main.WorldToScreenPoint(localPlayerHipTf.position);

            // 3. 레이캐스트 결과를 담을 리스트를 초기화합니다.
            clickResults.Clear();

            // 4. GraphicRaycaster를 사용하여 해당 스크린 위치에 UI 레이캐스트를 실행합니다.
            uiRaycaster.Raycast(clickData, clickResults);

            // 5. 결과 확인
            if (clickResults.Count > 0)
            {
                foreach(var result in clickResults)
                {
                    if (result.gameObject.CompareTag("BubbleUI"))
                        return true;
                }
            }

            return false;
        }

        private void CheckAndAdjustBubbleRotation()
        {
            if (isRotating) return;

            if (IsPlayerBehindBubble())
            {
                ChangeBubbleRotation(bubbleTailRect.rotation.eulerAngles.z + 20f);
            }
        }

        private Vector2 maxTailPivotPos = new Vector2(0f, 10f);
        private bool isRotating = false;
        // 각도가 바뀌어도 말풍선 꼬리 일정하게 보이도록 tailPivot 위치 이동
        private void ChangeBubbleRotation(float eulerRotationZ)
        {
            if (eulerRotationZ >= 0f)
                eulerRotationZ = eulerRotationZ % 360f;
            else
                eulerRotationZ = eulerRotationZ % 360f + 360f;

            float angularDistance = 0f;
            Vector2 targetAnchoredPos;
            
            bubbleTailRect.DORotate(new Vector3(0f, 0f, eulerRotationZ), 0.4f).
                OnComplete(()=>
                isRotating = false);

            if ((eulerRotationZ > 60f && eulerRotationZ < 120f) || (eulerRotationZ > 240f && eulerRotationZ < 300f))
            {
                targetAnchoredPos = maxTailPivotPos;
            }
            else
            {
                if (eulerRotationZ >= 120f && eulerRotationZ <= 240f)
                {
                    angularDistance = Mathf.Abs(eulerRotationZ - 180f);
                }
                else if (eulerRotationZ <= 60f)
                {
                    angularDistance = eulerRotationZ;
                }
                else
                {
                    angularDistance = Mathf.Abs(eulerRotationZ - 360f);
                }

                targetAnchoredPos = Vector3.Lerp(Vector2.zero, maxTailPivotPos, angularDistance / 60f);
            }
            Sequence seq = DOTween.Sequence();

            isRotating = true;
            seq.Append(bubbleTailRect.DORotate(new Vector3(0f, 0f, eulerRotationZ), 0.4f).SetEase(Ease.InOutSine)).
                Join(tailPivotRect.DOAnchorPos(targetAnchoredPos, 0.4f).SetEase(Ease.InOutSine));

            seq.OnComplete(() =>
            isRotating = false);

            return;
        }
    }
}