using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.GameRoot.BaseAnimations
{
    [RequireComponent(typeof(Image))]
    public class UIFillOnEnableImage : MonoBehaviour
    {
        [SerializeField] private Image _targetImage;
        [SerializeField] private float _duration = 3f;
        [SerializeField] private Ease _ease = Ease.Linear;
        [SerializeField] private TMP_Text _percentText;
        private Tween _fillTween;
        private Tween _textTween;

        private void Awake()
        {
            if (_targetImage == null)
                _targetImage = GetComponent<Image>();
        }

        private void OnEnable()
        {
            Debug.Log("OnEnable");
            if (_targetImage == null)
            {
                Debug.LogWarning(
                    "UIFillOnEnableImage: target image is null — assign in inspector or add Image on the same GameObject.");
                return;
            }

            StartCoroutine(StartFillNextFrame());
        }

        private IEnumerator StartFillNextFrame()
        {
            if (_fillTween != null && _fillTween.IsActive())
            {
                _fillTween.Kill();
                _fillTween = null;
            }

            if (_textTween != null && _textTween.IsActive())
            {
                _textTween.Kill();
                _textTween = null;
            }

            _targetImage.fillAmount = 0f;

            if (_percentText != null)
                _percentText.text = "0%";

            yield return null;

            if (!gameObject.activeInHierarchy || _targetImage == null)
                yield break;

            var dur = Mathf.Max(0.01f, _duration);

            _fillTween = DOTween
                .To(() => _targetImage.fillAmount, x => _targetImage.fillAmount = x, 1f, dur)
                .SetEase(_ease)
                .SetTarget(_targetImage)
                .OnStart(() => Debug.Log("Fill tween started"))
                .OnComplete(() => Debug.Log("Fill tween completed"));

            if (_percentText != null)
            {
                float value = 0f;
                _textTween = DOTween
                    .To(() => value, x =>
                    {
                        value = x;
                        _percentText.text = Mathf.RoundToInt(x) + "%";
                    }, 100f, dur)
                    .SetEase(_ease)
                    .SetTarget(_percentText);
            }
        }

        private void OnDisable()
        {
            if (_fillTween != null && _fillTween.IsActive())
            {
                _fillTween.Kill();
                _fillTween = null;
            }

            if (_textTween != null && _textTween.IsActive())
            {
                _textTween.Kill();
                _textTween = null;
            }

            if (_targetImage != null)
                _targetImage.fillAmount = 0f;

            if (_percentText != null)
                _percentText.text = "0%";
        }
    }
}