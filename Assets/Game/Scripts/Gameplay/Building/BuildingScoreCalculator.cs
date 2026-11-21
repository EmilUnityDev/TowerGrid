using System.Collections.Generic;
using Game.Scripts.Gameplay.Building.Data;
using Game.Scripts.Gameplay.Building.Grid;
using Game.Scripts.Gameplay.Core.Services;
using Game.Scripts.Gameplay.Skills;
using Game.Scripts.Gameplay.UI;
using UnityEngine;

namespace Game.Scripts.Gameplay.Building
{
    public class BuildingScoreCalculator
    {
        private readonly IBuildingGrid _grid;
        private readonly SkillService _skillService;
        private readonly TutorialGameService _tutorialGameService;
        private readonly DailyQuestManager _dailyQuestManager;

        private int _streakCount;

        public BuildingScoreCalculator(
            IBuildingGrid grid,
            SkillService skillService,
            TutorialGameService tutorialGameService,
            DailyQuestManager dailyQuestManager)
        {
            _grid = grid;
            _skillService = skillService;
            _tutorialGameService = tutorialGameService;
            _dailyQuestManager = dailyQuestManager;
            _streakCount = 0;
        }

        public ScoreBonusResult CalculateDetailed(
            BuildingData data,
            int slotIndex,
            int level)
        {
            ScoreBonusResult result =
                new ScoreBonusResult();

            int width =
                data.SlotWidth;

            int baseScore =
                GetBaseScore(width);

            result.Bonuses.Add(
                new ScoreBonusData(
                    "PLACEMENT",
                    baseScore));

            int supportBonus =
                CountSupportBonus(
                    slotIndex,
                    level,
                    width);

            if (supportBonus > 0)
            {
                result.Bonuses.Add(
                    new ScoreBonusData(
                        "SUPPORT",
                        supportBonus));
            }

            int contactCount =
                _grid.CountExternalContacts(
                    slotIndex,
                    level,
                    width);

            int contactBonus =
                contactCount * 8;

            if (contactBonus > 0)
            {
                result.Bonuses.Add(
                    new ScoreBonusData(
                        "SERVICE ADJACENCY",
                        contactBonus));
            }

            int floorBonus =
                CalculateFloorBonus(level);

            if (floorBonus > 0)
            {
                result.Bonuses.Add(
                    new ScoreBonusData(
                        "FLOOR COMPLETE",
                        floorBonus));
            }

            int zoningBonus =
                CalculateZoningBonus(
                    data.Type,
                    level,
                    width);

            if (zoningBonus > 0)
            {
                string zoningLabel =
                    data.Type switch
                    {
                        BuildingType.Commercial =>
                            "COMMERCIAL ZONE",

                        BuildingType.Living =>
                            "LIVING ZONE",

                        BuildingType.Premium =>
                            "PREMIUM HEIGHT",

                        _ =>
                            "ZONE BONUS"
                    };

                result.Bonuses.Add(
                    new ScoreBonusData(
                        zoningLabel,
                        zoningBonus));
            }

            int neighborhoodCount =
                _grid.CountServiceLivingNeighborhoods(
                    slotIndex,
                    level,
                    width,
                    data.Type);

            int neighborhoodBonus =
                neighborhoodCount * 20;

            if (neighborhoodBonus > 0)
            {
                result.Bonuses.Add(
                    new ScoreBonusData(
                        "SERVICE ADJACENCY",
                        neighborhoodBonus));
            }

            int comboConditions = 0;

            if (floorBonus > 0)
                comboConditions++;

            if (IsFullySupported(
                    slotIndex,
                    level,
                    width))
            {
                comboConditions++;
            }

            if (contactCount >= 4)
                comboConditions++;

            if (zoningBonus > 0)
                comboConditions++;

            if (neighborhoodCount > 0)
                comboConditions++;

            int comboBonus =
                GetComboBonus(
                    comboConditions);

            if (comboBonus > 0)
            {
                result.Bonuses.Add(
                    new ScoreBonusData(
                        "COMBO",
                        comboConditions,
                        true));
            }

            int rawTotal =
                baseScore +
                supportBonus +
                contactBonus +
                floorBonus +
                zoningBonus +
                neighborhoodBonus +
                comboBonus;

            bool isSuccessful =
                floorBonus > 0 ||
                zoningBonus > 0 ||
                neighborhoodBonus > 0 ||
                comboBonus > 0;

            if (isSuccessful)
            {
                _streakCount++;
            }
            else
            {
                _streakCount = 0;
            }

            float streakMultiplier =
                GetStreakMultiplier(
                    _streakCount);

            if (_streakCount >= 2)
            {
                result.Bonuses.Add(
                    new ScoreBonusData(
                        "STREAK",
                        _streakCount,
                        true));
            }

            int total =
                Mathf.RoundToInt(
                    rawTotal *
                    streakMultiplier);

            result.TotalScore =
                total;

            bool isTutorial =
                _tutorialGameService != null &&
                _tutorialGameService.IsTutorialGame;

            if (!isTutorial)
            {
                _skillService?.AddChargeForPlacement(
                    width,
                    floorBonus > 0);

                RegisterDailyQuestProgress(
                    data,
                    level,
                    floorBonus > 0,
                    neighborhoodCount,
                    comboBonus > 0,
                    isSuccessful);
            }

            Debug.Log(
                $"[Score] base={baseScore} " +
                $"support={supportBonus} " +
                $"contact={contactBonus} " +
                $"floor={floorBonus} " +
                $"zoning={zoningBonus} " +
                $"neighbor={neighborhoodBonus} " +
                $"combo={comboBonus} " +
                $"streak=x{streakMultiplier:F2} " +
                $"=> {total}");

            return result;
        }

