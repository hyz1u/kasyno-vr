using System;
using UnityEngine;

/// <summary>
/// A plate that starts dirty and gets cleaned by scrubbing it with a <see cref="Sponge"/>.
/// The dirt level is shown by tinting the plate's renderers.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Plate : MonoBehaviour
{
    [Range(0f, 1f)] public float dirtiness = 1f;
    [Tooltip("Plate counts as clean at or below this dirt level.")]
    [SerializeField] float cleanThreshold = 0.03f;
    [SerializeField] Color dirtyColor = new Color(0.42f, 0.30f, 0.17f);
    [SerializeField] Color cleanColor = Color.white;
    [Tooltip("Renderers tinted by dirt. Empty = all child renderers.")]
    [SerializeField] Renderer[] tintedRenderers;

    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int ColorId = Shader.PropertyToID("_Color");

    MaterialPropertyBlock block;
    MaterialPropertyBlock Block => block ??= new MaterialPropertyBlock();

    public event Action<Plate> Cleaned;

    public bool IsClean => dirtiness <= cleanThreshold;

    /// <summary>Set once the plate has been handed in, so it is never paid for twice.</summary>
    public bool Collected { get; set; }

    void Awake()
    {
        if (tintedRenderers == null || tintedRenderers.Length == 0)
            tintedRenderers = GetComponentsInChildren<Renderer>();
        UpdateVisual();
    }

    public void SetDirtiness(float value)
    {
        dirtiness = Mathf.Clamp01(value);
        UpdateVisual();
    }

    public void Scrub(float amount)
    {
        if (IsClean || amount <= 0f) return;
        dirtiness = Mathf.Clamp01(dirtiness - amount);
        if (IsClean)
        {
            dirtiness = 0f;
            Cleaned?.Invoke(this);
        }
        UpdateVisual();
    }

    void UpdateVisual()
    {

        var color = Color.Lerp(cleanColor, dirtyColor, dirtiness);
        foreach (var r in tintedRenderers)
        {
            if (r == null) continue;
            r.GetPropertyBlock(Block);
            Block.SetColor(BaseColorId, color);
            Block.SetColor(ColorId, color);
            r.SetPropertyBlock(Block);
        }
    }

    void OnValidate()
    {
        if (Application.isPlaying) UpdateVisual();
    }
}
