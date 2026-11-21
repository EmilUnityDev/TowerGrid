using System;
using Game.Scripts.Services.Money;
using UnityEngine;

public class BattlePassService
{
    private const string PointsKey =
        "BATTLE_PASS_POINTS";

    private const string ClaimedRewardKeyPrefix =
        "BATTLE_PASS_REWARD_CLAIMED_";

    public const int LevelsCount = 20;
    public const int PointsPerLevel = 100;
    public const int MoneyReward = 500;

    public const int ScoreReward = MoneyReward;

    private readonly MoneyService _moneyService;

    public event Action<int> OnPointsChanged;
    public event Action<int> OnLevelChanged;
    public event Action<int> OnRewardClaimed;
    public event Action OnBattlePassChanged;

    public int Points { get; private set; }

    public int UnlockedLevelsCount =>
        Mathf.Clamp(
            Points / PointsPerLevel,
            0,
            LevelsCount);

    public int CurrentDisplayLevel =>
        Mathf.Clamp(
            UnlockedLevelsCount + 1,
            1,
            LevelsCount);

    public int MaximumPoints =>
        LevelsCount * PointsPerLevel;

    public bool IsCompleted =>
        Points >= MaximumPoints;

    public BattlePassService(
        MoneyService moneyService)
    {
        _moneyService = moneyService;
        Load();
    }

    public void AddPoints(int amount)
    {
        if (amount <= 0)
            return;

        int previousUnlockedLevels =
            UnlockedLevelsCount;

        long result =
            (long)Points + amount;

        Points = (int)Math.Min(
            result,
            int.MaxValue);

        SavePoints();

        int newUnlockedLevels =
            UnlockedLevelsCount;

        OnPointsChanged?.Invoke(Points);

        if (newUnlockedLevels !=
            previousUnlockedLevels)
        {
            OnLevelChanged?.Invoke(
                newUnlockedLevels);
        }

        OnBattlePassChanged?.Invoke();
    }

    public bool CanClaimReward(
        int levelIndex)
    {
        return GetRewardState(levelIndex) ==
               BattlePassRewardState.Available;
    }

    public bool ClaimReward(
        int levelIndex)
    {
        if (!IsValidLevelIndex(levelIndex))
        {
            Debug.LogError(
                $"[BattlePassService] Некорректный индекс: {levelIndex}");

            return false;
        }

        if (!CanClaimReward(levelIndex))
            return false;

        SetRewardClaimed(levelIndex);

        _moneyService.AddMoney(
            MoneyReward);

        OnRewardClaimed?.Invoke(
            levelIndex);

        OnBattlePassChanged?.Invoke();

        return true;
    }

    public BattlePassRewardState GetRewardState(
        int levelIndex)
    {
        if (!IsValidLevelIndex(levelIndex))
            return BattlePassRewardState.Locked;

        if (IsRewardClaimed(levelIndex))
            return BattlePassRewardState.Claimed;

        int requiredPoints =
            GetRequiredPoints(levelIndex);

        return Points >= requiredPoints
            ? BattlePassRewardState.Available
            : BattlePassRewardState.Locked;
    }

    public int GetRequiredPoints(
        int levelIndex)
    {
        if (!IsValidLevelIndex(levelIndex))
            return 0;

        return (levelIndex + 1) *
               PointsPerLevel;
    }

    public bool IsRewardClaimed(
        int levelIndex)
    {
        if (!IsValidLevelIndex(levelIndex))
            return false;

        return PlayerPrefs.GetInt(
            GetClaimedRewardKey(levelIndex),
            0) == 1;
    }

    public int GetPointsInsideCurrentLevel()
    {
        if (IsCompleted)
            return PointsPerLevel;

        return Points % PointsPerLevel;
    }

    public float GetNormalizedProgressInsideCurrentLevel()
    {
        if (IsCompleted)
            return 1f;

        return Mathf.Clamp01(
            (float)GetPointsInsideCurrentLevel() /
            PointsPerLevel);
    }

    public void ResetProgress()
    {
        Points = 0;

        PlayerPrefs.DeleteKey(
            PointsKey);

        for (int i = 0;
             i < LevelsCount;
             i++)
        {
            PlayerPrefs.DeleteKey(
                GetClaimedRewardKey(i));
        }

        PlayerPrefs.Save();

        OnPointsChanged?.Invoke(Points);
        OnLevelChanged?.Invoke(0);
        OnBattlePassChanged?.Invoke();
    }

    private void Load()
    {
        Points = Mathf.Max(
            0,
            PlayerPrefs.GetInt(
                PointsKey,
                0));
    }

    private void SavePoints()
    {
        PlayerPrefs.SetInt(
            PointsKey,
            Points);

        PlayerPrefs.Save();
    }

    private void SetRewardClaimed(
        int levelIndex)
    {
        PlayerPrefs.SetInt(
            GetClaimedRewardKey(levelIndex),
            1);

        PlayerPrefs.Save();
    }

    private static bool IsValidLevelIndex(
        int levelIndex)
    {
        return levelIndex >= 0 &&
               levelIndex < LevelsCount;
    }

    private static string GetClaimedRewardKey(
        int levelIndex)
    {
        return
            $"{ClaimedRewardKeyPrefix}{levelIndex}";
    }
}