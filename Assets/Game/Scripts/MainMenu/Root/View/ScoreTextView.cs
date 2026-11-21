using Game.Scripts.Services.Money;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;

namespace Game.Scripts.MainMenu.Root.View
{
    public class ScoreTextView : MonoBehaviour
    {
        [Header("All-Time Points")]
        [SerializeField] private TMP_Text _scoreText;

        [Header("Money")]
        [FormerlySerializedAs(
            "_battlePassPointsText")]
        [SerializeField] private TMP_Text _moneyText;

        private MoneyService _moneyService;
        private BattlePassService _battlePassService;

        private bool _moneySubscribed;
        private bool _battlePassSubscribed;

        [Inject]
        public void Construct(
            MoneyService moneyService,
            BattlePassService battlePassService)
        {
            Unsubscribe();

            _moneyService = moneyService;
            _battlePassService =
                battlePassService;

            RefreshTexts();

            if (isActiveAndEnabled)
            {
                Subscribe();
            }
        }

        private void OnEnable()
        {
            RefreshTexts();
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (_battlePassService != null &&
                !_battlePassSubscribed)
            {
                _battlePassService.OnPointsChanged +=
                    HandleAllTimePointsChanged;

                _battlePassSubscribed = true;
            }

            if (_moneyService != null &&
                !_moneySubscribed)
            {
                _moneyService.OnMoneyChanged +=
                    HandleMoneyChanged;

                _moneySubscribed = true;
            }
        }

        private void Unsubscribe()
        {
            if (_battlePassService != null &&
                _battlePassSubscribed)
            {
                _battlePassService.OnPointsChanged -=
                    HandleAllTimePointsChanged;

                _battlePassSubscribed = false;
            }

            if (_moneyService != null &&
                _moneySubscribed)
            {
                _moneyService.OnMoneyChanged -=
                    HandleMoneyChanged;

                _moneySubscribed = false;
            }
        }

        private void RefreshTexts()
        {
            if (_battlePassService != null)
            {
                HandleAllTimePointsChanged(
                    _battlePassService.Points);
            }

            if (_moneyService != null)
            {
                HandleMoneyChanged(
                    _moneyService.Money);
            }
        }

        private void HandleAllTimePointsChanged(
            int points)
        {
            if (_scoreText == null)
                return;

            _scoreText.text =
                points.ToString("N0");
        }

        private void HandleMoneyChanged(
            int money)
        {
            if (_moneyText == null)
                return;

            _moneyText.text =
                money.ToString("N0");
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }
    }
}