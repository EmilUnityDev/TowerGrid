using System;
using TMPro;
using UnityEngine;

public class ShopWallet : MonoBehaviour
{
    private const string MoneyKey = "SHOP_MONEY";

    [Header("Settings")]
    [SerializeField] private int startMoney = 123456;

    [Header("UI")]
    [SerializeField] private TMP_Text moneyText;

    public int Money { get; private set; }

    public event Action<int> OnMoneyChanged;

    private void Awake()
    {
        Money = PlayerPrefs.GetInt(MoneyKey, startMoney);
        RefreshView();
    }

    public bool HasEnoughMoney(int amount)
    {
        return Money >= amount;
    }

    public bool TrySpend(int amount)
    {
        if (amount < 0)
        {
            Debug.LogError("ShopWallet: сумма покупки не может быть отрицательной.");
            return false;
        }

        if (!HasEnoughMoney(amount))
            return false;

        Money -= amount;
        Save();

        return true;
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0)
            return;

        Money += amount;
        Save();
    }

    public void SetMoney(int amount)
    {
        Money = Mathf.Max(0, amount);
        Save();
    }

    private void Save()
    {
        PlayerPrefs.SetInt(MoneyKey, Money);
        PlayerPrefs.Save();

        RefreshView();
        OnMoneyChanged?.Invoke(Money);
    }

    private void RefreshView()
    {
        if (moneyText != null)
            moneyText.text = Money.ToString("N0");
    }

    [ContextMenu("Add 1000 Money")]
    private void DebugAddMoney()
    {
        AddMoney(1000);
    }

    [ContextMenu("Clear Money Save")]
    private void DebugClearMoney()
    {
        PlayerPrefs.DeleteKey(MoneyKey);

        Money = startMoney;
        Save();
    }
}