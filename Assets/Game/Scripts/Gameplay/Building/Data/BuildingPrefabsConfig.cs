using System;
using UnityEngine;

namespace Game.Scripts.Gameplay.Building.Data
{
    [CreateAssetMenu(fileName = "BuildingPrefabsConfig", menuName = "Game/Building Prefabs Config")]
    public class BuildingPrefabsConfig : ScriptableObject
    {
        [SerializeField] private GameObject _smallPrefab;
        [SerializeField] private GameObject _mediumPrefab;
        [SerializeField] private GameObject _largePrefab;

        public GameObject GetPrefab(BuildingSize size)
        {
            return size switch
            {
                BuildingSize.Small => _smallPrefab,
                BuildingSize.Medium => _mediumPrefab,
                BuildingSize.Large => _largePrefab,
                _ => throw new ArgumentOutOfRangeException(nameof(size), size, null)
            };
        }
    }
}
