using UnityEngine;

public class EnemySpawner : MonoBehaviour {
    [SerializeField] private GameObject enemyPrefab;
    Vector3?[,] spawnPoints = {{null, null, null}, 
                               {null, null, null},
                               {new Vector3(10, 3, -1), new Vector3(8, 2, -1), null},
                               {new Vector3(6, 8, -1), new Vector3(14, 4, -1), new Vector3(1, 4, -1)},
                               {new Vector3(7, 7, -1), new Vector3(8, 1, -1), null}};

    // -1. x 0.> 1.< 2.^ 3.v 4.[o]                          
    int[,] modes = {{-1, -1, -1}, {-1, -1, -1}, {0, 1, -1}, {3, 1, 0}, {3, 4, -1}};

    void Start() {
        ResetEnemies();
    }

    public void ResetEnemies() {
        if (enemyPrefab == null) return;
        var parent = GameObject.Find("TilemapObstacles")?.transform;
        if (parent == null) return;

        for (int i = parent.childCount - 1; i >= 0; i--) {
            var child = parent.GetChild(i);
            if (child.GetComponent<EnemyController>() != null)
                Destroy(child.gameObject);
        }

        int level = GridManager.levelNumber;
        int cols  = spawnPoints.GetLength(1);

        for (int j = 0; j < cols; j++) {
            if (spawnPoints[level, j] is Vector3 pos) {
                var enemy = Instantiate(enemyPrefab, pos, Quaternion.identity, parent);
                enemy.GetComponent<EnemyController>().mode = modes[level, j];
            }
        }
    }

}
