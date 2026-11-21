using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(
    fileName = "ShopItemsDatabase",
    menuName = "Shop/Items Database")]
public class ShopItemsDatabase : ScriptableObject
{
    [Header("Shop Prefab")]
    [SerializeField] private GameObject shopPrefab;

    [Header("Collected Items")]
    [SerializeField] private ShopItemData[] items;

    public Sprite GetSprite(
        ShopCategory category,
        string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return null;

        foreach (ShopItemData item in items)
        {
            if (item == null)
                continue;

            if (item.Category != category)
                continue;

            if (item.ItemId != itemId)
                continue;

            return item.Sprite;
        }

        return null;
    }

#if UNITY_EDITOR
    [ContextMenu("Collect Items From Shop Prefab")]
    private void CollectItemsFromShopPrefab()
    {
        if (shopPrefab == null)
        {
            Debug.LogError(
                "ShopItemsDatabase: Shop Prefab не назначен.",
                this);

            return;
        }

        ShopItemView[] shopItems =
            shopPrefab.GetComponentsInChildren<ShopItemView>(
                true);

        items = new ShopItemData[shopItems.Length];

        for (int i = 0; i < shopItems.Length; i++)
        {
            ShopItemView item = shopItems[i];

            items[i] = new ShopItemData(
                item.ItemId,
                item.Category,
                item.ItemSprite);
        }

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();

        Debug.Log(
            $"ShopItemsDatabase: собрано предметов — {items.Length}.",
            this);
    }
#endif
}