using Game.Scripts.Gameplay.Building.Data;
using UnityEngine;

namespace Game.Scripts.Gameplay.Building
{
    public interface IBuildingFactory
    {
        GameObject Create(BuildingData data, Transform parent);
    }
}
