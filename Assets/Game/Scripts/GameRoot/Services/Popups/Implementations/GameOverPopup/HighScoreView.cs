using Game.Scripts.Gameplay.Core.Services;
using TMPro;
using UnityEngine;

namespace Game.Scripts.Services.Score.View
{
    public class HighScoreView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _highScoreText;
        [SerializeField] private TextMeshProUGUI _diffText;
        [SerializeField] private GameObject _newGameobject;

        private void Start()
        {
            int best = LocalScoreStorage.BestScore;
            int previous = LocalScoreStorage.PreviousBestScore;
            if (_highScoreText != null)
                _highScoreText.text = best.ToString();
            if (_diffText != null)
                _diffText.text = best > previous && previous > 0 ? $"+{best - previous}" : "";
            if (_newGameobject != null)
                _newGameobject.SetActive(LocalScoreStorage.MatchCount == 1);
        }
    }
}
