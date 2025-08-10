using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;
namespace Garage.UI.GameScene.Items
{
    public class EmotePopupUI : MonoBehaviour
    {
        private RectTransform rect;
        private Vector3 originScale;
        private Transform target = null;
        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            originScale = rect.localScale;
            rect.localScale = Vector3.zero;
        }

        private void Update()
        {
            if (target == null) return;

            Vector3 targetPos = target.position + Vector3.up * 1.5f;
            transform.position = Camera.main.WorldToScreenPoint(targetPos);
        }

        public void PopEmoteUI(Transform target)
        {
            this.target = target;
            Sequence seq = DOTween.Sequence();

            seq.Append(rect.DOScale(1.2f * originScale, 0.1f).SetEase(Ease.OutCubic))
                .Append(rect.DOScale(0.9f * originScale, 0.07f).SetEase(Ease.InOutCubic))
                .Append(rect.DOScale(originScale, 0.05f).SetEase(Ease.InOutCubic))
                .AppendInterval(0.5f)
                .Append(rect.DOScale(0.9f * originScale, 0.05f).SetEase(Ease.InOutCubic))
                .Append(rect.DOScale(1.2f * originScale, 0.07f).SetEase(Ease.InOutCubic))
                .Append(rect.DOScale(Vector3.zero, 0.1f).SetEase(Ease.InOutCubic));

            seq.OnComplete(() =>
            {
                this.target = null;
                Destroy(gameObject);
            });

            //seq.Append(rect.DOScale(originScale, 0.25f).SetEase(Ease.OutElastic))
            //    .AppendInterval(0.15f)
            //    .Append(rect.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InElastic));
        }
    }
}
