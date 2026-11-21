using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Game.Scripts.Gameplay.UI
{
    public class ScoreBonusPopup : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;

        public void Show(string displayText, float delay)
        {
            _text.text = displayText;

            var rt = (RectTransform)transform;
            var canvasGroup = gameObject.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 0f;
            rt.localScale = Vector3.one * 0.3f;

            var seq = DOTween.Sequence();
            seq.SetDelay(delay);

            seq.Append(canvasGroup.DOFade(1f, 0.15f));
            seq.Join(rt.DOScale(Vector3.one * 1.2f, 0.2f).SetEase(Ease.OutBack));
            seq.Append(rt.DOScale(Vector3.one, 0.1f).SetEase(Ease.InOutSine));

            seq.AppendInterval(0.2f);
           
            seq.Append(rt.DOAnchorPosY(rt.anchoredPosition.y + 120f, 1.4f).SetEase(Ease.OutSine));
            seq.Join(canvasGroup.DOFade(0f, 0.6f).SetDelay(0.8f));

            seq.OnComplete(() => Destroy(gameObject));
        }
    }
}
