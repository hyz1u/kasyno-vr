using System;
using UnityEngine;

/// <summary>
/// Global player wallet. Saved in PlayerPrefs, so money earned in the kitchen
/// stays available in the casino and between game sessions.
/// </summary>
public static class MoneyManager
{
    const string SaveKey = "Casino_Money";

    public static event Action<int> MoneyChanged;

    public static int Money => PlayerPrefs.GetInt(SaveKey, 0);

    public static void Add(int amount)
    {
        if (amount <= 0) return;
        Set(Money + amount);
    }

    public static bool TrySpend(int amount)
    {
        if (amount < 0 || Money < amount) return false;
        Set(Money - amount);
        return true;
    }

    public static void ResetMoney() => Set(0);

    public static void SetMoney(int value) => Set(Mathf.Max(0, value));

    static void Set(int value)
    {
        PlayerPrefs.SetInt(SaveKey, value);
        PlayerPrefs.Save();
        MoneyChanged?.Invoke(value);
    }
}
