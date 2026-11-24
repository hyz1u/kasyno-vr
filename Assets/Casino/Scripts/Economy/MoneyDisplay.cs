using TMPro;
using UnityEngine;

/// <summary>Shows the current wallet balance on a TextMeshPro label.</summary>
public class MoneyDisplay : MonoBehaviour
{
    [SerializeField] TMP_Text label;
    [SerializeField] string format = "Portfel: ${0}";

    void Awake()
    {
        if (label == null) label = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        MoneyManager.MoneyChanged += Refresh;
        Refresh(MoneyManager.Money);
    }

    void OnDisable() => MoneyManager.MoneyChanged -= Refresh;

    void Refresh(int money)
    {
        if (label != null) label.text = string.Format(format, money);
    }
}
