using UnityEngine;

public class ShopAppearanceService
{
    private readonly ShopProgressService _progressService;
    private readonly ShopItemsDatabase _database;

    public ShopAppearanceService(
        ShopProgressService progressService,
        ShopItemsDatabase database)
    {
        _progressService = progressService;
        _database = database;
    }

    public Sprite GetSelectedBlocksSprite()
    {
        return GetSelectedSprite(
            ShopCategory.Blocks);
    }

    public Sprite GetSelectedBackgroundSprite()
    {
        return GetSelectedSprite(
            ShopCategory.Backgrounds);
    }

    public Sprite GetSelectedPlayerBackgroundSprite()
    {
        return GetSelectedSprite(
            ShopCategory.PlayerBackground);
    }

    public string GetSelectedBackgroundItemId() {
        if (_progressService == null)
            return string.Empty;

        return _progressService.GetSelectedItemId(ShopCategory.Backgrounds);
    }

    public Sprite GetSelectedPlayerFrameSprite()
    {
        return GetSelectedSprite(
            ShopCategory.PlayerFrame);
    }

    public Sprite GetSelectedSprite(
        ShopCategory category)
    {
        string itemId =
            _progressService.GetSelectedItemId(category);

        if (string.IsNullOrWhiteSpace(itemId))
            return null;

        return _database.GetSprite(
            category,
            itemId);
    }
}