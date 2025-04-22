using UnityEngine;

public class Hover : MonoBehaviour
{
    [SerializeField] private float amplitude = 0.15f;
    [SerializeField] private float frequency = 1f;
    private Vector3 _origin;

    void Awake() => _origin = transform.position;

    void Update()
    {
        var offset = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = _origin + Vector3.up * offset;
    }
}
