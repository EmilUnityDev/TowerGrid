using System;
using System.Collections.Generic;
using System.Linq;
using Game.Scripts.Services.Money;
using UnityEngine;
using VContainer;

public class DailyQuestManager : MonoBehaviour
{
    private const string SaveKey =
        "DAILY_QUESTS_SAVE";

    [Header("Settings")]
    [SerializeField] private DailyQuestCatalog questCatalog;

    [Range(1, 10)]
    [SerializeField] private int dailyQuestCount = 3;

    public event Action OnQuestsChanged;
    public event Action<string> OnQuestProgressChanged;

    private MoneyService _moneyService;
    private BattlePassService _battlePassService;
    private DailyQuestSaveData _saveData;

    private int _currentMatchScore;
    private int _successfulMovesInRow;

    private bool _skillUsedInCurrentMatch;
    private bool _criticalOverflowInCurrentMatch;

    public IReadOnlyList<ActiveDailyQuestData> ActiveQuests =>
        _saveData?.activeQuests;

    [Inject]
    public void Construct(
        MoneyService moneyService,
        BattlePassService battlePassService)
    {
        _moneyService = moneyService;
        _battlePassService = battlePassService;
    }

    private void Awake()
    {
        LoadOrGenerateDailyQuests();
    }

    public void LoadOrGenerateDailyQuests()
    {
        Load();

        string currentDate =
            GetCurrentDate();

        bool needsNewQuests =
            _saveData == null ||
            _saveData.generationDate != currentDate ||
            _saveData.activeQuests == null ||
            _saveData.activeQuests.Count == 0;

        if (needsNewQuests)
        {
            GenerateNewDailyQuests();
        }

        OnQuestsChanged?.Invoke();
    }

    private void GenerateNewDailyQuests()
    {
        _saveData = new DailyQuestSaveData
        {
            generationDate =
                GetCurrentDate(),

            activeQuests =
                new List<ActiveDailyQuestData>()
        };

        if (questCatalog == null ||
            questCatalog.Quests.Count == 0)
        {
            Debug.LogError(
                "[DailyQuestManager] Quest catalog is empty.");

            Save();
            return;
        }

        List<DailyQuestDefinition> availableQuests =
            questCatalog.Quests
                .Where(quest => quest != null)
                .OrderBy(_ => UnityEngine.Random.value)
                .ToList();

        HashSet<DailyQuestCategory> selectedCategories =
            new();

        foreach (DailyQuestDefinition definition
                 in availableQuests)
        {
            if (_saveData.activeQuests.Count >=
                dailyQuestCount)
            {
                break;
            }

            if (selectedCategories.Contains(
                    definition.category))
            {
                continue;
            }

            AddGeneratedQuest(
                definition);

            selectedCategories.Add(
                definition.category);
        }

        foreach (DailyQuestDefinition definition
                 in availableQuests)
        {
            if (_saveData.activeQuests.Count >=
                dailyQuestCount)
            {
                break;
            }

            bool alreadyAdded =
                _saveData.activeQuests.Any(
                    quest =>
                        quest.questId ==
                        definition.id);

            if (alreadyAdded)
                continue;

            AddGeneratedQuest(
                definition);
        }

        Save();
    }

    private void AddGeneratedQuest(
        DailyQuestDefinition definition)
    {
        _saveData.activeQuests.Add(
            new ActiveDailyQuestData
            {
                questId = definition.id,
                progress = 0,
                rewardClaimed = false
            });
    }

    public DailyQuestDefinition GetDefinition(
        string questId)
    {
        if (questCatalog == null)
            return null;

        return questCatalog.GetQuestById(
            questId);
    }

    public ActiveDailyQuestData GetActiveQuest(
        string questId)
    {
        return _saveData?.activeQuests?.Find(
            quest =>
                quest.questId == questId);
    }

    public void AddProgress(
        DailyQuestType questType,
        int amount = 1)
    {
        if (amount <= 0 ||
            _saveData?.activeQuests == null)
        {
            return;
        }

        foreach (ActiveDailyQuestData activeQuest
                 in _saveData.activeQuests)
        {
            DailyQuestDefinition definition =
                GetDefinition(
                    activeQuest.questId);

            if (definition == null)
                continue;

            if (definition.questType !=
                questType)
            {
                continue;
            }

            if (activeQuest.rewardClaimed)
                continue;

            int oldProgress =
                activeQuest.progress;

            activeQuest.progress =
                Mathf.Clamp(
                    activeQuest.progress + amount,
                    0,
                    definition.targetValue);

            if (oldProgress !=
                activeQuest.progress)
            {
                OnQuestProgressChanged?.Invoke(
                    activeQuest.questId);
            }
        }

        Save();
    }

