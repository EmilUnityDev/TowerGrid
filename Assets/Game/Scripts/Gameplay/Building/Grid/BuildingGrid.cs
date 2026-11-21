using Game.Scripts.Gameplay.Building.Data;
using UnityEngine;

namespace Game.Scripts.Gameplay.Building.Grid
{
    public class BuildingGrid : IBuildingGrid
    {
        private readonly GridConfig _config;
        private readonly bool[,] _occupied;
        private readonly float[,] _placedCenterX;
        private readonly float[,] _placedHalfWidth;
        private readonly BuildingType?[,] _types;
        private readonly bool[] _levelRewarded;

        private bool _hasRestriction;
        private int _restrictedSlot;
        private int _restrictedLevel;
        private BuildingData _restrictedData;

        public int TotalSlots => _config.TotalSlots;
        public int TotalLevels => _config.TotalLevels;

        public BuildingGrid(GridConfig config)
        {
            _config = config;
            _occupied = new bool[config.TotalLevels, config.TotalSlots];
            _placedCenterX = new float[config.TotalLevels, config.TotalSlots];
            _placedHalfWidth = new float[config.TotalLevels, config.TotalSlots];
            _types = new BuildingType?[config.TotalLevels, config.TotalSlots];
            _levelRewarded = new bool[config.TotalLevels];
        }

        public void SetPlacementRestriction(int allowedSlotIndex, int allowedLevel, BuildingData allowedData)
        {
            _hasRestriction = true;
            _restrictedSlot = allowedSlotIndex;
            _restrictedLevel = allowedLevel;
            _restrictedData = allowedData;
        }

        public void ClearPlacementRestriction()
        {
            _hasRestriction = false;
            _restrictedData = null;
        }

        public bool CanPlace(int slotIndex, int level, BuildingData data)
        {
            if (_hasRestriction)
            {
                if (slotIndex != _restrictedSlot || level != _restrictedLevel ||
                    data.Type != _restrictedData.Type || data.Size != _restrictedData.Size)
                    return false;
            }

            int width = data.SlotWidth;

            if (slotIndex < 0 || slotIndex + width > _config.TotalSlots)
                return false;
            if (level < 0 || level >= _config.TotalLevels)
                return false;

            for (int i = slotIndex; i < slotIndex + width; i++)
            {
                if (_occupied[level, i])
                    return false;
            }

            if (level > 0 && !HasSupportBelow(slotIndex, level, width))
                return false;

            return true;
        }

        public bool TryPlace(int slotIndex, int level, BuildingData data)
        {
            if (!CanPlace(slotIndex, level, data))
                return false;

            int width = data.SlotWidth;
            for (int i = slotIndex; i < slotIndex + width; i++)
            {
                _occupied[level, i] = true;
                _types[level, i] = data.Type;
            }

            return true;
        }

        public BuildingType? GetTypeAt(int slotIndex, int level)
        {
            if (slotIndex < 0 || slotIndex >= _config.TotalSlots) return null;
            if (level < 0 || level >= _config.TotalLevels) return null;
            return _types[level, slotIndex];
        }

        public int CountExternalContacts(int slotIndex, int level, int slotWidth)
        {
            int contacts = 0;
            for (int i = slotIndex; i < slotIndex + slotWidth; i++)
            {
                if (level + 1 < _config.TotalLevels && _occupied[level + 1, i] && !IsPartOfPlacement(i, level + 1, slotIndex, level, slotWidth))
                    contacts++;
                if (level - 1 >= 0 && _occupied[level - 1, i])
                    contacts++;
                if (i - 1 >= 0 && _occupied[level, i - 1] && !IsPartOfPlacement(i - 1, level, slotIndex, level, slotWidth))
                    contacts++;
                if (i + 1 < _config.TotalSlots && _occupied[level, i + 1] && !IsPartOfPlacement(i + 1, level, slotIndex, level, slotWidth))
                    contacts++;
            }
            return contacts;
        }

        private bool IsPartOfPlacement(int slot, int lvl, int placedSlot, int placedLevel, int placedWidth)
        {
            return lvl == placedLevel && slot >= placedSlot && slot < placedSlot + placedWidth;
        }

        public int CountDirectSupport(int slotIndex, int level, int slotWidth)
        {
            if (level == 0) return slotWidth; 
            int count = 0;
            for (int i = slotIndex; i < slotIndex + slotWidth; i++)
            {
                if (_occupied[level - 1, i])
                    count++;
            }
            return count;
        }

