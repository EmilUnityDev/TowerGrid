using System;
using System.Collections.Generic;
using UnityEngine;

public enum DailyQuestCategory
{
    Floors,
    Skills,
    Score,
    FragmentTypes,
    HeightZoning
}

public enum DailyQuestType
{
    Complete3Floors,
    Complete5Floors,
    CompleteMatchWithoutCriticalOverflow,

    UseDraftRefresh2Times,
    UseRezone2Times,
    UseDemolition1Time,
    CompleteMatchWithoutSkills,

    Score3000InSingleMatch,
    Score5000Total,
    Get3Combos,
    Make3SuccessfulMovesInRow,

    Place10ResidentialFragments,
    Place8CommercialFragments,
    Place6ServiceFragments,
    Place5PremiumFragments,

    Build2CommercialFloorsInLowerZone,
    Build2ResidentialFloorsInMiddleZone,
    Place3PremiumFragmentsInUpperZone,
    Create5ServiceResidentialAdjacencies
}

public enum DailyFragmentType
{
    Residential,
    Commercial,
    Service,
    Premium
}

public enum DailyHeightZone
{
    Lower,
    Middle,
    Upper
}

public enum DailySkillType
{
    DraftRefresh,
    Rezone,
    Demolition
}

[Serializable]
public class DailyQuestDefinition
{
    [Header("Main")]
    public string id;
    public string title;

    public DailyQuestCategory category;
    public DailyQuestType questType;

    [Min(1)]
    public int targetValue = 1;

    [Header("Rewards")]
    [Min(0)]
    public int softCurrencyReward = 100;

    [Min(0)]
    public int battlePassPointsReward = 10;
}

[Serializable]
public class ActiveDailyQuestData
{
    public string questId;
    public int progress;
    public bool rewardClaimed;
}

[Serializable]
public class DailyQuestSaveData
{
    public string generationDate;
    public List<ActiveDailyQuestData> activeQuests = new();
}