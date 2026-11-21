using System;
using Game.Scripts.Gameplay.Core.Services;
using TMPro;
using UnityEngine;
using VContainer;

namespace Game.Scripts.Services.Score.View
{
    public class BestTodayScoreDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _bestTodayText;
        [SerializeField] private TextMeshProUGUI _diffText;
        [SerializeField] private GameObject _newGameobject;
        private ScoreService _pointsService;

        [Inject]
        public void Construct(ScoreService scoreService)
        {
            _pointsService = scoreService;
        }

        private void Start()
        {
            if (_bestTodayText == null || _diffText == null) return;
            var currentScore = 0;
            if (_pointsService != null) currentScore = Mathf.RoundToInt(_pointsService.Score);
            var date = DateTime.Now.ToString("yyyyMMdd");
            var bestKey = $"BestTodayScore_{date}";
            var storedBest = PlayerPrefs.GetInt(bestKey, -1);

            Debug.Log($"[BestToday] currentScore={currentScore}, storedBest={storedBest}");

            if (storedBest < 0)
            {
                _bestTodayText.text = currentScore.ToString();
                _diffText.text = "";
                if (_newGameobject != null) _newGameobject.SetActive(true);
                PlayerPrefs.SetInt(bestKey, currentScore);
                PlayerPrefs.Save();
            }
            else
            {
                if (currentScore > storedBest)
                {
                    var diff = currentScore - storedBest;
                    _bestTodayText.text = currentScore.ToString();
                    _diffText.text = $"+{diff}";
                    if (_newGameobject != null) _newGameobject.SetActive(false);
                    PlayerPrefs.SetInt(bestKey, currentScore);
                    PlayerPrefs.Save();
                }
                else
                {
                    _bestTodayText.text = storedBest.ToString();
                    _diffText.text = "";
                    if (_newGameobject != null) _newGameobject.SetActive(false);
                }
            }
        }
    }
}