using System;
using System.Collections.Generic;
using DG.Tweening;
using Game.Scripts.GameRoot;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Scripts.Game.SlideshowGamePopup
{
    public class SlideElement : MonoBehaviour
    {
        [SerializeField] private Image mainImage;

        [Header("Anim Elements")]
        [SerializeField] private List<RectTransform> animatedObjects = new List<RectTransform>();

        [Header("Settings")]
        [SerializeField] private float moveOffset = 140f;
        [SerializeField] private float enterDuration = 0.45f;
        [SerializeField] private float exitDuration  = 0.40f;
        [SerializeField] private float stagger       = 0.06f;
        [SerializeField] private Ease enterEase      = Ease.OutCubic;
        [SerializeField] private Ease exitEase       = Ease.InCubic;

        [SerializeField] private bool rootFadeAlways = true;
        public bool IsLast = false;
        
        private readonly List<Vector2> _basePos = new List<Vector2>();
        private readonly List<Tween> _running = new List<Tween>();
        private readonly List<CanvasGroup> _groups = new List<CanvasGroup>();
        private CanvasGroup _rootCg;
        private bool _cached;

        private void Awake()
        {
            EnsureCached();
            if (mainImage != null) mainImage.enabled = true;
            if (SceneManager.GetActiveScene().name == Scenes.MAIN_MENU && IsLast)
            {
                animatedObjects.RemoveAt(3);
            }
        }

        private void OnDisable()
        {
            KillRunning();
        }

        private void EnsureCached()
        {
            if (_cached) return;

            _rootCg = GetComponent<CanvasGroup>();
            if (_rootCg == null) _rootCg = gameObject.AddComponent<CanvasGroup>();
            _rootCg.alpha = 1f;

            _basePos.Clear();
            _groups.Clear();

            foreach (var rt in animatedObjects)
            {
                if (rt == null)
                {
                    _basePos.Add(Vector2.zero);
                    _groups.Add(null);
                    continue;
                }
                _basePos.Add(rt.anchoredPosition);
                var cg = rt.GetComponent<CanvasGroup>();
                if (cg == null) cg = rt.gameObject.AddComponent<CanvasGroup>();
                _groups.Add(cg);
            }
            _cached = true;
        }

        private void KillRunning()
        {
            foreach (var t in _running) t?.Kill();
            _running.Clear();
        }

        private bool HasValidAnimItems()
        {
            for (int i = 0; i < animatedObjects.Count; i++)
                if (animatedObjects[i] != null && _groups[i] != null)
                    return true;
            return false;
        }

        public void InstantShow()
        {
            EnsureCached();
            KillRunning();

            if (HasValidAnimItems())
            {
                for (int i = 0; i < animatedObjects.Count; i++)
                {
                    var rt = animatedObjects[i];
                    var cg = _groups[i];
                    if (rt == null || cg == null) continue;
                    rt.anchoredPosition = _basePos[i];
                    cg.alpha = 1f;
                }
            }
            _rootCg.alpha = 1f;
            if (mainImage != null) mainImage.enabled = true;
        }

        public void PrepareHidden(bool fromRight)
        {
            EnsureCached();
            KillRunning();

            float dir = fromRight ? 1f : -1f;

            if (HasValidAnimItems())
            {
                for (int i = 0; i < animatedObjects.Count; i++)
                {
                    var rt = animatedObjects[i];
                    var cg = _groups[i];
                    if (rt == null || cg == null) continue;

                    rt.anchoredPosition = _basePos[i] + new Vector2(moveOffset * dir, 0f);
                    cg.alpha = 0f;
                }
            }
            _rootCg.alpha = 0f;

            if (mainImage != null) mainImage.enabled = true;
        }

        public void PlayEnter(bool fromRight)
        {
            EnsureCached();
            KillRunning();

            float dir = fromRight ? 1f : -1f;

            if (HasValidAnimItems())
            {
                for (int i = 0; i < animatedObjects.Count; i++)
                {
                    var rt = animatedObjects[i];
                    var cg = _groups[i];
                    if (rt == null || cg == null) continue;

                    var fromPos = _basePos[i] + new Vector2(moveOffset * dir, 0f);
                    rt.anchoredPosition = _basePos[i];
                    cg.alpha = 1f;

                    var move = rt.DOAnchorPos(_basePos[i], enterDuration)
                                 .From(fromPos)
                                 .SetEase(enterEase)
                                 .SetDelay(i * stagger)
                                 .SetUpdate(true);

                    var fade = cg.DOFade(1f, enterDuration * 0.95f)
                                 .From(0f)
                                 .SetEase(Ease.OutSine)
                                 .SetDelay(i * stagger)
                                 .SetUpdate(true);

                    _running.Add(move);
                    _running.Add(fade);
                }
            }
            var rootFade = _rootCg.DOFade(1f, enterDuration)
                                   .From(rootFadeAlways ? 0f : _rootCg.alpha)
                                   .SetEase(enterEase)
                                   .SetUpdate(true);
            _running.Add(rootFade);
        }

        public void PlayExit(bool toLeft, Action onComplete = null)
        {
            EnsureCached();
            KillRunning();

            float dir = toLeft ? -1f : 1f;
            float lastDelay = 0f;
            Tween tail = null;

            if (HasValidAnimItems())
            {
                for (int i = 0; i < animatedObjects.Count; i++)
                {
                    var rt = animatedObjects[i];
                    var cg = _groups[i];
                    if (rt == null || cg == null) continue;

                    var toPos = _basePos[i] + new Vector2(moveOffset * dir, 0f);
                    float d = i * stagger;

                    rt.anchoredPosition = _basePos[i];
                    cg.alpha = 1f;

                    var move = rt.DOAnchorPos(toPos, exitDuration)
                                 .SetEase(exitEase)
                                 .SetDelay(d)
                                 .SetUpdate(true);

                    var fade = cg.DOFade(0f, exitDuration * 0.9f)
                                 .SetEase(Ease.InSine)
                                 .SetDelay(d)
                                 .SetUpdate(true);

                    _running.Add(move);
                    _running.Add(fade);

                    if (d >= lastDelay) { lastDelay = d; tail = move; }
                }
            }

            var rootOut = _rootCg.DOFade(0f, exitDuration)
                                 .SetEase(exitEase)
                                 .SetUpdate(true);
            _running.Add(rootOut);
            if (tail == null || lastDelay < 0f) tail = rootOut;
            tail.OnComplete(() => onComplete?.Invoke());
        }
    }
}
