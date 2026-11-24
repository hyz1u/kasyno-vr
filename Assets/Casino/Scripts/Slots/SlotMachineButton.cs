using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// SPIN button. Works with a poke (pressing it with a finger or controller tip)
/// and with a ray/direct select (pointing at it and pulling the trigger).
/// </summary>
[RequireComponent(typeof(XRSimpleInteractable))]
public class SlotMachineButton : MonoBehaviour
{
    [SerializeField] SlotMachine machine;
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

    void OnEnable() => interactable.selectEntered.AddListener(OnPressed);
    void OnDisable() => interactable.selectEntered.RemoveListener(OnPressed);

    void OnPressed(SelectEnterEventArgs args)
    {
        pressedUntil = Time.time + pressTime;
        if (machine != null) machine.TrySpin();
    }

    void Update()
    {
        if (buttonCap == null) return;
        float depth = Time.time < pressedUntil ? pressDepth : 0f;
        buttonCap.localPosition = capRestPosition - Vector3.up * depth;
    }
}
