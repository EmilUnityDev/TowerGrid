using UnityEngine;

namespace Game.Scripts.Gameplay.Building.Grid
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class GridBoundsProvider : MonoBehaviour
    {
        private BoxCollider2D _collider;

        public Bounds WorldBounds
        {
            get
            {
                if (_collider == null)
                    _collider = GetComponent<BoxCollider2D>();
                return _collider.bounds;
            }
        }

        public float MinX => WorldBounds.min.x;
        public float MaxX => WorldBounds.max.x;
        public float MinY => WorldBounds.min.y;
        public float MaxY => WorldBounds.max.y;
        public float Width => WorldBounds.size.x;

        private void Awake()
        {
            _collider = GetComponent<BoxCollider2D>();
            _collider.isTrigger = true;
        }

        private void OnDrawGizmos()
        {
            var col = GetComponent<BoxCollider2D>();
            if (col == null) return;

            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
}