    public void SetProgressToHighest(
        DailyQuestType questType,
        int newProgress)
    {
        if (_saveData?.activeQuests == null)
            return;

        foreach (ActiveDailyQuestData activeQuest
                 in _saveData.activeQuests)
        {
            DailyQuestDefinition definition =
                GetDefinition(
                    activeQuest.questId);

            if (definition == null)
                continue;

            if (definition.questType !=
                questType)
            {
                continue;
            }

            if (activeQuest.rewardClaimed)
                continue;

            int clampedProgress =
                Mathf.Clamp(
                    newProgress,
                    0,
                    definition.targetValue);

            if (clampedProgress <=
                activeQuest.progress)
            {
                continue;
            }

            activeQuest.progress =
                clampedProgress;

            OnQuestProgressChanged?.Invoke(
                activeQuest.questId);
        }

        Save();
    }

    public bool IsCompleted(
        string questId)
    {
        ActiveDailyQuestData activeQuest =
            GetActiveQuest(questId);

        DailyQuestDefinition definition =
            GetDefinition(questId);

        if (activeQuest == null ||
            definition == null)
        {
            return false;
        }

        return activeQuest.progress >=
               definition.targetValue;
    }

    public bool CanClaimReward(
        string questId)
    {
        ActiveDailyQuestData activeQuest =
            GetActiveQuest(questId);

        if (activeQuest == null)
            return false;

        return IsCompleted(questId) &&
               !activeQuest.rewardClaimed;
    }

    public void ClaimReward(
        string questId)
    {
        ActiveDailyQuestData activeQuest =
            GetActiveQuest(questId);

        DailyQuestDefinition definition =
            GetDefinition(questId);

        if (activeQuest == null ||
            definition == null)
        {
            return;
        }

        if (!CanClaimReward(questId))
            return;

        if (_moneyService == null)
        {
            Debug.LogError(
                "[DailyQuestManager] " +
                "MoneyService was not injected.");

            return;
        }

        if (_battlePassService == null)
        {
            Debug.LogError(
                "[DailyQuestManager] " +
                "BattlePassService was not injected.");

            return;
        }

        activeQuest.rewardClaimed = true;

        _moneyService.AddMoney(
            definition.softCurrencyReward);

        _battlePassService.AddPoints(
            definition.battlePassPointsReward);

        Save();

        OnQuestProgressChanged?.Invoke(
            questId);

        Debug.Log(
            $"[DailyQuestManager] Reward claimed: {questId}. " +
            $"Money: +{definition.softCurrencyReward}, " +
            $"Battle Pass: +{definition.battlePassPointsReward}");
    }

    #region Match tracking

    public void RegisterMatchStarted()
    {
        _currentMatchScore = 0;
        _successfulMovesInRow = 0;

        _skillUsedInCurrentMatch = false;
        _criticalOverflowInCurrentMatch = false;
    }

    public void RegisterMatchScoreAdded(
        int scoreAmount)
    {
        if (scoreAmount <= 0)
            return;

        _currentMatchScore +=
            scoreAmount;

        AddProgress(
            DailyQuestType.Score5000Total,
            scoreAmount);

        SetProgressToHighest(
            DailyQuestType.Score3000InSingleMatch,
            _currentMatchScore);
    }

    public void RegisterMatchFinished(
        bool matchCompleted,
        bool hadCriticalOverflow)
    {
        _criticalOverflowInCurrentMatch |=
            hadCriticalOverflow;

        if (!matchCompleted)
            return;

        if (!_criticalOverflowInCurrentMatch)
        {
            AddProgress(
                DailyQuestType
                    .CompleteMatchWithoutCriticalOverflow);
        }

        if (!_skillUsedInCurrentMatch)
        {
            AddProgress(
                DailyQuestType
                    .CompleteMatchWithoutSkills);
        }
    }

    public void RegisterCriticalOverflow()
    {
        _criticalOverflowInCurrentMatch =
            true;
    }

    #endregion

    #region Floors

    public void RegisterFloorCompleted()
    {
        AddProgress(
            DailyQuestType.Complete3Floors);

        AddProgress(
            DailyQuestType.Complete5Floors);
    }

