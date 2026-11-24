using UnityEngine;

/// <summary>
/// TESTING HELPER: sets the wallet and tokens to fixed amounts every time the game starts.
/// Disable or remove this component once money should carry over between sessions.
/// </summary>
[DefaultExecutionOrder(-100)]
public class TestStartingMoney : MonoBehaviour
{
    [SerializeField] int startingMoney = 100;
    [SerializeField] int startingTokens = 0;

    void Awake()
    {
        MoneyManager.SetMoney(startingMoney);
        TokenManager.SetTokens(startingTokens);
    }
}
