using UnityEngine;

public class ShopCategoryController : MonoBehaviour
{
    [Header("Shop Items")]
    [SerializeField] private ShopItemView[] items;

    private ShopProgressService _progressService;

    public void Initialize(
        ShopProgressService progressService)
    {
        _progressService = progressService;

        if (_progressService == null)
        {
            Debug.LogError(
                "ShopCategoryController: " +
                "ShopProgressService is null.",
                this);

            return;
        }

        EnsureDefaultSelections();
        RefreshAllItems();
    }

    public void SelectItem(
        ShopItemView selectedItem)
    {
        if (_progressService == null)
            return;

        if (selectedItem == null)
            return;

        if (!selectedItem.IsPurchased)
            return;

        _progressService.SelectItem(
            selectedItem.Category,
            selectedItem.ItemId);

        RefreshCategory(
            selectedItem.Category);
    }

    public void RefreshAllItems()
    {
        foreach (ShopItemView item in items)
        {
            if (item != null)
                item.Refresh();
        }
    }

    public void RefreshCategory(
        ShopCategory category)
    {
        foreach (ShopItemView item in items)
        {
            if (item == null)
                continue;

            if (item.Category != category)
                continue;

            item.Refresh();
        }
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
            GetFirstPurchasedItem(category);

        if (firstPurchasedItem == null)
        {
            Debug.LogWarning(
                $"ShopCategoryController: " +
                $"в категории {category} " +
                $"нет купленного предмета.",
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
        if (string.IsNullOrWhiteSpace(itemId))
            return false;

        foreach (ShopItemView item in items)
        {
            if (item == null)
                continue;

            if (item.Category != category)
                continue;

            if (item.ItemId != itemId)
                continue;

            return item.IsPurchased;
        }

        return false;
    }

    private ShopItemView GetFirstPurchasedItem(
        ShopCategory category)
    {
        foreach (ShopItemView item in items)
        {
            if (item == null)
                continue;

            if (item.Category != category)
                continue;

            if (item.IsPurchased)
                return item;
        }

        return null;
    }
}