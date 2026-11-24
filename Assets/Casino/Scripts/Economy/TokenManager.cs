using System;
using UnityEngine;

/// <summary>
/// Casino tokens, bought with money at the exchange machine and used on the slot machines.
/// Saved in PlayerPrefs alongside <see cref="MoneyManager"/>.
/// </summary>
public static class TokenManager
{
    const string SaveKey = "Casino_Tokens";

    public static event Action<int> TokensChanged;

    public static int Tokens => PlayerPrefs.GetInt(SaveKey, 0);

    public static void Add(int amount)
    {
        if (amount <= 0) return;
        Set(Tokens + amount);
    }

    public static bool TrySpend(int amount)
    {
        if (amount < 0 || Tokens < amount) return false;
        Set(Tokens - amount);
        return true;
    }

    public static void SetTokens(int value) => Set(Mathf.Max(0, value));

    static void Set(int value)
    {
        PlayerPrefs.SetInt(SaveKey, value);
        PlayerPrefs.Save();
        TokensChanged?.Invoke(value);
    }
}
