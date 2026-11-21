using DG.Tweening;
using Game.Scripts.Gameplay.Core.Services;
using TMPro;
using UnityEngine;
using VContainer;

namespace Game.Scripts.Gameplay.View
{
    public class ScoreView : MonoBehaviour
    {
        [Inject] private ScoreService _scoreService;
        
        [SerializeField] private TextMeshProUGUI _scoreText;

        private Tween _countTween;
        private int _currentDisplayedScore;

        private void Start()
        {
            if (_scoreService != null)
            {
                _scoreService.OnScoreChanged += OnScoreChanged;
                UpdateScoreText(_scoreService.Score);
            }
            else
            {
                ApplyText(0);
            }
        }
        
        private void OnScoreChanged(int newScore)
        {
            UpdateScoreText(newScore);
        }

        private void UpdateScoreText(int newScore)
        {
            if (_scoreText == null) return;

            _countTween?.Kill();

            int start = _currentDisplayedScore;
            int target = newScore;

            _countTween = DOVirtual.Int(start, target, 0.5f, val =>
            {
                _scoreText.text = val.ToString();
            }).SetEase(Ease.OutCubic).OnComplete(() => _currentDisplayedScore = target);
        }

        private void ApplyText(int score)
        {
            if (_scoreText == null) return;
            _scoreText.text = score.ToString();
            _currentDisplayedScore = score;
        }

        private string FormatScore(int score)
        {
            if (score < 1000)
            {
                return score.ToString();
            }
            else if (score < 1000000)
            {
                float kScore = score / 1000f;
                return kScore % 1 == 0 ? $"{(int)kScore}k" : $"{kScore:F1}k";
            }
            else
            {
                float mScore = score / 1000000f;
                return mScore % 1 == 0 ? $"{(int)mScore}m" : $"{mScore:F1}m";
            }
        }

        private void OnDestroy()
        {
            if (_scoreService != null)
            {
                _scoreService.OnScoreChanged -= OnScoreChanged;
            }
            _countTween?.Kill();
        }
    }
}