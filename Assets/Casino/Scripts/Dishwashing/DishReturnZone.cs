using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Drop-off area for washed plates. A clean plate released inside the trigger
/// is paid for and removed. A dirty plate makes the zone flash red.
/// </summary>
[RequireComponent(typeof(Collider))]
public class DishReturnZone : MonoBehaviour
{
    [SerializeField] int rewardPerPlate = 5;
    [SerializeField] Renderer indicator;
    [SerializeField] Color idleColor = new Color(0.15f, 0.45f, 0.2f);
    [SerializeField] Color acceptedColor = new Color(0.3f, 1f, 0.4f);
    [SerializeField] Color rejectedColor = new Color(1f, 0.2f, 0.15f);
    [SerializeField] float flashTime = 0.4f;

    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    MaterialPropertyBlock block;
    MaterialPropertyBlock Block => block ??= new MaterialPropertyBlock();
    float flashUntil;
    Color flashColor;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        SetIndicator(idleColor);
    }

    void Update()
    {
        if (indicator == null) return;
        if (Time.time < flashUntil)
            SetIndicator(Color.Lerp(idleColor, flashColor, (flashUntil - Time.time) / flashTime));
        else
            SetIndicator(idleColor);
    }

    void OnTriggerStay(Collider other)
    {
        var plate = other.GetComponentInParent<Plate>();
        if (plate == null || plate.Collected) return;

        // Wait until the player lets go of the plate.
        var grab = plate.GetComponent<XRGrabInteractable>();
        if (grab != null && grab.isSelected) return;

        if (plate.IsClean)
        {
            plate.Collected = true;
            MoneyManager.Add(rewardPerPlate);
            Flash(acceptedColor);
            Destroy(plate.gameObject);
        }
        else if (Time.time >= flashUntil)
        {
            Flash(rejectedColor);
        }
    }

    void Flash(Color color)
    {
        flashColor = color;
        flashUntil = Time.time + flashTime;
    }

    void SetIndicator(Color color)
    {
        if (indicator == null) return;
        indicator.GetPropertyBlock(Block);
        Block.SetColor(BaseColorId, color);
        indicator.SetPropertyBlock(Block);
    }
}
