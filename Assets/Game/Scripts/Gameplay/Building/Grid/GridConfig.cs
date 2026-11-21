using UnityEngine;

namespace Game.Scripts.Gameplay.Building.Grid
{
    [CreateAssetMenu(fileName = "GridConfig", menuName = "Game/Grid Config")]
    public class GridConfig : ScriptableObject
    {
        [field: SerializeField] public int TotalSlots { get; private set; } = 11;
        [field: SerializeField] public int TotalLevels { get; private set; } = 4;

        [field: SerializeField] public float SlotCompression { get; private set; } = 0.95f;

        [field: SerializeField] public float[] LevelYPositions { get; private set; } =
            { -1.642f, 2.54f, 6.717f, 10.873f };

        private float _minX;
        private float _maxX;
        private float _slotWidth;
        private float _totalWidth;

        public float SlotWidth => _slotWidth;
        public float MinX => _minX;
        public float MaxX => _maxX;
        public float TotalWidth => _totalWidth;

        public void InitializeFromBounds(Bounds bounds)
        {
            _minX = bounds.min.x;
            _maxX = bounds.max.x;
            _totalWidth = bounds.size.x;
            _slotWidth = _totalWidth / TotalSlots * SlotCompression;
        }

        public float GetGridStartX()
        {
            float totalGridWidth = TotalSlots * _slotWidth;
            return _minX + (_totalWidth - totalGridWidth) * 0.5f;
        }

        public float GetBuildingCenterX(int slotIndex, int buildingSlotWidth)
        {
            float gridStartX = GetGridStartX();
            float left = gridStartX + slotIndex * _slotWidth;
            return left + buildingSlotWidth * _slotWidth * 0.5f;
        }

        public float GetLevelWorldY(int level)
        {
            return LevelYPositions[Mathf.Clamp(level, 0, TotalLevels - 1)];
        }
    }
}
