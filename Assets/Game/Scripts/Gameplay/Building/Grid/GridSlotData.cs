using Game.Scripts.Gameplay.Building.Data;

namespace Game.Scripts.Gameplay.Building.Grid
{
    public readonly struct GridSlotData
    {
        public int SlotIndex { get; }
        public int Level { get; }
        public int Width { get; }
        public BuildingData BuildingData { get; }

        public GridSlotData(int slotIndex, int level, int width, BuildingData buildingData)
        {
            SlotIndex = slotIndex;
            Level = level;
            Width = width;
            BuildingData = buildingData;
        }
    }
}
