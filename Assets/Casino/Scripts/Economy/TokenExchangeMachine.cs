using TMPro;
using UnityEngine;

/// <summary>
/// Cashier machine: buys tokens with money and cashes tokens back into money at a fixed rate.
/// Hook its public methods up to <see cref="PressButton"/> events.
/// </summary>
public class TokenExchangeMachine : MonoBehaviour
{
    [Tooltip("Dollars per token, both when buying and when cashing out.")]
    [Min(1)] [SerializeField] int dollarsPerToken = 1;

    [Header("Displays (optional)")]
    [SerializeField] TMP_Text statusLabel;
    [SerializeField] TMP_Text balanceLabel;
    [SerializeField] TMP_Text rateLabel;

    static readonly Color OkColor = new Color(0.55f, 1f, 0.55f);
    static readonly Color ErrorColor = new Color(1f, 0.3f, 0.25f);
    static readonly Color IdleColor = new Color(0.85f, 0.85f, 0.85f);

    void Awake()
    {
        if (rateLabel != null) rateLabel.text = "1 żeton = $" + dollarsPerToken;
        Show("Wybierz opcję", IdleColor);
    }

    void OnEnable()
    {
        MoneyManager.MoneyChanged += OnBalanceChanged;
        TokenManager.TokensChanged += OnBalanceChanged;
        RefreshBalance();
    }

    void OnDisable()
    {
        MoneyManager.MoneyChanged -= OnBalanceChanged;
        TokenManager.TokensChanged -= OnBalanceChanged;
    }

    public void BuyTokens(int tokens)
    {
        if (tokens <= 0) return;
        int cost = tokens * dollarsPerToken;
        if (!MoneyManager.TrySpend(cost))
        {
            Show("Za mało pieniędzy! Potrzebujesz $" + cost, ErrorColor);
            return;
        }
        TokenManager.Add(tokens);
        Show("Kupiono " + tokens + " żet. za $" + cost, OkColor);
    }

    public void CashOutAll()
    {
        int tokens = TokenManager.Tokens;
        if (tokens <= 0)
        {
            Show("Nie masz żetonów", ErrorColor);
            return;
        }
        TokenManager.TrySpend(tokens);
        int payout = tokens * dollarsPerToken;
        MoneyManager.Add(payout);
        Show("Wypłacono $" + payout, OkColor);
    }

    void OnBalanceChanged(int _) => RefreshBalance();

    void RefreshBalance()
    {
        if (balanceLabel != null)
            balanceLabel.text = "Portfel: $" + MoneyManager.Money + "\nŻetony: " + TokenManager.Tokens;
    }

    void Show(string message, Color color)
    {
        if (statusLabel == null) return;
        statusLabel.text = message;
        statusLabel.color = color;
    }
}
