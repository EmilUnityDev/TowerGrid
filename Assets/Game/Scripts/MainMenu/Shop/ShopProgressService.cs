using System;
using UnityEngine;

public class ShopProgressService
{
    private const string PurchasedKeyPrefix = "SHOP_PURCHASED_";
    private const string SelectedKeyPrefix = "SHOP_SELECTED_";

    public event Action<ShopCategory, string> OnItemPurchased;
    public event Action<SelectedShopItem> OnItemSelected;

    public bool IsPurchased(
        ShopCategory category,
        string itemId,
        bool unlockedByDefault = false)
    {
        if (unlockedByDefault)
            return true;

        string key = GetPurchasedKey(category, itemId);

        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    public void Purchase(
        ShopCategory category,
        string itemId)
    {
        string key = GetPurchasedKey(category, itemId);

        if (PlayerPrefs.GetInt(key, 0) == 1)
            return;

        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();

        OnItemPurchased?.Invoke(category, itemId);
    }

    public bool IsSelected(
        ShopCategory category,
        string itemId)
    {
        return GetSelectedItemId(category) == itemId;
    }

    public string GetSelectedItemId(ShopCategory category)
    {
        return PlayerPrefs.GetString(
            GetSelectedKey(category),
            string.Empty);
    }

    public void SelectItem(
        ShopCategory category,
        string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return;

        PlayerPrefs.SetString(
            GetSelectedKey(category),
            itemId);

        PlayerPrefs.Save();

        OnItemSelected?.Invoke(
            new SelectedShopItem(category, itemId));
    }

    private string GetPurchasedKey(
        ShopCategory category,
        string itemId)
    {
        return $"{PurchasedKeyPrefix}{category}_{itemId}";
    }

    private string GetSelectedKey(ShopCategory category)
    {
        return $"{SelectedKeyPrefix}{category}";
    }
}