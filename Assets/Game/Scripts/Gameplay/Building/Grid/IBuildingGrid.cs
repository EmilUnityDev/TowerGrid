using Game.Scripts.Gameplay.Building.Data;
using UnityEngine;

namespace Game.Scripts.Gameplay.Building.Grid
{
    public interface IBuildingGrid
    {
        bool CanPlace(int slotIndex, int level, BuildingData data);
        bool TryPlace(int slotIndex, int level, BuildingData data);
        bool TryGetSlotAt(Vector2 worldPos, BuildingData data, out int slotIndex, out int level);
        bool TryFindNearestValidSlot(Vector2 worldPos, BuildingData data, out int slotIndex, out int level);
        Vector2 GetSnapPosition(int slotIndex, int level, BuildingData data);
        Vector2 GetTightSnapPosition(int slotIndex, int level, BuildingData data, float colliderWidth, float gridMinX, float gridMaxX);
        float GetOccupiedRightEdge(int level, int slotIndex, float gridMinX);
        float GetOccupiedLeftEdge(int level, int slotIndex, int buildingWidth, float gridMinX);
        bool IsOverlapping(Vector2 worldPos, float colliderWidth, int level);

        /// <summary>Количество внешних контактов (горизонт/вертикаль) для фигуры шириной slotWidth на позиции (slotIndex, level).</summary>
        int CountExternalContacts(int slotIndex, int level, int slotWidth);

        /// <summary>Сколько нижних клеток фигуры имеют прямую опору (занятую клетку уровнем ниже).</summary>
        int CountDirectSupport(int slotIndex, int level, int slotWidth);

        /// <summary>Проверяет, полностью ли заполнен уровень.</summary>
        bool IsLevelComplete(int level);

        /// <summary>Отмечает уровень как уже засчитанный для бонуса.</summary>
        void MarkLevelRewarded(int level);

        /// <summary>Был ли уровень уже засчитан.</summary>
        bool IsLevelRewarded(int level);

        /// <summary>Количество соседств Service↔Living для фигуры.</summary>
        int CountServiceLivingNeighborhoods(int slotIndex, int level, int slotWidth, BuildingType placedType);

        /// <summary>Возвращает тип здания в ячейке (null если пусто).</summary>
        BuildingType? GetTypeAt(int slotIndex, int level);

        int TotalSlots { get; }
        int TotalLevels { get; }

        bool ClearCell(int slotIndex, int level);

        bool HasSpaceForSize(int slotWidth);

        void SetPlacementRestriction(int allowedSlotIndex, int allowedLevel, BuildingData allowedData);
        void ClearPlacementRestriction();
    }
}
