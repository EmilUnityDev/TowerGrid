using System;
using UnityEngine;

namespace Game.Scripts.Services.Money
{
    public class MoneyService
    {
        private const string MoneyKey = "MONEY";

        private const string LegacyScoreKey = "Score";

        private int _money;

        public int Money => _money;

        public event Action<int> OnMoneyChanged;

        public MoneyService()
        {
            Load();
        }

        public void AddMoney(int amount)
        {
            if (amount <= 0)
                return;

            long result =
                (long)_money + amount;

            _money = (int)Math.Min(
                result,
                int.MaxValue);

            Save();
            OnMoneyChanged?.Invoke(_money);
        }

        public bool CanAfford(int cost)
        {
            return cost >= 0 &&
                   _money >= cost;
        }

        public bool SpendMoney(int cost)
        {
            if (!CanAfford(cost))
                return false;

            _money -= cost;

            Save();
            OnMoneyChanged?.Invoke(_money);

            return true;
        }

        private void Load()
        {
            if (PlayerPrefs.HasKey(MoneyKey))
            {
                _money = Mathf.Max(
                    0,
                    PlayerPrefs.GetInt(MoneyKey));

                return;
            }
            
            if (PlayerPrefs.HasKey(LegacyScoreKey))
            {
                _money = Mathf.Max(
                    0,
                    PlayerPrefs.GetInt(
                        LegacyScoreKey));

                PlayerPrefs.SetInt(
                    MoneyKey,
                    _money);

                PlayerPrefs.DeleteKey(
                    LegacyScoreKey);

                PlayerPrefs.Save();

                return;
            }

            _money = 0;
        }

        private void Save()
        {
            PlayerPrefs.SetInt(
                MoneyKey,
                _money);

            PlayerPrefs.Save();
        }
    }
}