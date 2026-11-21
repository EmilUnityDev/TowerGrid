using Game.Scripts.Gameplay.Building.Data;
using UnityEngine;

namespace Game.Scripts.Gameplay.Building
{
    public class BuildingInstance : MonoBehaviour
    {
        public BuildingData Data { get; private set; }
        public bool IsPlaced { get; private set; }

        public void Initialize(BuildingData data)
        {
            Data = data;
        }

        public void MarkPlaced()
        {
            IsPlaced = true;
        }
    }
}