        public bool IsLevelComplete(int level)
        {
            for (int i = 0; i < _config.TotalSlots; i++)
            {
                if (!_occupied[level, i]) return false;
            }
            return true;
        }

        public void MarkLevelRewarded(int level)
        {
            if (level >= 0 && level < _config.TotalLevels)
                _levelRewarded[level] = true;
        }

        public bool IsLevelRewarded(int level)
        {
            return level >= 0 && level < _config.TotalLevels && _levelRewarded[level];
        }

        public int CountServiceLivingNeighborhoods(int slotIndex, int level, int slotWidth, BuildingType placedType)
        {
            if (placedType != BuildingType.Service && placedType != BuildingType.Living)
                return 0;

            var targetType = placedType == BuildingType.Service ? BuildingType.Living : BuildingType.Service;
            int count = 0;

            for (int i = slotIndex; i < slotIndex + slotWidth; i++)
            {
                if (CheckNeighborType(i, level, -1, 0, slotIndex, slotWidth, targetType)) count++;
                if (CheckNeighborType(i, level, 1, 0, slotIndex, slotWidth, targetType)) count++;
                if (CheckNeighborType(i, level, 0, -1, slotIndex, slotWidth, targetType)) count++;
                if (CheckNeighborType(i, level, 0, 1, slotIndex, slotWidth, targetType)) count++;
            }
            return count;
        }

        private bool CheckNeighborType(int slot, int level, int ds, int dl, int placedSlot, int placedWidth, BuildingType target)
        {
            int ns = slot + ds;
            int nl = level + dl;
            if (ns < 0 || ns >= _config.TotalSlots || nl < 0 || nl >= _config.TotalLevels) return false;
            if (dl == 0 && ns >= placedSlot && ns < placedSlot + placedWidth) return false;
            return _types[nl, ns] == target;
        }

        public bool TryGetSlotAt(Vector2 worldPos, BuildingData data, out int slotIndex, out int level)
        {
            if (_hasRestriction)
            {
                slotIndex = _restrictedSlot;
                level = _restrictedLevel;
                return CanPlace(slotIndex, level, data);
            }

            level = FindNearestLevel(worldPos.y);
            slotIndex = WorldXToSlot(worldPos.x, data.SlotWidth);

            if (CanPlace(slotIndex, level, data) && !IsOverlapping(worldPos, GetEstimatedWidth(data), level))
                return true;

            int alt = WorldXToSlotFloor(worldPos.x, data.SlotWidth);
            if (alt != slotIndex && CanPlace(alt, level, data) && !IsOverlapping(worldPos, GetEstimatedWidth(data), level))
            {
                slotIndex = alt;
                return true;
            }

            alt = WorldXToSlotCeil(worldPos.x, data.SlotWidth);
            if (alt != slotIndex && CanPlace(alt, level, data) && !IsOverlapping(worldPos, GetEstimatedWidth(data), level))
            {
                slotIndex = alt;
                return true;
            }

            int baseSlot = slotIndex;
            for (int offset = 1; offset <= 3; offset++)
            {
                int left = baseSlot - offset;
                if (left >= 0 && CanPlace(left, level, data))
                {
                    float snapX = _config.GetBuildingCenterX(left, data.SlotWidth);
                    if (!IsOverlapping(new Vector2(snapX, worldPos.y), GetEstimatedWidth(data), level))
                    {
                        slotIndex = left;
                        return true;
                    }
                }

                int right = baseSlot + offset;
                if (right + data.SlotWidth <= _config.TotalSlots && CanPlace(right, level, data))
                {
                    float snapX = _config.GetBuildingCenterX(right, data.SlotWidth);
                    if (!IsOverlapping(new Vector2(snapX, worldPos.y), GetEstimatedWidth(data), level))
                    {
                        slotIndex = right;
                        return true;
                    }
                }
            }

            return false;
        }

