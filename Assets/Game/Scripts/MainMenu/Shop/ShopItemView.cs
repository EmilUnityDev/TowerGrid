using DG.Tweening;
using Game.Scripts.Services.Money;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemView : MonoBehaviour
{
    [Header("Item")]
    [SerializeField] private string itemId;
    [SerializeField] private ShopCategory category;
    [SerializeField] private int price = 1000;
    [SerializeField] private bool unlockedByDefault;

    [Header("Item Content")]
    [SerializeField] private Sprite itemSprite;

    [Header("Buy Button")]
    [SerializeField] private Button buyButton;
    [SerializeField] private Image buyButtonImage;
    [SerializeField] private TMP_Text priceText;

    [Header("Buy Button Sprites")]
    [SerializeField] private Sprite notEnoughMoneySprite;
    [SerializeField] private Sprite enoughMoneySprite;

    [Header("Selection")]
    [SerializeField] private GameObject selectionRoot;
    [SerializeField] private Button selectionButton;
    [SerializeField] private Image selectionButtonImage;

    [Header("Selection Sprites")]
    [SerializeField] private Sprite selectionOffSprite;
    [SerializeField] private Sprite selectionOnSprite;

    [Header("Selection Handle")]
    [SerializeField] private RectTransform selectionHandle;
    private float handleMoveDistance = 100f;
    [SerializeField] private float handleMoveDuration = 0.25f;
    [SerializeField] private Ease handleMoveEase = Ease.OutBack;

    private ShopPopupView _popup;
    private MoneyService _moneyService;
    private ShopProgressService _progressService;

    private Vector2 _handleOnPosition;
    private bool _handleStateInitialized;

    public string ItemId => itemId;
    public ShopCategory Category => category;
    public int Price => price;
    public bool UnlockedByDefault => unlockedByDefault;
    public Sprite ItemSprite => itemSprite;

    public bool IsPurchased =>
        _progressService != null &&
        _progressService.IsPurchased(
            category,
            itemId,
            unlockedByDefault);

    private void Awake()
    {
        if (selectionHandle == null &&
            selectionButton != null)
        {
            selectionHandle =
                selectionButton.GetComponent<
                    RectTransform>();
        }

        if (selectionButton != null)
        {
            selectionButton.interactable = true;
        }

        if (selectionHandle != null)
        {
            _handleOnPosition =
                selectionHandle.anchoredPosition;
        }
        else
        {
            Debug.LogWarning(
                $"[ShopItemView] Selection Handle не найден: {name}",
                this);
        }
    }

    public void Initialize(
        ShopPopupView popup,
        MoneyService moneyService,
        ShopProgressService progressService)
    {
        _popup = popup;
        _moneyService = moneyService;
        _progressService = progressService;

        RemoveListeners();

        if (buyButton != null)
        {
            buyButton.onClick.AddListener(
                TryPurchase);
        }

        if (selectionButton != null)
        {
            selectionButton.onClick.AddListener(
                TrySelect);
        }

        if (priceText != null)
        {
            priceText.text =
                price.ToString("N0");
        }

        Refresh();
    }

    public void Refresh()
    {
        if (_progressService == null)
            return;

        bool isPurchased =
            IsPurchased;

        bool isSelected =
            _progressService.IsSelected(
                category,
                itemId);

        SetPurchaseState(
            isPurchased);

        if (isPurchased)
        {
            RefreshSelectionButton(
                isSelected);
        }
        else
        {
            RefreshBuyButton();
        }
    }

    private void SetPurchaseState(
        bool isPurchased)
    {
        if (buyButton != null)
        {
            buyButton.gameObject.SetActive(
                !isPurchased);
        }

        if (selectionRoot != null)
        {
            selectionRoot.SetActive(
                isPurchased);
        }
    }

    private void RefreshBuyButton()
    {
        if (buyButton == null ||
            _moneyService == null)
        {
            return;
        }

        bool hasEnoughMoney =
            _moneyService.CanAfford(
                price);

        buyButton.interactable =
            hasEnoughMoney;

        if (buyButtonImage != null)
        {
            buyButtonImage.sprite =
                hasEnoughMoney
                    ? enoughMoneySprite
                    : notEnoughMoneySprite;
        }
    }

    private void RefreshSelectionButton(
        bool isSelected)
    {
        if (selectionButtonImage != null)
        {
            selectionButtonImage.sprite =
                isSelected
                    ? selectionOnSprite
                    : selectionOffSprite;
        }

        if (selectionButton != null)
        {
            selectionButton.interactable = true;
        }

        UpdateHandlePosition(
            isSelected);
    }

    private void UpdateHandlePosition(
        bool isSelected)
    {
        if (selectionHandle == null)
            return;

        float moveDistance =
            Mathf.Abs(
                handleMoveDistance);
        float targetX =
            isSelected
                ? _handleOnPosition.x
                : _handleOnPosition.x -
                  moveDistance;

        selectionHandle.DOKill();
        if (!_handleStateInitialized)
        {
            Vector2 position =
                selectionHandle.anchoredPosition;

            position.x = targetX;

            selectionHandle.anchoredPosition =
                position;

            _handleStateInitialized = true;
            return;
        }

        selectionHandle
            .DOAnchorPosX(
                targetX,
                handleMoveDuration)
            .SetEase(
                handleMoveEase)
            .SetUpdate(true);
    }

    private void TryPurchase()
    {
        _popup?.TryPurchaseItem(this);
    }

    private void TrySelect()
    {
        if (_progressService == null)
            return;

        bool isAlreadySelected =
            _progressService.IsSelected(
                category,
                itemId);

        if (isAlreadySelected)
            return;

        _popup?.SelectItem(this);
    }

    private void RemoveListeners()
    {
        if (buyButton != null)
        {
            buyButton.onClick.RemoveListener(
                TryPurchase);
        }

        if (selectionButton != null)
        {
            selectionButton.onClick.RemoveListener(
                TrySelect);
        }
    }

    private void OnDestroy()
    {
        if (selectionHandle != null)
        {
            selectionHandle.DOKill();
        }

        RemoveListeners();
    }
}