using Game.Scripts.Gameplay.Core.Services;
using TMPro;
using UnityEngine;

namespace Game.Scripts.Services.Score.View
{
    public class AverageScoreDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _averageScoreText;
        [SerializeField] private TextMeshProUGUI _diffText;
        [SerializeField] private GameObject _newGameobject;

        private void Start()
        {
            int average = LocalScoreStorage.AverageScore;
            int previous = PlayerPrefs.GetInt("Portfolio_PreviousAverageScore", -1);
            if (_averageScoreText != null)
                _averageScoreText.text = average.ToString();
            if (_diffText != null)
                _diffText.text = previous >= 0 && average > previous ? $"+{average - previous}" : "";
            if (_newGameobject != null)
                _newGameobject.SetActive(previous < 0);
            PlayerPrefs.SetInt("Portfolio_PreviousAverageScore", average);
            PlayerPrefs.Save();
        }
    }
}