        public bool TryFindNearestValidSlot(Vector2 worldPos, BuildingData data, out int slotIndex, out int level)
        {
            level = FindNearestLevel(worldPos.y);
            slotIndex = WorldXToSlot(worldPos.x, data.SlotWidth);

            if (CanPlace(slotIndex, level, data))
                return true;

            for (int offset = 1; offset <= _config.TotalSlots; offset++)
            {
                int left = slotIndex - offset;
                int right = slotIndex + offset;

                if (left >= 0 && CanPlace(left, level, data))
                {
                    slotIndex = left;
                    return true;
                }
                if (right + data.SlotWidth <= _config.TotalSlots && CanPlace(right, level, data))
                {
                    slotIndex = right;
                    return true;
                }
            }

            slotIndex = -1;
            level = -1;
            return false;
        }

        public Vector2 GetSnapPosition(int slotIndex, int level, BuildingData data)
        {
            float centerX = _config.GetBuildingCenterX(slotIndex, data.SlotWidth);
            float y = _config.GetLevelWorldY(level);
            return new Vector2(centerX, y);
        }

        public Vector2 GetTightSnapPosition(int slotIndex, int level, BuildingData data, float colliderWidth, float gridMinX, float gridMaxX)
        {
            float halfWidth = colliderWidth * 0.5f;
            float y = _config.GetLevelWorldY(level);
            float centerX = _config.GetBuildingCenterX(slotIndex, data.SlotWidth);

            float snappedX = centerX;

            bool adjacentLeft = slotIndex > 0 && _occupied[level, slotIndex - 1];
            bool adjacentRight = (slotIndex + data.SlotWidth < _config.TotalSlots) && _occupied[level, slotIndex + data.SlotWidth];

            if (adjacentLeft && adjacentRight)
            {
                float leftEdge = _placedCenterX[level, slotIndex - 1] + _placedHalfWidth[level, slotIndex - 1];
                float rightEdge = _placedCenterX[level, slotIndex + data.SlotWidth] - _placedHalfWidth[level, slotIndex + data.SlotWidth];
                snappedX = (leftEdge + rightEdge) * 0.5f;
            }
            else if (adjacentLeft)
            {
                float leftEdge = _placedCenterX[level, slotIndex - 1] + _placedHalfWidth[level, slotIndex - 1];
                snappedX = leftEdge + halfWidth;
            }
            else if (adjacentRight)
            {
                float rightEdge = _placedCenterX[level, slotIndex + data.SlotWidth] - _placedHalfWidth[level, slotIndex + data.SlotWidth];
                snappedX = rightEdge - halfWidth;
            }
            else if (slotIndex == 0)
            {
                snappedX = gridMinX + halfWidth;
            }
            else if (slotIndex + data.SlotWidth >= _config.TotalSlots)
            {
                snappedX = gridMaxX - halfWidth;
            }

            if (snappedX - halfWidth < gridMinX)
                snappedX = gridMinX + halfWidth;
            if (snappedX + halfWidth > gridMaxX)
                snappedX = gridMaxX - halfWidth;

            for (int i = slotIndex; i < slotIndex + data.SlotWidth; i++)
            {
                _placedCenterX[level, i] = snappedX;
                _placedHalfWidth[level, i] = halfWidth;
            }

            return new Vector2(snappedX, y);
        }

        public float GetOccupiedRightEdge(int level, int slotIndex, float gridMinX)
        {
            for (int i = slotIndex - 1; i >= 0; i--)
            {
                if (_occupied[level, i])
                    return _placedCenterX[level, i] + _placedHalfWidth[level, i];
            }
            return gridMinX;
        }

        public float GetOccupiedLeftEdge(int level, int slotIndex, int buildingWidth, float gridMinX)
        {
            for (int i = slotIndex; i < _config.TotalSlots; i++)
            {
                if (_occupied[level, i])
                    return _placedCenterX[level, i] - _placedHalfWidth[level, i];
            }
            return _config.MaxX;
        }

        private float FindLeftNeighborRightEdge(int level, int slotIndex)
        {
            for (int i = slotIndex - 1; i >= 0; i--)
            {
                if (_occupied[level, i])
                    return _placedCenterX[level, i] + _placedHalfWidth[level, i];
            }
            return float.MinValue;
        }

        private float FindRightNeighborLeftEdge(int level, int startSlot)
        {
            for (int i = startSlot; i < _config.TotalSlots; i++)
            {
                if (_occupied[level, i])
                    return _placedCenterX[level, i] - _placedHalfWidth[level, i];
            }
            return float.MaxValue;
        }

