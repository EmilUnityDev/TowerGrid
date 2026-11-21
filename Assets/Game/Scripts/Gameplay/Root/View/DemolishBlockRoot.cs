using System;
using System.Collections.Generic;
using DG.Tweening;
using Game.Scripts.Gameplay.Building;
using Game.Scripts.Gameplay.Building.Grid;
using UnityEngine;

namespace Game.Scripts.Gameplay.Root.View
{
    public class DemolishBlockRoot : MonoBehaviour
    {
        private DemolishCell[] _cells;
        private IBuildingGrid _grid;
        private GridConfig _gridConfig;
        private Action _onDemolishComplete;

        public void Initialize(IBuildingGrid grid, GridConfig gridConfig, Action onDemolishComplete)
        {
            _grid = grid;
            _gridConfig = gridConfig;
            _onDemolishComplete = onDemolishComplete;

            _cells = GetComponentsInChildren<DemolishCell>(true);
            foreach (var cell in _cells)
                cell.OnCellClicked += OnCellSelected;

            Hide();
        }

        public void Show()
        {
            foreach (var cell in _cells)
                cell.gameObject.SetActive(true);
        }

        public void Hide()
        {
            foreach (var cell in _cells)
                cell.gameObject.SetActive(false);
        }

        private void OnCellSelected(DemolishCell cell)
        {
            var bounds = cell.Collider.bounds;

            var buildings = FindBuildingsInBounds(bounds);
            if (buildings.Count == 0)
                return;

           
            foreach (var building in buildings)
            {
                ClearGridForBuilding(building);
            }

            int remaining = buildings.Count;
            foreach (var building in buildings)
            {
                var sr = building.GetComponentInChildren<SpriteRenderer>();
                var seq = DOTween.Sequence();
                seq.Append(building.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack));
                if (sr != null) seq.Join(sr.DOFade(0f, 0.25f));
                seq.OnComplete(() =>
                {
                    Destroy(building.gameObject);
                    remaining--;
                    if (remaining <= 0) _onDemolishComplete?.Invoke();
                });
            }
        }

        private void ClearGridForBuilding(BuildingInstance building)
        {
            var data = building.Data;
            Vector2 pos = building.transform.position;

            
            int level = -1;
            float bestDist = float.MaxValue;
            for (int l = 0; l < _grid.TotalLevels; l++)
            {
                float dist = Mathf.Abs(pos.y - _gridConfig.GetLevelWorldY(l));
                if (dist < bestDist)
                {
                    bestDist = dist;
                    level = l;
                }
            }

            
            int slotWidth = data.SlotWidth;
            int bestSlot = 0;
            float bestSlotDist = float.MaxValue;
            for (int s = 0; s <= _grid.TotalSlots - slotWidth; s++)
            {
                float centerX = _gridConfig.GetBuildingCenterX(s, slotWidth);
                float dist = Mathf.Abs(pos.x - centerX);
                if (dist < bestSlotDist)
                {
                    bestSlotDist = dist;
                    bestSlot = s;
                }
            }

           
            for (int i = bestSlot; i < bestSlot + slotWidth; i++)
            {
                Debug.Log($"[Demolish] Clearing slot={i} level={level}");
                _grid.ClearCell(i, level);
            }
        }

        private List<BuildingInstance> FindBuildingsInBounds(Bounds bounds)
        {
            var results = new List<BuildingInstance>();
            foreach (var b in FindObjectsByType<BuildingInstance>(FindObjectsSortMode.None))
            {
                if (!b.IsPlaced) continue;
                var col = b.GetComponent<BoxCollider2D>();
                if (col != null && bounds.Intersects(col.bounds))
                    results.Add(b);
            }
            return results;
        }

        private void OnDestroy()
        {
            if (_cells == null) return;
            foreach (var cell in _cells)
                if (cell != null) cell.OnCellClicked -= OnCellSelected;
        }
    }
}
