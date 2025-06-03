using Garage.Controller;
using Garage.Utils;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Newtonsoft.Json.Bson;
using Unity.VisualScripting;
using UnityEditor.Localization.Plugins.XLIFF.V20;

namespace Garage.UI.GameScene.Items
{
    public class CarStatusUI : MonoBehaviour
    {
        [Header("Bubble UI")]
        [SerializeField] private RectTransform bubbleUIRect;
        [SerializeField] private RectTransform maskToFill;
        [SerializeField] private RectTransform bubbleImageRect;
        [SerializeField] private Image bubbleIconImage;

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

        private Camera mainCam;
        private CarController car;
        private Transform partPos;
        private Vector3 bubbleUIScale = Vector3.one;
        private bool isEnlarged = false;
        private Color blinkOriginColor;
        private Color fireBlinkColor1;
        private Color fireBlinkColor2;
        private CarParts curPart = CarParts.FLT;

        private void Awake()
        {
            mainCam = Camera.main;

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

            // AnchorMin.y를 0으로 강제설정
            Vector2 currentAnchorMin = maskToFill.anchorMin;
            if (!Mathf.Approximately(currentAnchorMin.y, 0f))
            {
                maskToFill.anchorMin = new Vector2(currentAnchorMin.x, 0f);
            }

            ApplyFill(0f); // 처음 mask가 비어있게 설정
        }

        public void OnUpdate()
        {
            if (partPos == null) return;

            if (!isEnlarged)
            {
                if (curPart != CarParts.Fire)
                    OnUpdateBlinking();
                else
                    OnUpdateFireBlinking();
            }

            Vector3 tmpPos = mainCam.WorldToScreenPoint(partPos.position);
            transform.position = tmpPos;
            
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

        public void InitCarStatusUI(CarController carCtr, CarParts carPart)
        {
            car = carCtr;
            curPart = carPart;
            SetUI(carPart);
        }

        private void SetUIPos(CarParts carPart)
        {
            
        }

        private void SetUI(CarParts carPart)
        {
            switch (carPart)
            {
                // 이미지, 사이즈, 좌우반전, 위치 초기화
                case CarParts.FLT:
                    bubbleIconImage.sprite = tireEmptyImage;
                    blinkingIconImage.sprite = tireBlinkImage;
                    SetUIScale(carPart);
                    break;
                case CarParts.FRT:
                    bubbleIconImage.sprite = tireEmptyImage;
                    blinkingIconImage.sprite = tireBlinkImage;
                    SetUIScale(carPart);
                    break;
                case CarParts.RLT:
                    bubbleIconImage.sprite = tireEmptyImage;
                    blinkingIconImage.sprite = tireBlinkImage;
                    SetUIScale(carPart);
                    break;
                case CarParts.RRT:
                    bubbleIconImage.sprite = tireEmptyImage;
                    blinkingIconImage.sprite = tireBlinkImage;
                    SetUIScale(carPart);
                    break;
                case CarParts.Engine:
                    bubbleIconImage.sprite = engineImage;
                    blinkingIconImage.sprite = wranchBlinkImage;
                    SetUIScale(carPart);
                    break;
                case CarParts.Oil:
                    bubbleIconImage.sprite = oilImage;
                    blinkingIconImage.sprite = oilBlinkImage;
                    SetUIScale(carPart);
                    break;
				case CarParts.Fire:
					bubbleIconImage.sprite = oilImage;
                    blinkingIconImage.sprite = fireBlinkImage;
                    maskToFill.GetComponent<Image>().color = Color.red;
					SetUIScale(carPart);
					break;
			}

            partPos = car.PartTransforms[(int)carPart];
        }
        
        private void SetUIScale(CarParts carPart) // 차량의 방향과 부품위치에 따라 스케일(좌우반전) 및 피봇 조정
        {
            if (car.Direction == VehicleDirection.Up)
            {
                switch (carPart)
                {
                    case CarParts.FLT:
                        bubbleUIScale = new Vector3(1f, -1f, 1f);
                        break;
                    case CarParts.FRT:
                        bubbleUIScale = new Vector3(1f, 1f, 1f);
                        blinkingUIRect.pivot = new Vector2(0.5f, 0.2f);
                        break;
                    case CarParts.Oil:
                        bubbleUIScale = new Vector3(1f, -1f, 1f);
                        bubbleImageRect.localScale = new Vector3(1f, -1f, 1f);
                        break;
                    case CarParts.Engine:
                        bubbleUIScale = new Vector3(-1f, 1f, 1f);
                        bubbleImageRect.localScale = new Vector3(-1f, 1f, 1f);
                        break;
                    case CarParts.RLT:
                        bubbleUIScale = new Vector3(-1f, -1f, 1f);
                        break;
                    case CarParts.RRT:
                        bubbleUIScale = new Vector3(-1f, 1f, 1f);
                        blinkingUIRect.pivot = new Vector2(0.5f, 0.2f);
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
                        blinkingUIRect.pivot = new Vector2(0.5f, 0.2f);
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
                        blinkingUIRect.pivot = new Vector2(0.5f, 0.2f);
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

        public void ApplyFill(float progress)
        {
            maskToFill.anchorMax = new Vector2(maskToFill.anchorMax.x, progress);
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
                bubbleIconImage.sprite = tireImage;
                blinkingIconImage.sprite = wranchBlinkImage;
            }
            else
            {
                bubbleIconImage.sprite = tireEmptyImage;
                blinkingIconImage.sprite = tireBlinkImage;
            }
        }
	}
}