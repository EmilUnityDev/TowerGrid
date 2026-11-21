using Game.Scripts.Gameplay.Core.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Scripts.Gameplay.Root.View
{
    [RequireComponent(typeof(Image))]
    public class CameraInputView : MonoBehaviour, IDragHandler
    {
        private ICameraControlService _cameraService;
        private Image _image;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        public void Initialize(ICameraControlService cameraService)
        {
            _cameraService = cameraService;
        }

        public void SetEnabled(bool enabled)
        {
            _image.raycastTarget = enabled;
        }

        public void OnDrag(PointerEventData eventData)
        {
            _cameraService?.ApplyDragDelta(eventData.delta.y);
        }
    }
}
