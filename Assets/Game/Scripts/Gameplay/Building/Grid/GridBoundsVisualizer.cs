using UnityEngine;

namespace Game.Scripts.Gameplay.Building.Grid
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class GridBoundsVisualizer : MonoBehaviour
    {
        [SerializeField] private GridConfig _gridConfig;

        private BoxCollider2D _collider;

        public BoxCollider2D BoundsCollider => _collider;

        private void Awake()
        {
            _collider = GetComponent<BoxCollider2D>();
            _collider.isTrigger = true;
            RecalculateBounds();
        }

        private void RecalculateBounds()
        {
            float width = _gridConfig.TotalSlots * _gridConfig.SlotWidth;
            float minY = _gridConfig.LevelYPositions[0];
            float maxY = _gridConfig.LevelYPositions[_gridConfig.TotalLevels - 1];
            float height = maxY - minY + 4f;

            _collider.size = new Vector2(width, height);
        }

        private void OnDrawGizmos()
        {
            if (_gridConfig == null) return;

            Gizmos.color = Color.cyan;
            for (int level = 0; level < _gridConfig.TotalLevels; level++)
            {
                for (int slot = 0; slot < _gridConfig.TotalSlots; slot++)
                {
                    Vector2 pos = new Vector2(_gridConfig.GetBuildingCenterX(slot, 1), _gridConfig.GetLevelWorldY(level));
                    Gizmos.DrawWireCube(pos, new Vector3(_gridConfig.SlotWidth * 0.95f, 3.8f, 0f));
                }
            }
        }
    }
}
