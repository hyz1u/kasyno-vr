using UnityEngine;

/// <summary>
/// Marquee bulbs around the slot machine. Slow chase while idle, fast chase while
/// the reels spin, gold flashing on a win and a rainbow strobe on three of a kind.
/// </summary>
public class SlotMachineLights : MonoBehaviour
{
    [SerializeField] SlotMachine machine;
    [SerializeField] Renderer[] bulbs;
    [SerializeField] Color idleColor = new Color(1f, 0.75f, 0.3f);
    [SerializeField] Color winColor = new Color(1f, 0.85f, 0.2f);
    [SerializeField] float emissionIntensity = 4f;
    [SerializeField] float idleChaseSpeed = 4f;
    [SerializeField] float spinChaseSpeed = 18f;
    [SerializeField] float winDuration = 2.5f;
    [SerializeField] float jackpotDuration = 5f;

    static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");
    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    MaterialPropertyBlock block;
    float celebrateUntil;
    bool jackpot;

    void Awake() => block = new MaterialPropertyBlock();

    void OnEnable()
    {
        if (machine == null) return;
        machine.SpinStarted += OnSpinStarted;
        machine.SpinFinished += OnSpinFinished;
    }

    void OnDisable()
    {
        if (machine == null) return;
        machine.SpinStarted -= OnSpinStarted;
        machine.SpinFinished -= OnSpinFinished;
    }

    void OnSpinStarted() => celebrateUntil = 0f;

    void OnSpinFinished(int win, bool triple)
    {
        if (win <= 0) return;
        jackpot = triple;
        celebrateUntil = Time.time + (triple ? jackpotDuration : winDuration);
    }

    void Update()
    {
        if (bulbs == null) return;
        float t = Time.time;
        bool spinning = machine != null && machine.IsSpinning;
        bool celebrating = t < celebrateUntil;

        for (int i = 0; i < bulbs.Length; i++)
        {
            if (bulbs[i] == null) continue;
            Color c;
            if (celebrating && jackpot)
                c = Color.HSVToRGB(Mathf.Repeat(t * 1.5f + i * 0.07f, 1f), 0.9f, 1f);
            else if (celebrating)
                c = Mathf.Repeat(t * 8f, 1f) < 0.5f ? winColor : winColor * 0.1f;
            else
            {
                float speed = spinning ? spinChaseSpeed : idleChaseSpeed;
                bool lit = Mathf.FloorToInt(t * speed + i) % 3 == 0;
                c = lit ? idleColor : idleColor * 0.12f;
            }

            bulbs[i].GetPropertyBlock(block);
            block.SetColor(BaseColorId, c);
            block.SetColor(EmissionId, c * emissionIntensity);
            bulbs[i].SetPropertyBlock(block);
        }
    }
}
