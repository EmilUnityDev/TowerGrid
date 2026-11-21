using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HybridGridAnimator : MonoBehaviour {

    [Header("Sprites")]
    [SerializeField] private List<Sprite> sprites1Cell;
    [SerializeField] private List<Sprite> sprites2Cells;
    [SerializeField] private List<Sprite> sprites3Cells;

    [Header("Images")]
    [SerializeField] private List<Image> images1Cell;
    [SerializeField] private List<Image> images2Cells;
    [SerializeField] private List<Image> images3Cells;

    [Header("Animation Offsets & Timings")]
    [SerializeField] private float startYOffset = 1000f;
    [SerializeField] private float exitYOffset = -1000f;
    [SerializeField] private float fallDuration = 0.8f;
    [SerializeField] private float pauseDuration = 1.5f;
    [SerializeField] private float exitDuration = 0.6f;
    [SerializeField] private float cascadeDelay = 0.05f;

    [SerializeField] private float fadeInDuration = 0.4f;
    [SerializeField] private float fadeOutDuration = 0.3f;
    [SerializeField] private float fadeOutDelay = 0.2f;

    [Header("Eases")]
    [SerializeField] private Ease fallEase = Ease.OutBounce;
    [SerializeField] private Ease exitEase = Ease.InCubic;

    private class BlockRuntimeData {
        public RectTransform rectTransform;
        public CanvasGroup canvasGroup;
        public Vector2 middlePosition;
    }

    private List<BlockRuntimeData> allBlocks = new List<BlockRuntimeData>();
    private Sequence mainSeq;


    private void OnEnable() {

        DOVirtual.DelayedCall(0.01f, () => {
            PlayAnimationSequence();
        });
       
    }

    private void Awake() {
        RandomizeSprites();

        PrepareBlocksDataAndHide();
    }


    private void OnDestroy() {
        // Хорошая практика — убивать твины при уничтожении объекта
        mainSeq?.Kill();
    }

    private void RandomizeSprites() {
        AssignRandomSprites(images1Cell, sprites1Cell);
        AssignRandomSprites(images2Cells, sprites2Cells);
        AssignRandomSprites(images3Cells, sprites3Cells);
    }

    private void AssignRandomSprites(List<Image> images, List<Sprite> sprites) {
        if (sprites == null || sprites.Count == 0) return;

        foreach (var img in images) {
            if (img != null) {
                img.sprite = sprites[Random.Range(0, sprites.Count)];
            }
        }
    }

    private void PrepareBlocksDataAndHide() {
        allBlocks.Clear();

        List<Image> combinedImages = new List<Image>();
        combinedImages.AddRange(images1Cell);
        combinedImages.AddRange(images2Cells);
        combinedImages.AddRange(images3Cells);

        foreach (var img in combinedImages) {
            if (img == null) continue;

            CanvasGroup cg = img.GetComponent<CanvasGroup>();
            if (cg == null) {
                cg = img.gameObject.AddComponent<CanvasGroup>();
            }

            RectTransform rect = img.rectTransform;
            Vector2 targetPos = rect.anchoredPosition; // Запоминаем конечную точную позицию

            // МГНОВЕННО прячем объект еще до отрисовки
            rect.anchoredPosition = targetPos + new Vector2(0, startYOffset);
            cg.alpha = 0f;

            allBlocks.Add(new BlockRuntimeData {
                rectTransform = rect,
                canvasGroup = cg,
                middlePosition = targetPos
            });
        }
    }

    public void PlayAnimationSequence() {
        // Сбрасываем старую последовательность, если игралась
        mainSeq?.Kill();
        mainSeq = DOTween.Sequence();

        // 1. Появление (Вход)
        Sequence enterSeq = DOTween.Sequence();

        for (int i = 0; i < allBlocks.Count; i++) {
            var block = allBlocks[i];
            float delay = i * cascadeDelay;

            // Гарантируем сброс в стартовое положение при повторном запуске
            block.rectTransform.anchoredPosition = block.middlePosition + new Vector2(0, startYOffset);
            block.canvasGroup.alpha = 0f;

            Tween moveTween = block.rectTransform
                .DOAnchorPos(block.middlePosition, fallDuration)
                .SetEase(fallEase)
                .SetDelay(delay);

            Tween fadeTween = block.canvasGroup
                .DOFade(1f, fadeInDuration)
                .SetDelay(delay);

            enterSeq.Join(moveTween);
            enterSeq.Join(fadeTween);
        }

        mainSeq.Append(enterSeq);

        // 2. Пауза
        mainSeq.AppendInterval(pauseDuration);

        // 3. Уход (Выход)
        Sequence exitSeq = DOTween.Sequence();

        for (int i = 0; i < allBlocks.Count; i++) {
            var block = allBlocks[i];
            float delay = i * cascadeDelay;
            Vector2 targetExitPos = block.middlePosition + new Vector2(0, exitYOffset);

            Tween moveTween = block.rectTransform
                .DOAnchorPos(targetExitPos, exitDuration)
                .SetEase(exitEase)
                .SetDelay(delay);

            Tween fadeTween = block.canvasGroup
                .DOFade(0f, fadeOutDuration)
                .SetDelay(delay + fadeOutDelay);

            exitSeq.Join(moveTween);
            exitSeq.Join(fadeTween);
        }

        mainSeq.Append(exitSeq);
    }
}