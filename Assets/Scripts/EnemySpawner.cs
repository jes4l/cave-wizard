using UnityEngine;
using UnityEngine.UIElements;

// Manaages where the enemies spawn and the projectile modes it spawns with.
public class EnemySpawner : MonoBehaviour {
    [SerializeField] private GameObject enemyPrefab;

    // 2D array for the co ordinates on the grid for the enemies.
    // null doesnt spawn any.
    Vector3?[,] spawnPoints = {{null, null, null}, 
                               {null, null, null},
                               {new Vector3(8, 8, -1), new Vector3(6, 2, -1), null},
                               {new Vector3(6, 8, -1), new Vector3(14, 4, -1), new Vector3(1, 3, -1)},
                               {new Vector3(7, 8, -1), new Vector3(4, 0, -1), new Vector3(8, 3, -1)}};

    // The modes for each enemy 
    // -1. x 0.> 1.< 2.^ 3.v 4.[o]                          
    int[,] modes = {{-1, -1, -1}, {-1, -1, -1}, {3, 1, -1}, {4, 1, 0}, {4, 4, 2}};

    // Resets the enemy locations at the beginning.
    // This is so if the level changes it resets co ordinates.
    void Start() {
        ResetEnemies();
    }
    
    // Resets enemy position when the reset button is clicked.
    // Deletes before it spawns the enemies back.
    public void ResetEnemies() {
        if (enemyPrefab == null) return;
        var parent = GameObject.Find("Enemy")?.transform;
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
