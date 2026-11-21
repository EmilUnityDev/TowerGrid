using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Scripts.Gameplay.Root.View
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class DemolishCell : MonoBehaviour, IPointerDownHandler
    {
        public event Action<DemolishCell> OnCellClicked;

        private BoxCollider2D _collider;

        public BoxCollider2D Collider => _collider ??= GetComponent<BoxCollider2D>();

        public void OnPointerDown(PointerEventData eventData)
        {
            OnCellClicked?.Invoke(this);
        }
    }
}
