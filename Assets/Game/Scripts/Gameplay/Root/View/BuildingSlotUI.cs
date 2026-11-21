using System;
using Game.Scripts.Gameplay.Building;
using Game.Scripts.Gameplay.Building.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Scripts.Gameplay.Root.View
{
    public class BuildingSlotUI : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private Image _buildingImage;

        private const float SmallWidth = 40f;
        private const float MediumWidth = 80f;
        private const float LargeWidth = 160f;

        private BuildingData _currentData;
        private IBuildingSpawnService _spawnService;
        private Action _onBuildingPlaced;
        private Camera _uiCamera;

        private Func<BuildingSlotUI, bool> _interceptHandler;
        private bool _interactable = true;

        public BuildingData CurrentData => _currentData;
        public event Action OnBuildingPlaced;

        public void Initialize(IBuildingSpawnService spawnService, Action onBuildingPlaced, Camera uiCamera)
        {
            _spawnService = spawnService;
            _onBuildingPlaced = onBuildingPlaced;
            _uiCamera = uiCamera;
        }

        public void SetInterceptHandler(Func<BuildingSlotUI, bool> handler)
        {
            _interceptHandler = handler;
        }

        public void SetInteractable(bool interactable)
        {
            _interactable = interactable;
        }

        public void SetBuilding(BuildingData data)
        {
            _currentData = data;
            _buildingImage.sprite = data.Sprite;
            _buildingImage.enabled = true;
            gameObject.SetActive(true);
            UpdateImageWidth(data.Size);
        }

        public void Clear()
        {
            _currentData = null;
            _buildingImage.sprite = null;
            _buildingImage.enabled = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_interactable) return;
            if (_currentData == null) return;

            if (_interceptHandler != null && _interceptHandler(this))
                return;

            if (_spawnService == null) return;

            var data = _currentData;
            var dragHandler = _spawnService.SpawnAndBeginDrag(data, eventData.position);

            Vector3 slotWorldPos = GetSlotWorldPosition();
            dragHandler.SetReturnPosition(slotWorldPos);

            Clear();

            dragHandler.OnPlaced += () =>
            {
                _onBuildingPlaced?.Invoke();
                OnBuildingPlaced?.Invoke();
            };
            dragHandler.OnFailed += () => SetBuilding(data);
        }

        private Vector3 GetSlotWorldPosition()
        {
            RectTransform rt = transform as RectTransform;
            if (rt != null && _uiCamera != null)
            {
                Vector3[] corners = new Vector3[4];
                rt.GetWorldCorners(corners);
                Vector3 center = (corners[0] + corners[2]) * 0.5f;
                Vector3 screenPos = _uiCamera.WorldToScreenPoint(center);
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
                worldPos.z = 0f;
                return worldPos;
            }

            Vector3 fallback = Camera.main.ScreenToWorldPoint(
                RectTransformUtility.WorldToScreenPoint(_uiCamera, transform.position));
            fallback.z = 0f;
            return fallback;
        }

        private void UpdateImageWidth(BuildingSize size)
        {
            var rt = _buildingImage.rectTransform;
            var sizeDelta = rt.sizeDelta;
            sizeDelta.x = size switch
            {
                BuildingSize.Small => SmallWidth,
                BuildingSize.Medium => MediumWidth,
                BuildingSize.Large => LargeWidth,
                _ => sizeDelta.x
            };
            rt.sizeDelta = sizeDelta;
        }
    }
}
