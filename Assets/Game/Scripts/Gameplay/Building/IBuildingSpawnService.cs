using Game.Scripts.Gameplay.Building.Data;
using UnityEngine;

namespace Game.Scripts.Gameplay.Building
{
    public interface IBuildingSpawnService
    {
        BuildingDragHandler SpawnAndBeginDrag(BuildingData data, Vector2 screenPosition);
    }
}
