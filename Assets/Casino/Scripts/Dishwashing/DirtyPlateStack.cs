using UnityEngine;

/// <summary>
/// Spawns a stack of dirty plates and refills it a few seconds after the
/// player has taken every plate, so dishwashing can go on as long as needed.
/// </summary>
public class DirtyPlateStack : MonoBehaviour
{
    [SerializeField] Plate platePrefab;
    [SerializeField] int platesPerStack = 6;
    [Tooltip("Vertical distance between stacked plates (m).")]
    [SerializeField] float plateSpacing = 0.03f;
    [SerializeField] float refillDelay = 3f;
    [SerializeField] Vector2 dirtinessRange = new Vector2(0.8f, 1f);
    [Tooltip("Half size of the box used to check whether plates are still on the stack.")]
    [SerializeField] Vector3 checkHalfExtents = new Vector3(0.2f, 0.25f, 0.2f);

    const float CheckInterval = 0.5f;
    readonly Collider[] hits = new Collider[32];
    float nextCheck;
    float emptySince = -1f;

    void Start() => SpawnStack();

    void Update()
    {
        if (Time.time < nextCheck) return;
        nextCheck = Time.time + CheckInterval;

        if (HasPlatesOnStack())
        {
            emptySince = -1f;
            return;
        }

        if (emptySince < 0f) emptySince = Time.time;
        else if (Time.time - emptySince >= refillDelay)
        {
            SpawnStack();
            emptySince = -1f;
        }
    }

    bool HasPlatesOnStack()
    {
        var center = transform.position + transform.up * checkHalfExtents.y;
        int count = Physics.OverlapBoxNonAlloc(center, checkHalfExtents, hits, transform.rotation,
            ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; i++)
            if (hits[i].GetComponentInParent<Plate>() != null) return true;
        return false;
    }

    void SpawnStack()
    {
        if (platePrefab == null) return;
        for (int i = 0; i < platesPerStack; i++)
        {
            var pos = transform.position + transform.up * (0.015f + i * plateSpacing);
            var rot = transform.rotation * Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            var plate = Instantiate(platePrefab, pos, rot);
            plate.SetDirtiness(Random.Range(dirtinessRange.x, dirtinessRange.y));
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.matrix = Matrix4x4.TRS(transform.position + transform.up * checkHalfExtents.y,
            transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, checkHalfExtents * 2f);
    }
}
