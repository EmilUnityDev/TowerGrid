using UnityEngine;

namespace Game.Scripts.Gameplay.Core.Services
{
    public class CameraControlService : ICameraControlService
    {
        private const float MinY = 0f;
        private const float MaxY = 6f;

        private readonly Camera _camera;
        private readonly float _sensitivity;

        public CameraControlService(float sensitivity = 0.01f)
        {
            _camera = Camera.main;
            _sensitivity = sensitivity;
        }

        public void ApplyDragDelta(float deltaY)
        {
            var pos = _camera.transform.position;
            pos.y = Mathf.Clamp(pos.y - deltaY * _sensitivity, MinY, MaxY);
            _camera.transform.position = pos;
        }
    }
}
