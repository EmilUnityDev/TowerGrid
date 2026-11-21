using UnityEngine;

namespace Game.Scripts.Gameplay.Core.Services
{
    public static class LocalScoreStorage
    {
        private const string BestKey = "Portfolio_BestScore";
        private const string PreviousBestKey = "Portfolio_PreviousBestScore";
        private const string TotalKey = "Portfolio_TotalScore";
        private const string CountKey = "Portfolio_MatchesPlayed";

        public static int BestScore => PlayerPrefs.GetInt(BestKey, 0);
        public static int PreviousBestScore => PlayerPrefs.GetInt(PreviousBestKey, 0);
        public static int MatchCount => PlayerPrefs.GetInt(CountKey, 0);
        public static int AverageScore => MatchCount == 0 ? 0 : Mathf.RoundToInt(PlayerPrefs.GetFloat(TotalKey, 0f) / MatchCount);

        public static void RecordScore(int score)
        {
            score = Mathf.Max(0, score);
            int previousBest = BestScore;
            PlayerPrefs.SetInt(PreviousBestKey, previousBest);
            PlayerPrefs.SetInt(BestKey, Mathf.Max(previousBest, score));
            PlayerPrefs.SetFloat(TotalKey, PlayerPrefs.GetFloat(TotalKey, 0f) + score);
            PlayerPrefs.SetInt(CountKey, MatchCount + 1);
            PlayerPrefs.Save();
        }
    }
}
