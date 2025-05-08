using UnityEngine;

// Vertical oscillatiion for objects or prefabs making them float.
// Only used in gems to make them float.
public class Hover : MonoBehaviour {
    [SerializeField] private float amplitude = 0.15f;
    [SerializeField] private float frequency = 1f;
    private Vector3 origin;

    void Awake() => origin = transform.position;

    void Update() {
        var offset = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = origin + Vector3.up * offset;
    }
}
