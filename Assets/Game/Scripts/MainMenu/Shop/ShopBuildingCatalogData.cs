using System;
using Game.Scripts.Gameplay.Building.Data;
using UnityEngine;

[Serializable]
public class ShopBuildingCatalogData
{
    [SerializeField] private string itemId;
    [SerializeField] private BuildingCatalog buildingCatalog;
    [SerializeField] private bool isDefault;

    public string ItemId => itemId;
    public BuildingCatalog BuildingCatalog => buildingCatalog;
    public bool IsDefault => isDefault;
}