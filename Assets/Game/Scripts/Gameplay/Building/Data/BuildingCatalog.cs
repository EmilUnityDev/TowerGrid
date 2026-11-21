using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.Gameplay.Building.Data
{
    [CreateAssetMenu(fileName = "BuildingCatalog", menuName = "Game/Building Catalog")]
    public class BuildingCatalog : ScriptableObject
    {
        [SerializeField] private List<BuildingData> _buildings;

        public IReadOnlyList<BuildingData> Buildings => _buildings;

        public BuildingData GetRandom()
        {
            return _buildings[Random.Range(0, _buildings.Count)];
        }

        public BuildingData GetRandomWithSizeExcludingType(BuildingSize size, BuildingType excludeType)
        {
            var candidates = new List<BuildingData>();
            foreach (var b in _buildings)
            {
                if (b.Size == size && b.Type != excludeType)
                    candidates.Add(b);
            }

            if (candidates.Count == 0)
                return GetRandom();

            return candidates[Random.Range(0, candidates.Count)];
        }
    }
}
