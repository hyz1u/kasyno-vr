using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Generic VR push button: poke it or point and pull the trigger, then <see cref="onPressed"/> fires.
/// </summary>
[RequireComponent(typeof(XRSimpleInteractable))]
public class PressButton : MonoBehaviour
{
    public UnityEvent onPressed;
    [Tooltip("Visual part that moves down when pressed.")]
    [SerializeField] Transform buttonCap;
    [SerializeField] float pressDepth = 0.01f;
    [SerializeField] float pressTime = 0.15f;

    XRSimpleInteractable interactable;
    Vector3 capRestPosition;
    float pressedUntil;

    void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        if (buttonCap != null) capRestPosition = buttonCap.localPosition;
    }

    void OnEnable() => interactable.selectEntered.AddListener(OnSelect);
    void OnDisable() => interactable.selectEntered.RemoveListener(OnSelect);

    void OnSelect(SelectEnterEventArgs args)
    {
        pressedUntil = Time.time + pressTime;
        onPressed?.Invoke();
    }

    void Update()
    {
        if (buttonCap == null) return;
        float depth = Time.time < pressedUntil ? pressDepth : 0f;
        buttonCap.localPosition = capRestPosition - Vector3.up * depth;
    }
}
