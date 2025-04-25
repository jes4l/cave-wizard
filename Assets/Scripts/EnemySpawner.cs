using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    private static readonly Vector3 SpawnPoint = new Vector3(13f, 2f, -1f);

    void Start()
    {
        if (enemyPrefab == null) return;
        var parent = GameObject.Find("TilemapObstacles")?.transform;
        if (parent == null) return;

        Instantiate(enemyPrefab, SpawnPoint, Quaternion.identity, parent);
    }
}
