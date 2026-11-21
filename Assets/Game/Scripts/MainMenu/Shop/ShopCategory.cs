public enum ShopCategory
{
    Blocks, 
    Backgrounds,
    PlayerBackground,
    PlayerFrame
}
public readonly struct SelectedShopItem
{
    public ShopCategory Category { get; }
    public string ItemId { get; }

    public SelectedShopItem(ShopCategory category, string itemId)
    {
        Category = category;
        ItemId = itemId;
    }
}