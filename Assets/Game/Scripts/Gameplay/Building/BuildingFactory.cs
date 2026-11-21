using Game.Scripts.Gameplay.Building.Data;
using UnityEngine;

namespace Game.Scripts.Gameplay.Building
{
    public class BuildingFactory : IBuildingFactory
    {
        private readonly BuildingPrefabsConfig _prefabsConfig;

        public BuildingFactory(BuildingPrefabsConfig prefabsConfig)
        {
            _prefabsConfig = prefabsConfig;
        }

        public GameObject Create(BuildingData data, Transform parent)
        {
            var prefab = _prefabsConfig.GetPrefab(data.Size);
            var instance = parent != null
                ? Object.Instantiate(prefab, parent)
                : Object.Instantiate(prefab);
            instance.name = $"Building_{data.Type}_{data.Size}";

            var sr = instance.GetComponent<SpriteRenderer>();
            sr.sprite = data.Sprite;

            var collider = instance.GetComponent<BoxCollider2D>();
            if (collider == null)
                collider = instance.AddComponent<BoxCollider2D>();

            collider.isTrigger = true;
            collider.size = sr.sprite.bounds.size;
            collider.offset = Vector2.zero;

            var building = instance.GetComponent<BuildingInstance>();
            building.Initialize(data);

            return instance;
        }
    }
}
