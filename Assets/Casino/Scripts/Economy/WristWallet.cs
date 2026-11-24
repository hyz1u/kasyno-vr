using TMPro;
using UnityEngine;

/// <summary>
/// Small money + token readout hanging below a controller. Turns to face the headset so it
/// stays readable from any wrist angle.
/// </summary>
public class WristWallet : MonoBehaviour
{
    [SerializeField] TMP_Text label;
    [Tooltip("Headset camera; falls back to Camera.main.")]
    [SerializeField] Transform head;
    [Tooltip("Controller the panel hangs from; falls back to the parent.")]
    [SerializeField] Transform anchor;
    [Tooltip("World-space offset from the anchor, so the panel stays below the hand at any controller tilt.")]
    [SerializeField] Vector3 worldOffset = new Vector3(0f, -0.19f, 0f);

    void Awake()
    {
        if (label == null) label = GetComponentInChildren<TMP_Text>();
        if (head == null && Camera.main != null) head = Camera.main.transform;
        if (anchor == null) anchor = transform.parent;
    }

    void OnEnable()
    {
        MoneyManager.MoneyChanged += OnChanged;
        TokenManager.TokensChanged += OnChanged;
        Refresh();
    }

    void OnDisable()
    {
        MoneyManager.MoneyChanged -= OnChanged;
        TokenManager.TokensChanged -= OnChanged;
    }

    void OnChanged(int _) => Refresh();

    void Refresh()
    {
        if (label != null)
            label.text = "<color=#7CFC7C>$" + MoneyManager.Money + "</color>\n<color=#FFD24A>" + TokenManager.Tokens + " żet.</color>";
    }

    void LateUpdate()
    {
        if (anchor != null) transform.position = anchor.position + worldOffset;
        if (head == null) return;
        Vector3 away = transform.position - head.position;
        if (away.sqrMagnitude > 1e-6f) transform.rotation = Quaternion.LookRotation(away, head.up);
    }
}
