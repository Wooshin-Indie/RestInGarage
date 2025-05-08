using Garage.Controller;
using Garage.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Garage.UI.GameScene.Items
{
    public class CarStatusUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private RectTransform maskToFill;

        [Header("Car Part Images")]
        [SerializeField] private Sprite tireImage;
        [SerializeField] private Sprite engineImage;
        [SerializeField] private Sprite oilImage;

        private Camera mainCam;
        private CarController car;
        private Transform partPos;
        private RectTransform uiRect;
        private Vector2 originSize;

        private void Start()
        {
            mainCam = Camera.main;
            uiRect = GetComponent<RectTransform>();
            originSize = uiRect.sizeDelta;

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
            Vector3 tmpPos = mainCam.WorldToScreenPoint(partPos.position);
            transform.position = tmpPos;
        }

        public void InitCarStatusUI(CarController carCtr, CarParts carPart)
        {
            car = carCtr;
            SetUI(carPart);
        }

        private void SetUIPos(CarParts carPart)
        {
            
        }

        private void SetUI(CarParts carPart)
        {
            RectTransform rt = iconImage.rectTransform;
            switch (carPart)
            {
                // 이미지, 사이즈, 좌우반전, 위치 초기화
                case CarParts.FLT:
                    iconImage.sprite = tireImage;
                    SetTireUI(carPart);
                    break;
                case CarParts.FRT:
                    iconImage.sprite = tireImage;
                    SetTireUI(carPart);
                    break;
                case CarParts.RLT:
                    iconImage.sprite = tireImage;
                    SetTireUI(carPart);
                    break;
                case CarParts.RRT:
                    iconImage.sprite = tireImage;
                    iconImage.rectTransform.localScale = new Vector3(-1f, 1f, 1f);
                    SetTireUI(carPart);
                    break;
                case CarParts.Engine:
                    iconImage.sprite = engineImage;
                    break;
                case CarParts.Oil:
                    iconImage.sprite = oilImage;
                    break;
            }

            partPos = car.PartTransforms[(int)carPart];
        }
        
        private void SetTireUI(CarParts carPart) // 차량의 방향과 바퀴위치에 따라 이미지반전 및 pivot 수정
        {// 피봇이 1f 면 원래보다 왼쪽으로 이동, 0f면 오른쪽으로 이동
            RectTransform rt = iconImage.rectTransform;
            if(car.Direction == VehicleDirection.Up)
            {
                switch (carPart)
                {
                    case CarParts.FLT: // 왼쪽이면 좌우반전
                    case CarParts.RLT:
                        rt.localScale = new Vector3(-1f, 1f, 1f);
                        break;
                    case CarParts.FRT:
                    case CarParts.RRT:
                        rt.localScale = new Vector3(1f, 1f, 1f);
                        break;
                }
            }
            else
            {
                switch (carPart)
                {
                    case CarParts.FLT:
                    case CarParts.RLT:
                        rt.localScale = new Vector3(1f, 1f, 1f);
                        break;
                    case CarParts.FRT: // 오른 쪽이면 좌우반전
                    case CarParts.RRT:
                        rt.localScale = new Vector3(-1f, 1f, 1f);
                        break;
                }
            }

            rt.pivot = new Vector2(0f, 0f);
        }

        public void ApplyFill(float progress)
        {
            maskToFill.anchorMax = new Vector2(maskToFill.anchorMax.x, progress);
        }

        public void ResizeCarPartUI(bool enlarge)
        {
            // 크기 확대
            if (enlarge)
            {
                uiRect.sizeDelta = 1.5f * originSize;
            }
            // 크기 축소
            else
            {
                uiRect.sizeDelta = originSize;
            }
        }
    }
}