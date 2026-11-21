using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public enum ShopTab
{
    Blocks,
    Backgrounds,
    Personal
}
public class ShopTabsSwitcher : MonoBehaviour
{
    [Header("Tab Buttons")]
    [SerializeField] private Button blocksButton;
    [SerializeField] private Button backgroundsButton;
    [SerializeField] private Button personalButton;

    [Header("Button Images")]
    [SerializeField] private Image blocksButtonImage;
    [SerializeField] private Image backgroundsButtonImage;
    [SerializeField] private Image personalButtonImage;

    [Header("Button Sprites")]
    [SerializeField] private Sprite activeButtonSprite;
    [SerializeField] private Sprite inactiveButtonSprite;

    [Header("Shop Panels")]
    [SerializeField] private CanvasGroup blocksPanel;
    [SerializeField] private CanvasGroup backgroundsPanel;
    [SerializeField] private CanvasGroup personalPanel;

    [Header("Animation")]
    [SerializeField] private float animationDuration = 0.25f;
    [SerializeField] private float slideOffset = 50f;

    [Header("Default Panel")]
    [SerializeField] private ShopTab defaultTab = ShopTab.Blocks;

    private Tween _currentTween;

    private void Awake()
    {
        if (blocksButton != null)
            blocksButton.onClick.AddListener(OpenBlocks);

        if (backgroundsButton != null)
            backgroundsButton.onClick.AddListener(OpenBackgrounds);

        if (personalButton != null)
            personalButton.onClick.AddListener(OpenPersonal);

        ShowImmediately(defaultTab);
    }

    private void OpenBlocks()
    {
        OpenTab(ShopTab.Blocks);
    }

    private void OpenBackgrounds()
    {
        OpenTab(ShopTab.Backgrounds);
    }

    private void OpenPersonal()
    {
        OpenTab(ShopTab.Personal);
    }

    private void OpenTab(ShopTab tab)
    {
        CanvasGroup target = GetPanel(tab);

        if (target == null)
            return;

        CanvasGroup current = GetActivePanel();

        UpdateButtonSprites(tab);

        if (current == target)
            return;

        _currentTween?.Kill();

        Sequence sequence = DOTween.Sequence();

        if (current != null)
        {
            RectTransform currentRect =
                current.GetComponent<RectTransform>();

            current.interactable = false;
            current.blocksRaycasts = false;

            sequence.Join(
                current.DOFade(
                    0f,
                    animationDuration));

            sequence.Join(
                currentRect.DOAnchorPosX(
                    -slideOffset,
                    animationDuration));

            sequence.AppendCallback(() =>
            {
                current.gameObject.SetActive(false);
            });
        }

        target.gameObject.SetActive(true);
        target.interactable = false;
        target.blocksRaycasts = false;
        target.alpha = 0f;

        RectTransform targetRect =
            target.GetComponent<RectTransform>();

        targetRect.anchoredPosition =
            new Vector2(
                slideOffset,
                targetRect.anchoredPosition.y);

        sequence.Append(
            target.DOFade(
                1f,
                animationDuration));

        sequence.Join(
            targetRect.DOAnchorPosX(
                0f,
                animationDuration));

        sequence.AppendCallback(() =>
        {
            target.interactable = true;
            target.blocksRaycasts = true;
        });

        _currentTween = sequence;
    }

    private void ShowImmediately(ShopTab tab)
    {
        DisablePanel(blocksPanel);
        DisablePanel(backgroundsPanel);
        DisablePanel(personalPanel);

        CanvasGroup panel = GetPanel(tab);

        if (panel == null)
            return;

        panel.gameObject.SetActive(true);
        panel.alpha = 1f;
        panel.interactable = true;
        panel.blocksRaycasts = true;

        RectTransform panelRect =
            panel.GetComponent<RectTransform>();

        panelRect.anchoredPosition =
            new Vector2(
                0f,
                panelRect.anchoredPosition.y);

        UpdateButtonSprites(tab);
    }

    private void DisablePanel(CanvasGroup panel)
    {
        if (panel == null)
            return;

        panel.gameObject.SetActive(false);
        panel.alpha = 0f;
        panel.interactable = false;
        panel.blocksRaycasts = false;
    }

    private CanvasGroup GetPanel(ShopTab tab)
    {
        return tab switch
        {
            ShopTab.Blocks => blocksPanel,
            ShopTab.Backgrounds => backgroundsPanel,
            ShopTab.Personal => personalPanel,
            _ => blocksPanel
        };
    }

    private CanvasGroup GetActivePanel()
    {
        if (blocksPanel != null &&
            blocksPanel.gameObject.activeSelf)
        {
            return blocksPanel;
        }

        if (backgroundsPanel != null &&
            backgroundsPanel.gameObject.activeSelf)
        {
            return backgroundsPanel;
        }

        if (personalPanel != null &&
            personalPanel.gameObject.activeSelf)
        {
            return personalPanel;
        }

        return null;
    }

    private void UpdateButtonSprites(ShopTab tab)
    {
        if (blocksButtonImage != null)
        {
            blocksButtonImage.sprite =
                tab == ShopTab.Blocks
                    ? activeButtonSprite
                    : inactiveButtonSprite;
        }

        if (backgroundsButtonImage != null)
        {
            backgroundsButtonImage.sprite =
                tab == ShopTab.Backgrounds
                    ? activeButtonSprite
                    : inactiveButtonSprite;
        }

        if (personalButtonImage != null)
        {
            personalButtonImage.sprite =
                tab == ShopTab.Personal
                    ? activeButtonSprite
                    : inactiveButtonSprite;
        }
    }

    private void OnDestroy()
    {
        _currentTween?.Kill();

        if (blocksButton != null)
            blocksButton.onClick.RemoveListener(OpenBlocks);

        if (backgroundsButton != null)
            backgroundsButton.onClick.RemoveListener(OpenBackgrounds);

        if (personalButton != null)
            personalButton.onClick.RemoveListener(OpenPersonal);
    }
}