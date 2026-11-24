using UnityEngine;

/// <summary>Marks a trigger volume as sink water. A sponge that touches it gets wet.</summary>
[RequireComponent(typeof(Collider))]
public class SinkWater : MonoBehaviour
{
    void Reset() => GetComponent<Collider>().isTrigger = true;
}
