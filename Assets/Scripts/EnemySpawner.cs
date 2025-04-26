using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    Vector3?[,] spawnPoints = {{null, null}, 
                               {null, null},
                               {new Vector3(10, 3, -1), new Vector3(8, 2, -1)},
                               {new Vector3(6, 8, -1), new Vector3(14, 4, -1)},
                               {new Vector3(13, 2, -1), new Vector3(10, 2, -1)}};

    // -1. x 0.> 1.< 2.^ 3.v                           
    int[,] modes = {{-1, -1}, // level 1
                   {-1, -1}, // level 2
                   {0, 1}, // level  3
                   {3, 1}, // level 4
                   {-1, -1}}; // level 5

    void Start()
    {
        if (enemyPrefab == null) return;
        var parent = GameObject.Find("TilemapObstacles")?.transform;
        if (parent == null) return;

        if (spawnPoints[GridManager.levelNumber, 0] is Vector3 spawn)
        {        
            GameObject e =
                Instantiate(enemyPrefab, spawn, Quaternion.identity, parent);
            e.GetComponent<EnemyController>().mode = modes[GridManager.levelNumber, 0];
        }
        
        if (spawnPoints[GridManager.levelNumber, 1] is Vector3 spawn2)
        {        
            GameObject e =
                Instantiate(enemyPrefab, spawn2, Quaternion.identity, parent);
            e.GetComponent<EnemyController>().mode = modes[GridManager.levelNumber, 1];
        }
    }
}
