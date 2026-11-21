using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(
    fileName = "DailyQuestCatalog",
    menuName = "Daily Quests/Daily Quest Catalog"
)]
public class DailyQuestCatalog : ScriptableObject
{
    private const int DailyMoneyReward = 30;
    private const int DailyBattlePassPointsReward = 35;

    [SerializeField]
    private List<DailyQuestDefinition> quests = new();

    public IReadOnlyList<DailyQuestDefinition> Quests =>
        quests;

    public DailyQuestDefinition GetQuestById(
        string questId)
    {
        return quests.Find(
            quest => quest.id == questId);
    }

#if UNITY_EDITOR

    private void OnEnable()
    {
        EditorApplication.delayCall +=
            RefreshDefaultQuests;
    }

    private void RefreshDefaultQuests()
    {
        if (this == null)
            return;

        FillDefaultQuests();
    }

    [ContextMenu("Fill Default Daily Quests")]
    public void FillDefaultQuests()
    {
        quests = new List<DailyQuestDefinition>
        {
            CreateQuest(
                id: "complete_3_floors",
                title: "Complete 3 Floors",
                category: DailyQuestCategory.Floors,
                questType: DailyQuestType.Complete3Floors,
                targetValue: 3
            ),

            CreateQuest(
                id: "complete_5_floors",
                title: "Complete 5 Floors",
                category: DailyQuestCategory.Floors,
                questType: DailyQuestType.Complete5Floors,
                targetValue: 5
            ),

            CreateQuest(
                id: "complete_without_overflow",
                title: "Complete a Match Without Critical Overflow",
                category: DailyQuestCategory.Floors,
                questType:
                    DailyQuestType
                        .CompleteMatchWithoutCriticalOverflow,
                targetValue: 1
            ),

            CreateQuest(
                id: "use_draft_refresh_2",
                title: "Use Draft Refresh 2 Times",
                category: DailyQuestCategory.Skills,
                questType:
                    DailyQuestType.UseDraftRefresh2Times,
                targetValue: 2
            ),

            CreateQuest(
                id: "use_rezone_2",
                title: "Use Rezone 2 Times",
                category: DailyQuestCategory.Skills,
                questType:
                    DailyQuestType.UseRezone2Times,
                targetValue: 2
            ),

            CreateQuest(
                id: "use_demolition_1",
                title: "Use Demolition 1 Time",
                category: DailyQuestCategory.Skills,
                questType:
                    DailyQuestType.UseDemolition1Time,
                targetValue: 1
            ),

            CreateQuest(
                id: "complete_without_skills",
                title: "Complete a Match Without Using Skills",
                category: DailyQuestCategory.Skills,
                questType:
                    DailyQuestType.CompleteMatchWithoutSkills,
                targetValue: 1
            ),

            CreateQuest(
                id: "score_3000_single_match",
                title: "Score 3000 Points in a Single Match",
                category: DailyQuestCategory.Score,
                questType:
                    DailyQuestType.Score3000InSingleMatch,
                targetValue: 3000
            ),

            CreateQuest(
                id: "score_5000_total",
                title: "Score 5000 Total Points",
                category: DailyQuestCategory.Score,
                questType:
                    DailyQuestType.Score5000Total,
                targetValue: 5000
            ),

            CreateQuest(
                id: "get_3_combos",
                title: "Get 3 Combos",
                category: DailyQuestCategory.Score,
                questType:
                    DailyQuestType.Get3Combos,
                targetValue: 3
            ),

            CreateQuest(
                id: "make_3_successful_moves",
                title: "Make a Streak of 3 Successful Moves",
                category: DailyQuestCategory.Score,
                questType:
                    DailyQuestType.Make3SuccessfulMovesInRow,
                targetValue: 3
            ),

            CreateQuest(
                id: "place_10_residential",
                title: "Place 10 Residential Fragments",
                category:
                    DailyQuestCategory.FragmentTypes,
                questType:
                    DailyQuestType.Place10ResidentialFragments,
                targetValue: 10
            ),

            CreateQuest(
                id: "place_8_commercial",
                title: "Place 8 Commercial Fragments",
                category:
                    DailyQuestCategory.FragmentTypes,
                questType:
                    DailyQuestType.Place8CommercialFragments,
                targetValue: 8
            ),

            CreateQuest(
                id: "place_6_service",
                title: "Place 6 Service Fragments",
                category:
                    DailyQuestCategory.FragmentTypes,
                questType:
                    DailyQuestType.Place6ServiceFragments,
                targetValue: 6
            ),

            CreateQuest(
                id: "place_5_premium",
                title: "Place 5 Premium Fragments",
                category:
                    DailyQuestCategory.FragmentTypes,
                questType:
                    DailyQuestType.Place5PremiumFragments,
                targetValue: 5
            ),

            CreateQuest(
                id: "commercial_lower_2",
                title:
                    "Build 2 Commercial Floors in the Lower Zone",
                category:
                    DailyQuestCategory.HeightZoning,
                questType:
                    DailyQuestType
                        .Build2CommercialFloorsInLowerZone,
                targetValue: 2
            ),

            CreateQuest(
                id: "residential_middle_2",
                title:
                    "Build 2 Residential Floors in the Middle Zone",
                category:
                    DailyQuestCategory.HeightZoning,
                questType:
                    DailyQuestType
                        .Build2ResidentialFloorsInMiddleZone,
                targetValue: 2
            ),

            CreateQuest(
                id: "premium_upper_3",
                title:
                    "Place 3 Premium Fragments in the Upper Zone",
                category:
                    DailyQuestCategory.HeightZoning,
                questType:
                    DailyQuestType
                        .Place3PremiumFragmentsInUpperZone,
                targetValue: 3
            ),

            CreateQuest(
                id: "service_residential_adjacency_5",
                title:
                    "Create 5 Beneficial Service and Residential Adjacencies",
                category:
                    DailyQuestCategory.HeightZoning,
                questType:
                    DailyQuestType
                        .Create5ServiceResidentialAdjacencies,
                targetValue: 5
            )
        };

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();

        Debug.Log(
            $"[DailyQuestCatalog] Updated {quests.Count} daily quests. " +
            $"Reward: {DailyMoneyReward} money and " +
            $"{DailyBattlePassPointsReward} Battle Pass points.",
            this);
    }

    [ContextMenu("Clear Daily Quests")]
    private void ClearQuests()
    {
        quests.Clear();

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }

    private static DailyQuestDefinition CreateQuest(
        string id,
        string title,
        DailyQuestCategory category,
        DailyQuestType questType,
        int targetValue)
    {
        return new DailyQuestDefinition
        {
            id = id,
            title = title,
            category = category,
            questType = questType,
            targetValue = targetValue,
            softCurrencyReward =
                DailyMoneyReward,
            battlePassPointsReward =
                DailyBattlePassPointsReward
        };
    }

#endif
}