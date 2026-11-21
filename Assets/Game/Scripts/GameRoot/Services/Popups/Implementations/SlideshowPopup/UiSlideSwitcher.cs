using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Scripts.Game.SlideshowGamePopup
{
    public class UiSlideSwitcher : MonoBehaviour
    {
        [Header("Slides")]
        [SerializeField] private List<SlideElement> slides = new List<SlideElement>();

        [Header("Settings")]
        [SerializeField] private int startIndex = 0;

        [SerializeField] private List<Image> slideNumbers = new List<Image>();
        [SerializeField] private Sprite offSprite;
        [SerializeField] private Sprite onSprite;

        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;

        [Header("How to play settings")]
        // [SerializeField] private TMP_Text buttonText;
        [SerializeField] private string sceneName;
        
        [Header("Arrow Anim")]
        [SerializeField] private float buttonFadeDuration = 0.18f;
        [SerializeField] private float buttonScaleHidden = 0.92f;

        public event Action<int, int> OnSlideChanged;

        private int _current;
        private bool _isSwitching;

        private void Awake()
        {
            for (int i = 0; i < slides.Count; i++)
                slides[i]?.gameObject.SetActive(i == startIndex);

            _current = Mathf.Clamp(startIndex, 0, Mathf.Max(0, slides.Count - 1));
        }

        private void Start()
        {
            if (slides.Count > 0)
                slides[_current].InstantShow();

            ApplyButtonsImmediate(_current);
            SetIndicatorsImmediate(_current);
            // if (SceneManager.GetActiveScene().name!=sceneName) 
            //     buttonText.text = "CLOSE";
        }

        public void NextSlide()
        {
            if (_isSwitching || slides.Count == 0) return;
            int next = _current + 1;
            if (next >= slides.Count) return;
            Switch(_current, next, forward: true);
        }

        public void PrevSlide()
        {
            if (_isSwitching || slides.Count == 0) return;
            int prev = _current - 1;
            if (prev < 0) return;
            Switch(_current, prev, forward: false);
        }

        private void Switch(int from, int to, bool forward)
        {
            _isSwitching = true;
            
            OnSlideChanged?.Invoke(to, slides.Count);

            if (leftButton != null) leftButton.interactable = false;
            if (rightButton != null) rightButton.interactable = false;
            
            SetIndicatorsImmediate(to);
            var arrowsSeq = AnimateButtonsForTarget(to);

            arrowsSeq.OnComplete(() =>
            {
                var fromSlide = slides[from];
                var toSlide   = slides[to];

                toSlide.gameObject.SetActive(true);
                toSlide.PrepareHidden(fromRight: forward);
                toSlide.PlayEnter(fromRight: forward);

                fromSlide.PlayExit(toLeft: forward, onComplete: () =>
                {
                    fromSlide.gameObject.SetActive(false);
                    _current = to;
                    _isSwitching = false;

                    bool isFirst = to <= 0;
                    bool isLast = to >= Mathf.Max(0, slides.Count - 1);
                    
                    if (leftButton != null && !isFirst) leftButton.interactable = true;
                    if (rightButton != null && !isLast) rightButton.interactable = true;
                });
            });
        }

        #region Buttons (animate only on state change)

        private Sequence AnimateButtonsForTarget(int targetIndex)
        {
            bool isFirst = targetIndex <= 0;
            bool isLast  = targetIndex >= Mathf.Max(0, slides.Count - 1);

            bool shouldShowLeft  = !isFirst;
            bool shouldShowRight = !isLast;

            var seq = DOTween.Sequence();

            if (leftButton != null)
            {
                bool currentlyShown = leftButton.gameObject.activeSelf;
                if (currentlyShown != shouldShowLeft)
                    seq.Join(AnimateButton(leftButton, show: shouldShowLeft));
            }

            if (rightButton != null)
            {
                bool currentlyShown = rightButton.gameObject.activeSelf;
                if (currentlyShown != shouldShowRight)
                    seq.Join(AnimateButton(rightButton, show: shouldShowRight));
            }
            return seq;
        }

        private Tween AnimateButton(Button btn, bool show)
        {
            var go = btn.gameObject;
            var cg = EnsureCanvasGroup(go);
            var tr = btn.transform as RectTransform;

            if (show)
            {
                btn.interactable = false;
                if (!go.activeSelf) go.SetActive(true);
                tr.localScale = Vector3.one * buttonScaleHidden;
                cg.alpha = 0f;

                return DOTween.Sequence()
                    .Join(cg.DOFade(1f, buttonFadeDuration).SetEase(Ease.OutSine))
                    .Join(tr.DOScale(1f, buttonFadeDuration).SetEase(Ease.OutBack, overshoot: 1.2f))
                    .OnComplete(() =>
                    {
                        cg.alpha = 1f;
                        tr.localScale = Vector3.one;
                    });
            }
            else
            {
                btn.interactable = false;
                
                return DOTween.Sequence()
                    .Join(cg.DOFade(0f, buttonFadeDuration).SetEase(Ease.InSine))
                    .Join(tr.DOScale(buttonScaleHidden, buttonFadeDuration).SetEase(Ease.InSine))
                    .OnComplete(() =>
                    {
                        cg.alpha = 0f;
                        go.SetActive(false);
                        btn.interactable = false;
                    });
            }
        }

        private void ApplyButtonsImmediate(int index)
        {
            bool isFirst = index <= 0;
            bool isLast  = index >= Mathf.Max(0, slides.Count - 1);

            if (leftButton != null)
            {
                var cg = EnsureCanvasGroup(leftButton.gameObject);
                bool show = !isFirst;
                leftButton.gameObject.SetActive(show);
                cg.alpha = show ? 1f : 0f;
                (leftButton.transform as RectTransform).localScale = Vector3.one;
                leftButton.interactable = show;
            }

            if (rightButton != null)
            {
                var cg = EnsureCanvasGroup(rightButton.gameObject);
                bool show = !isLast;
                rightButton.gameObject.SetActive(show);
                cg.alpha = show ? 1f : 0f;
                (rightButton.transform as RectTransform).localScale = Vector3.one;
                rightButton.interactable = show;
            }
        }

        #endregion

        #region Indicators (no animation)

        private void SetIndicatorsImmediate(int activeIndex)
        {
            if (slideNumbers == null || slideNumbers.Count == 0) return;

            for (int i = 0; i < slideNumbers.Count; i++)
            {
                var img = slideNumbers[i];
                if (img == null) continue;

                if (onSprite != null && offSprite != null)
                    img.sprite = (i == activeIndex) ? onSprite : offSprite;
            }
        }

        #endregion

        #region Utils

        private CanvasGroup EnsureCanvasGroup(GameObject go)
        {
            var cg = go.GetComponent<CanvasGroup>();
            if (cg == null) cg = go.AddComponent<CanvasGroup>();
            return cg;
        }

        #endregion
    }
}
