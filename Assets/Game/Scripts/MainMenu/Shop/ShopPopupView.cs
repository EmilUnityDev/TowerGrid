using System;
using Game.Scripts.GameRoot.Services.Popups;
using Game.Scripts.Services.Money;
using TMPro;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class ShopPopupView : MonoBehaviour, IPopup
{
    [Header("Top Panel")]
    [SerializeField] private TMP_Text moneyText;

    [Header("Tabs")]
    [SerializeField] private ShopTabsSwitcher tabsSwitcher;

    [Header("Items")]
    [SerializeField] private ShopItemView[] items;

    private MoneyService _moneyService;
    private ShopProgressService _progressService;

    private bool _initialized;

    public event Action OnClose;

    [Inject]
    public void Construct(
        MoneyService moneyService,
        ShopProgressService progressService)
    {
        _moneyService = moneyService;
        _progressService = progressService;
    }

    private void Start()
    {
        InjectDependenciesIfNeeded();
        Initialize();
    }

    public void Bind(
        IPopupData data = null)
    {
    }

    public void ClosePopup()
    {
        OnClose?.Invoke();
        Destroy(gameObject);
    }

    private void InjectDependenciesIfNeeded()
    {
        if (_moneyService != null &&
            _progressService != null)
        {
            return;
        }

        LifetimeScope lifetimeScope =
            GetComponentInParent<LifetimeScope>();

        if (lifetimeScope == null)
        {
            lifetimeScope =
                FindFirstObjectByType<
                    LifetimeScope>();
        }

        if (lifetimeScope == null)
        {
            Debug.LogError(
                "ShopPopupView: на сцене " +
                "не найден LifetimeScope.",
                this);

            return;
        }

        lifetimeScope.Container.Inject(this);
    }

    private void Initialize()
    {
        if (_initialized)
            return;

        if (_moneyService == null)
        {
            Debug.LogError(
                "ShopPopupView: MoneyService " +
                "не был внедрён через VContainer.",
                this);

            return;
        }

        if (_progressService == null)
        {
            Debug.LogError(
                "ShopPopupView: ShopProgressService " +
                "не был внедрён через VContainer.",
                this);

            return;
        }

        _initialized = true;

        Subscribe();
        InitializeItems();
        EnsureDefaultSelections();
        RefreshAll();
    }

    private void Subscribe()
    {
        _moneyService.OnMoneyChanged +=
            HandleMoneyChanged;

        _progressService.OnItemPurchased +=
            HandleItemPurchased;

        _progressService.OnItemSelected +=
            HandleItemSelected;
    }

    private void InitializeItems()
    {
        if (items == null)
            return;

        foreach (ShopItemView item in items)
        {
            if (item == null)
                continue;

            item.Initialize(
                this,
                _moneyService,
                _progressService);
        }
    }

    public void TryPurchaseItem(
        ShopItemView item)
    {
        if (item == null)
            return;

        if (item.IsPurchased)
            return;

        bool purchaseSuccessful =
            _moneyService.SpendMoney(
                item.Price);

        if (!purchaseSuccessful)
        {
            item.Refresh();
            return;
        }

        _progressService.Purchase(
            item.Category,
            item.ItemId);

        _progressService.SelectItem(
            item.Category,
            item.ItemId);
    }

    public void SelectItem(
        ShopItemView item)
    {
        if (item == null)
            return;

        if (!item.IsPurchased)
            return;

        if (_progressService.IsSelected(
                item.Category,
                item.ItemId))
        {
            return;
        }

        _progressService.SelectItem(
            item.Category,
            item.ItemId);
    }

    private void EnsureDefaultSelections()
    {
        EnsureCategoryHasSelection(
            ShopCategory.Blocks);

        EnsureCategoryHasSelection(
            ShopCategory.Backgrounds);

        EnsureCategoryHasSelection(
            ShopCategory.PlayerBackground);

        EnsureCategoryHasSelection(
            ShopCategory.PlayerFrame);
    }

    private void EnsureCategoryHasSelection(
        ShopCategory category)
    {
        string selectedItemId =
            _progressService.GetSelectedItemId(
                category);

        if (HasValidSelectedItem(
                category,
                selectedItemId))
        {
            return;
        }

        ShopItemView firstPurchasedItem =
            FindFirstPurchasedItem(
                category);

        if (firstPurchasedItem == null)
        {
            Debug.LogWarning(
                $"ShopPopupView: в категории {category} " +
                "нет доступного предмета по умолчанию.",
                this);

            return;
        }

        _progressService.SelectItem(
            category,
            firstPurchasedItem.ItemId);
    }

    private bool HasValidSelectedItem(
        ShopCategory category,
        string itemId)
    {
        if (string.IsNullOrWhiteSpace(
                itemId))
        {
            return false;
        }

        ShopItemView item =
            FindItem(
                category,
                itemId);

        return item != null &&
               item.IsPurchased;
    }

    private ShopItemView FindFirstPurchasedItem(
        ShopCategory category)
    {
        if (items == null)
            return null;

        foreach (ShopItemView item in items)
        {
            if (item == null)
                continue;

            if (item.Category != category)
                continue;

            if (!item.IsPurchased)
                continue;

            return item;
        }

        return null;
    }

    private ShopItemView FindItem(
        ShopCategory category,
        string itemId)
    {
        if (items == null ||
            string.IsNullOrWhiteSpace(itemId))
        {
            return null;
        }

        foreach (ShopItemView item in items)
        {
            if (item == null)
                continue;

            if (item.Category != category)
                continue;

            if (item.ItemId != itemId)
                continue;

            return item;
        }

        return null;
    }

    private void HandleMoneyChanged(
        int currentMoney)
    {
        RefreshMoney();
        RefreshAllItems();
    }

    private void HandleItemPurchased(
        ShopCategory category,
        string itemId)
    {
        RefreshCategory(
            category);
    }

    private void HandleItemSelected(
        SelectedShopItem selectedItem)
    {
        RefreshCategory(
            selectedItem.Category);
    }

    private void RefreshAll()
    {
        RefreshMoney();
        RefreshAllItems();
    }

    private void RefreshMoney()
    {
        if (moneyText == null ||
            _moneyService == null)
        {
            return;
        }

        moneyText.text =
            _moneyService.Money.ToString("N0");
    }

    private void RefreshAllItems()
    {
        if (items == null)
            return;

        foreach (ShopItemView item in items)
        {
            if (item != null)
            {
                item.Refresh();
            }
        }
    }

    private void RefreshCategory(
        ShopCategory category)
    {
        if (items == null)
            return;

        foreach (ShopItemView item in items)
        {
            if (item == null)
                continue;

            if (item.Category != category)
                continue;

            item.Refresh();
        }
    }

    private void OnDestroy()
    {
        if (_moneyService != null)
        {
            _moneyService.OnMoneyChanged -=
                HandleMoneyChanged;
        }

        if (_progressService != null)
        {
            _progressService.OnItemPurchased -=
                HandleItemPurchased;

            _progressService.OnItemSelected -=
                HandleItemSelected;
        }
    }
}