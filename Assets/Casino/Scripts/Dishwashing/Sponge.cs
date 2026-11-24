using UnityEngine;

/// <summary>
/// Grabbable sponge. Dip it in the sink water to wet it, then rub it over a plate
/// to clean it. Needs a trigger collider (slightly bigger than the solid one)
/// to detect plates and the water.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Sponge : MonoBehaviour
{
    [Tooltip("Dirt removed per metre of scrubbing movement.")]
    [SerializeField] float scrubPerMetre = 0.8f;
    [Tooltip("Sponge must move at least this fast (m/s) to scrub.")]
    [SerializeField] float minScrubSpeed = 0.05f;
    [Tooltip("Faster movement doesn't scrub faster (stops instant cleaning when the sponge snaps into the hand).")]
    [SerializeField] float maxScrubSpeed = 1.2f;
    [SerializeField] bool requireWater = true;
    [Tooltip("How long (s) the sponge stays wet after leaving the water.")]
    [SerializeField] float wetDuration = 20f;
    [SerializeField] Renderer spongeRenderer;
    [SerializeField] Color dryColor = new Color(1f, 0.85f, 0.25f);
    [SerializeField] Color wetColor = new Color(0.75f, 0.58f, 0.10f);

    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    MaterialPropertyBlock block;
    MaterialPropertyBlock Block => block ??= new MaterialPropertyBlock();
    Vector3 lastPosition;
    float speed;
    float wetUntil = -1f;

    public bool IsWet => !requireWater || Time.time < wetUntil;

    void Awake()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        // Measured from the transform, so it works while held (kinematic) too.
        float dt = Mathf.Max(Time.deltaTime, 0.0001f);
        float instant = Mathf.Min((transform.position - lastPosition).magnitude / dt, maxScrubSpeed);
        speed = Mathf.Lerp(speed, instant, 0.5f);
        lastPosition = transform.position;

        if (spongeRenderer != null)
        {
            float wet = requireWater ? Mathf.Clamp01((wetUntil - Time.time) / wetDuration) : 1f;
            spongeRenderer.GetPropertyBlock(Block);
            Block.SetColor(BaseColorId, Color.Lerp(dryColor, wetColor, wet));
            spongeRenderer.SetPropertyBlock(Block);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.GetComponentInParent<SinkWater>() != null)
        {
            wetUntil = Time.time + wetDuration;
            return;
        }

        if (!IsWet || speed < minScrubSpeed) return;

        var plate = other.GetComponentInParent<Plate>();
        if (plate != null)
            plate.Scrub(speed * scrubPerMetre * Time.fixedDeltaTime);
    }
}
