using System;
using UnityEngine;

[Serializable]
public class ShopItemData
{
    [SerializeField] private string itemId;
    [SerializeField] private ShopCategory category;
    [SerializeField] private Sprite sprite;

    public string ItemId => itemId;
    public ShopCategory Category => category;
    public Sprite Sprite => sprite;

    public ShopItemData(
        string itemId,
        ShopCategory category,
        Sprite sprite)
    {
        this.itemId = itemId;
        this.category = category;
        this.sprite = sprite;
    }
}