    public void RegisterFloorCompletedWithFragment(
        DailyFragmentType fragmentType,
        DailyHeightZone heightZone)
    {
        RegisterFloorCompleted();

        if (fragmentType ==
            DailyFragmentType.Commercial &&
            heightZone ==
            DailyHeightZone.Lower)
        {
            AddProgress(
                DailyQuestType
                    .Build2CommercialFloorsInLowerZone);
        }

        if (fragmentType ==
            DailyFragmentType.Residential &&
            heightZone ==
            DailyHeightZone.Middle)
        {
            AddProgress(
                DailyQuestType
                    .Build2ResidentialFloorsInMiddleZone);
        }
    }

    #endregion

    #region Skills

    public void RegisterSkillUsed(
        DailySkillType skillType)
    {
        _skillUsedInCurrentMatch = true;

        switch (skillType)
        {
            case DailySkillType.DraftRefresh:
                AddProgress(
                    DailyQuestType
                        .UseDraftRefresh2Times);
                break;

            case DailySkillType.Rezone:
                AddProgress(
                    DailyQuestType
                        .UseRezone2Times);
                break;

            case DailySkillType.Demolition:
                AddProgress(
                    DailyQuestType
                        .UseDemolition1Time);
                break;
        }
    }

    #endregion

    #region Fragments

    public void RegisterFragmentPlaced(
        DailyFragmentType fragmentType,
        DailyHeightZone heightZone)
    {
        switch (fragmentType)
        {
            case DailyFragmentType.Residential:
                AddProgress(
                    DailyQuestType
                        .Place10ResidentialFragments);
                break;

            case DailyFragmentType.Commercial:
                AddProgress(
                    DailyQuestType
                        .Place8CommercialFragments);
                break;

            case DailyFragmentType.Service:
                AddProgress(
                    DailyQuestType
                        .Place6ServiceFragments);
                break;

            case DailyFragmentType.Premium:
                AddProgress(
                    DailyQuestType
                        .Place5PremiumFragments);

                if (heightZone ==
                    DailyHeightZone.Upper)
                {
                    AddProgress(
                        DailyQuestType
                            .Place3PremiumFragmentsInUpperZone);
                }

                break;
        }
    }

    public void RegisterServiceResidentialAdjacency(
        int amount = 1)
    {
        AddProgress(
            DailyQuestType
                .Create5ServiceResidentialAdjacencies,
            amount);
    }

    #endregion

    #region Combos and successful moves

    public void RegisterCombo()
    {
        AddProgress(
            DailyQuestType.Get3Combos);
    }

    public void RegisterSuccessfulMove()
    {
        _successfulMovesInRow++;

        SetProgressToHighest(
            DailyQuestType
                .Make3SuccessfulMovesInRow,
            _successfulMovesInRow);
    }

    public void RegisterFailedMove()
    {
        _successfulMovesInRow = 0;
    }

    #endregion

    private string GetCurrentDate()
    {
        return DateTime.UtcNow.ToString(
            "yyyy-MM-dd");
    }

    private void Save()
    {
        if (_saveData == null)
            return;

        string json =
            JsonUtility.ToJson(_saveData);

        PlayerPrefs.SetString(
            SaveKey,
            json);

        PlayerPrefs.Save();
    }

    private void Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            _saveData = null;
            return;
        }

        string json =
            PlayerPrefs.GetString(SaveKey);

        if (string.IsNullOrWhiteSpace(json))
        {
            _saveData = null;
            return;
        }

        try
        {
            _saveData =
                JsonUtility.FromJson<
                    DailyQuestSaveData>(json);
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[DailyQuestManager] Failed to load save: " +
                $"{exception}");

            _saveData = null;
        }
    }

#if UNITY_EDITOR

    [ContextMenu("Debug/Clear Daily Quests")]
    private void DebugClearDailyQuests()
    {
        PlayerPrefs.DeleteKey(
            SaveKey);

        PlayerPrefs.Save();

        LoadOrGenerateDailyQuests();
    }

    [ContextMenu("Debug/Complete All Daily Quests")]
    private void DebugCompleteAllDailyQuests()
    {
        if (_saveData?.activeQuests == null)
            return;

        foreach (ActiveDailyQuestData activeQuest
                 in _saveData.activeQuests)
        {
            DailyQuestDefinition definition =
                GetDefinition(
                    activeQuest.questId);

            if (definition == null)
                continue;

            activeQuest.progress =
                definition.targetValue;
        }

        Save();
        OnQuestsChanged?.Invoke();
    }

#endif
}