        public int Calculate(
            BuildingData data,
            int slotIndex,
            int level)
        {
            return CalculateDetailed(
                data,
                slotIndex,
                level).TotalScore;
        }

        private void RegisterDailyQuestProgress(
            BuildingData data,
            int level,
            bool floorCompleted,
            int neighborhoodCount,
            bool comboCreated,
            bool successfulMove)
        {
            if (_dailyQuestManager == null ||
                data == null)
            {
                return;
            }

            if (TryConvertFragmentType(
                    data.Type,
                    out DailyFragmentType fragmentType))
            {
                DailyHeightZone heightZone =
                    GetHeightZone(
                        data.Type,
                        level);

                _dailyQuestManager.RegisterFragmentPlaced(
                    fragmentType,
                    heightZone);

                if (floorCompleted)
                {
                    _dailyQuestManager
                        .RegisterFloorCompletedWithFragment(
                            fragmentType,
                            heightZone);
                }
            }
            else if (floorCompleted)
            {
                _dailyQuestManager
                    .RegisterFloorCompleted();
            }

            if (neighborhoodCount > 0)
            {
                _dailyQuestManager
                    .RegisterServiceResidentialAdjacency(
                        neighborhoodCount);
            }

            if (comboCreated)
            {
                _dailyQuestManager.RegisterCombo();
            }

            if (successfulMove)
            {
                _dailyQuestManager
                    .RegisterSuccessfulMove();
            }
            else
            {
                _dailyQuestManager
                    .RegisterFailedMove();
            }
        }

        private static bool TryConvertFragmentType(
            BuildingType buildingType,
            out DailyFragmentType fragmentType)
        {
            switch (buildingType)
            {
                case BuildingType.Living:
                    fragmentType =
                        DailyFragmentType.Residential;

                    return true;

                case BuildingType.Commercial:
                    fragmentType =
                        DailyFragmentType.Commercial;

                    return true;

                case BuildingType.Service:
                    fragmentType =
                        DailyFragmentType.Service;

                    return true;

                case BuildingType.Premium:
                    fragmentType =
                        DailyFragmentType.Premium;

                    return true;

                default:
                    fragmentType = default;
                    return false;
            }
        }

        private static DailyHeightZone GetHeightZone(
            BuildingType buildingType,
            int level)
        {
            if (buildingType ==
                    BuildingType.Commercial &&
                level <= 1)
            {
                return DailyHeightZone.Lower;
            }

            if (buildingType ==
                    BuildingType.Living &&
                level >= 1 &&
                level <= 2)
            {
                return DailyHeightZone.Middle;
            }

            if (buildingType ==
                    BuildingType.Premium &&
                level >= 2)
            {
                return DailyHeightZone.Upper;
            }

            if (level <= 1)
                return DailyHeightZone.Lower;

            if (level <= 2)
                return DailyHeightZone.Middle;

            return DailyHeightZone.Upper;
        }

        private int GetBaseScore(
            int slotWidth)
        {
            return slotWidth switch
            {
                1 => 20,
                2 => 45,
                3 => 75,
                4 => 110,
                _ => 20 * slotWidth
            };
        }

        private int CountSupportBonus(
            int slotIndex,
            int level,
            int width)
        {
            int supported =
                _grid.CountDirectSupport(
                    slotIndex,
                    level,
                    width);

            return supported * 12;
        }

        private bool IsFullySupported(
            int slotIndex,
            int level,
            int width)
        {
            if (level == 0)
                return true;

            return _grid.CountDirectSupport(
                       slotIndex,
                       level,
                       width) >= width;
        }

        private int CalculateFloorBonus(
            int level)
        {
            if (_grid.IsLevelComplete(level) &&
                !_grid.IsLevelRewarded(level))
            {
                _grid.MarkLevelRewarded(
                    level);

                return 180;
            }

            return 0;
        }

        private int CalculateZoningBonus(
            BuildingType type,
            int level,
            int width)
        {
            int perCell = 0;

            switch (type)
            {
                case BuildingType.Commercial:
                    if (level <= 1)
                        perCell = 15;
                    break;

                case BuildingType.Living:
                    if (level >= 1 &&
                        level <= 2)
                    {
                        perCell = 15;
                    }

                    break;

                case BuildingType.Premium:
                    if (level >= 2)
                        perCell = 20;
                    break;
            }

            return perCell * width;
        }

        private int GetComboBonus(
            int conditions)
        {
            return conditions switch
            {
                2 => 80,
                3 => 140,
                4 => 220,
                >= 5 => 300,
                _ => 0
            };
        }

        private float GetStreakMultiplier(
            int streak)
        {
            return streak switch
            {
                <= 1 => 1f,
                2 => 1.15f,
                3 => 1.30f,
                4 => 1.50f,
                _ => 1.75f
            };
        }
    }
}