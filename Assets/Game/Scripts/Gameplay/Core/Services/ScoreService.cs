using System;
using UnityEngine;

namespace Game.Scripts.Gameplay.Core.Services
{
    public class ScoreService
    {
        private const string HighScoreKey =
            "HighScore";

        private int _score;
        private int _highScore;

        public float ScoreMultiplier { get; private set; } =
            1f;

        public int Score
        {
            get => _score;
            private set
            {
                _score = Mathf.Max(0, value);
                OnScoreChanged?.Invoke(_score);
            }
        }

        public int HighScore
        {
            get => _highScore;
            private set
            {
                _highScore = Mathf.Max(0, value);
                OnHighScoreChanged?.Invoke(_highScore);
            }
        }

        public event Action<int> OnScoreChanged;
        public event Action<int> OnHighScoreChanged;

        public ScoreService()
        {
            _score = 0;

            _highScore = Mathf.Max(
                0,
                PlayerPrefs.GetInt(
                    HighScoreKey,
                    0));
        }

        public void AddPoints(int points)
        {
            if (points <= 0)
                return;

            int result =
                (int)(points * ScoreMultiplier);

            Score += result;

            if (Score > HighScore)
            {
                HighScore = Score;
                SaveHighScore();
            }
        }

        public void ResetScore()
        {
            Score = 0;
            ResetScoreMultiplier();
        }

        public void IncreaseScoreMultiplier()
        {
            ScoreMultiplier += 0.2f;
        }

        public void SetScoreMultiplier(
            float newMultiplier)
        {
            if (newMultiplier > ScoreMultiplier)
            {
                ScoreMultiplier =
                    newMultiplier;
            }
        }

        public void ResetScoreMultiplier(
            float baseMultiplier = 1f)
        {
            ScoreMultiplier =
                Mathf.Max(1f, baseMultiplier);
        }

        private void SaveHighScore()
        {
            PlayerPrefs.SetInt(
                HighScoreKey,
                HighScore);

            PlayerPrefs.Save();
        }
    }
}