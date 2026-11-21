using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace Game.Scripts.GameRoot.Services.Popups
{
    public static class PopupAnimationHandler
    {
        private const string AnimPrefix = "Anim_";
        private const string FadePrefix = "Fade_";
        private const string NoAnimBGName = "NoAnimBG";

        public static void PlaySpawnAnimation(GameObject popup, Action onComplete = null)
        {
            var allChildren = GetAllChildren(popup.transform, skipNoAnimBGChildren: true)
                .ToList();

            var sequence = DOTween.Sequence();

            var noAnimBG = popup.transform
                .GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(t => t.name == NoAnimBGName);

            if (noAnimBG != null)
            {
                noAnimBG.localScale = Vector3.one * 0.4f;

                sequence.Append(noAnimBG.DOScale(1.2f, 0.3f).SetEase(Ease.OutQuad));
                sequence.Append(noAnimBG.DOScale(0.9f, 0.2f).SetEase(Ease.InOutQuad));
                sequence.Append(noAnimBG.DOScale(1.0f, 0.1f).SetEase(Ease.OutQuad));
            }

            float duration = 0.14f;
            float overlap = 0.08f;

            int index = 0;
            foreach (var child in allChildren)
            {
                if (IsScaleAnimatable(child))
                {
                    var originalScale = child.localScale;
                    child.localScale = Vector3.zero;

                    float startTime = 0.3f + index * (duration - overlap);

                    sequence.Insert(startTime,
                        child.DOScale(originalScale, duration)
                            .SetEase(Ease.OutQuad)
                    );
                    index++;
                }
                else if (IsFadeAnimatable(child))
                {
                    float startTime = 0.3f + index * (duration - overlap);

                    var cg = child.GetComponent<CanvasGroup>();
                    if (cg == null) cg = child.gameObject.AddComponent<CanvasGroup>();

                    float targetAlpha = cg.alpha;
                    cg.alpha = 0f;

                    sequence.Insert(startTime,
                        cg.DOFade(targetAlpha, duration)
                            .SetEase(Ease.OutQuad)
                    );
                    index++;
                }
            }

            sequence.OnComplete(() => onComplete?.Invoke());
            sequence.Play();
        }


        public static void PlayCloseAnimation(GameObject popup, Action onComplete = null)
        {
            var allChildren = GetAllChildren(popup.transform, skipNoAnimBGChildren: true)
                .Reverse()
                .ToList();

            var sequence = DOTween.Sequence();

            float duration = 0.16f;
            float overlap = 0.1f;

            int index = 0;
            foreach (var child in allChildren)
            {
                if (IsScaleAnimatable(child))
                {
                    float startTime = index * (duration - overlap);

                    sequence.Insert(startTime,
                        child.DOScale(Vector3.zero, duration)
                            .SetEase(Ease.InBack)
                    );
                    index++;
                }
                else if (IsFadeAnimatable(child))
                {
                    float startTime = index * (duration - overlap);

                    var cg = child.GetComponent<CanvasGroup>();
                    if (cg == null) cg = child.gameObject.AddComponent<CanvasGroup>();

                    sequence.Insert(startTime,
                        cg.DOFade(0f, duration)
                            .SetEase(Ease.InQuad)
                    );
                    index++;
                }
            }

            sequence.OnComplete(() => onComplete?.Invoke());
            sequence.Play();
        }

        private static IEnumerable<Transform> GetAllChildren(Transform parent, bool skipNoAnimBGChildren = false)
        {
            var children = new List<Transform>();
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);

                if (skipNoAnimBGChildren && child.name == NoAnimBGName)
                {
                    continue;
                }

                children.Add(child);
                children.AddRange(GetAllChildren(child, skipNoAnimBGChildren));
            }

            return children;
        }

        private static bool IsScaleAnimatable(Transform t)
        {
            return t.name.StartsWith(AnimPrefix, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsFadeAnimatable(Transform t)
        {
            return t.name.StartsWith(FadePrefix, StringComparison.OrdinalIgnoreCase);
        }
    }
}