        private bool HasSupportBelow(int slotIndex, int level, int width)
        {
            int below = level - 1;
            for (int i = slotIndex; i < slotIndex + width; i++)
            {
                if (_occupied[below, i])
                    return true;
            }
            return false;
        }

        private int FindNearestLevel(float worldY)
        {
            int best = 0;
            float bestDist = Mathf.Abs(worldY - _config.GetLevelWorldY(0));
            for (int i = 1; i < _config.TotalLevels; i++)
            {
                float dist = Mathf.Abs(worldY - _config.GetLevelWorldY(i));
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = i;
                }
            }
            return best;
        }

        private int WorldXToSlot(float worldX, int buildingWidth)
        {
            float gridStartX = _config.GetGridStartX();
            float relative = worldX - gridStartX;
            float halfBuilding = buildingWidth * _config.SlotWidth * 0.5f;
            int slot = Mathf.RoundToInt((relative - halfBuilding) / _config.SlotWidth);
            return Mathf.Clamp(slot, 0, _config.TotalSlots - buildingWidth);
        }

        private int WorldXToSlotFloor(float worldX, int buildingWidth)
        {
            float gridStartX = _config.GetGridStartX();
            float relative = worldX - gridStartX;
            float halfBuilding = buildingWidth * _config.SlotWidth * 0.5f;
            int slot = Mathf.FloorToInt((relative - halfBuilding) / _config.SlotWidth);
            return Mathf.Clamp(slot, 0, _config.TotalSlots - buildingWidth);
        }

        private int WorldXToSlotCeil(float worldX, int buildingWidth)
        {
            float gridStartX = _config.GetGridStartX();
            float relative = worldX - gridStartX;
            float halfBuilding = buildingWidth * _config.SlotWidth * 0.5f;
            int slot = Mathf.CeilToInt((relative - halfBuilding) / _config.SlotWidth);
            return Mathf.Clamp(slot, 0, _config.TotalSlots - buildingWidth);
        }

        public bool IsOverlapping(Vector2 worldPos, float colliderWidth, int level)
        {
            float tolerance = colliderWidth * 0.15f;
            float halfWidth = colliderWidth * 0.5f - tolerance;
            float newLeft = worldPos.x - halfWidth;
            float newRight = worldPos.x + halfWidth;

            for (int i = 0; i < _config.TotalSlots; i++)
            {
                if (!_occupied[level, i]) continue;
                if (_placedHalfWidth[level, i] <= 0f) continue;

                float placedLeft = _placedCenterX[level, i] - _placedHalfWidth[level, i];
                float placedRight = _placedCenterX[level, i] + _placedHalfWidth[level, i];

                if (newLeft < placedRight && newRight > placedLeft)
                    return true;
            }

            return false;
        }

        private bool IsOverlappingAtSnap(int slotIndex, int level, BuildingData data)
        {
            if (_hasRestriction && slotIndex == _restrictedSlot && level == _restrictedLevel)
                return false;

            float snapX = _config.GetBuildingCenterX(slotIndex, data.SlotWidth);
            float estimatedWidth = GetEstimatedWidth(data);
            return IsOverlapping(new Vector2(snapX, 0f), estimatedWidth, level);
        }

        private float GetEstimatedWidth(BuildingData data)
        {
            return data.SlotWidth * _config.SlotWidth;
        }

        public bool ClearCell(int slotIndex, int level)
        {
            if (slotIndex < 0 || slotIndex >= _config.TotalSlots) return false;
            if (level < 0 || level >= _config.TotalLevels) return false;
            if (!_occupied[level, slotIndex]) return false;

            _occupied[level, slotIndex] = false;
            _types[level, slotIndex] = null;
            _placedCenterX[level, slotIndex] = 0f;
            _placedHalfWidth[level, slotIndex] = 0f;
            return true;
        }

        public bool HasSpaceForSize(int slotWidth)
        {
            for (int level = 0; level < _config.TotalLevels; level++)
            {
                for (int slot = 0; slot <= _config.TotalSlots - slotWidth; slot++)
                {
                    bool fits = true;
                    for (int i = slot; i < slot + slotWidth; i++)
                    {
                        if (_occupied[level, i]) { fits = false; break; }
                    }
                    if (!fits) continue;

                    if (level == 0 || HasSupportBelow(slot, level, slotWidth))
                        return true;
                }
            }
            return false;
        }
    }
}
