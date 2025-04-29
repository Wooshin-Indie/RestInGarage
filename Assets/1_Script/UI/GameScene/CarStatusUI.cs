using Garage.Controller;
using Garage.Structs;
using Garage.Utils;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Security.Cryptography;
using UnityEditor;
using static UnityEngine.ParticleSystem;
using Unity.VisualScripting;

namespace Garage.UI.GameScene.Items
{
    public class CarStatusUI : MonoBehaviour
    {
        [SerializeField] Image iconImage;
        [Header("Car Part Images")]
        [SerializeField] private Sprite TireImage;
        [SerializeField] private Sprite EngineImage;
        [SerializeField] private Sprite OilImage;

        private Camera mainCam;
        private CarController car;
        private Transform partPos;

        private void Awake()
        {
            mainCam = Camera.main;
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
                    iconImage.sprite = TireImage;
                    rt.sizeDelta = new Vector2(48, 35);
                    SetTireUI(carPart);
                    break;
                case CarParts.FRT:
                    iconImage.sprite = TireImage;
                    rt.sizeDelta = new Vector2(48, 35);
                    SetTireUI(carPart);
                    break;
                case CarParts.RLT:
                    iconImage.sprite = TireImage;
                    rt.sizeDelta = new Vector2(48, 35);
                    SetTireUI(carPart);
                    break;
                case CarParts.RRT:
                    iconImage.sprite = TireImage;
                    rt.sizeDelta = new Vector2(48, 35);
                    iconImage.rectTransform.localScale = new Vector3(-1f, 1f, 1f);
                    SetTireUI(carPart);
                    break;
                case CarParts.Engine:
                    iconImage.sprite = EngineImage;
                    rt.sizeDelta = new Vector2(35, 35);
                    break;
                case CarParts.Oil:
                    iconImage.sprite = OilImage;
                    rt.sizeDelta = new Vector2(35, 35);
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
    }
}