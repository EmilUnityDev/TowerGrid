using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Gameplay.Core.Services
{
    public class TimerComponent : MonoBehaviour
    {
        [Header("Настройки таймера")]
        [SerializeField] private float _durationMultiGame = 120f;
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private Text _timerLegacyText;

        public event Action OnCompleted;

        private Tween _fillTween;
        private float _remaining;
        private bool _running;
        private float _duration;


        private void Start()
        {
            _duration = _durationMultiGame;
            _remaining = _duration;
            UpdateDisplay(_remaining);
        }

        private void Update()
        {
            if (!_running) return;

            _remaining -= Time.deltaTime;
            if (_remaining <= 0f)
            {
                _remaining = 0f;
                _running = false;
                OnTimerComplete();
            }
            UpdateDisplay(_remaining);
        }

        private void UpdateDisplay(float time)
        {
            var minutes = Mathf.FloorToInt(time / 60f);
            var seconds = Mathf.FloorToInt(time % 60f);

            if (_timerText != null)
                _timerText.text = $"{minutes:00}:{seconds:00}";
            else if (_timerLegacyText != null)
                _timerLegacyText.text = $"{minutes:00}:{seconds:00}";
        }

        public void StartTimer()
        {
            _running = true;

            _fillTween?.Kill();
        }

        public void PauseTimer()
        {
            _running = false;
            _fillTween?.Pause();
        }

        public void StopTimer()
        {
            _running = false;
            _fillTween?.Kill();
            _fillTween = null;
        }

        public void ResetTimer()
        {
            _running = false;
            _remaining = _duration;
            UpdateDisplay(_remaining);

            _fillTween?.Kill();
        }

        public void SetDuration(float seconds)
        {
            _duration = Mathf.Max(0f, seconds);
            _remaining = _duration;
            
            UpdateDisplay(_remaining);
        }

        private void OnTimerComplete()
        {
            OnCompleted?.Invoke();
        }

        private void OnDestroy()
        {
            _fillTween?.Kill();
        }
    }
}