using System.Collections;
using UnityEngine;

// Hnadles the spawing of the projectile for the enemy.
public class EnemyController : MonoBehaviour {
    public ProjectileController projectilePrefab;
    private GridManager gridManager;
    private Vector2Int gridPosition;

    public int mode = 0;

    // On start, find the enemy cell and store its position to make it inaccessible to the player.
    // Then spawns the projectile from the enemys cell.
    void Start() {
        gridManager = Object.FindFirstObjectByType<GridManager>();
        if (gridManager == null) return;

        var worldPos = transform.position;
        gridPosition = new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));
        gridManager.RegisterEnemyCell(gridPosition, this);

        StartCoroutine(SpawnProjectile());
    }

    // Once enemy cell is destroy the player can move to its cell.
    void OnDestroy() {
        if (gridManager != null)
            gridManager.UnregisterEnemyCell(gridPosition, this);
    }

    // Spawn projectile every 2 seconds on the enemy.
    // Takes the mode from projectile controller for the direction the projectile will spawn.
    // Disables gravity for the projection as 2D rigid body is added.
    private IEnumerator SpawnProjectile() {
        while (true) {
            yield return new WaitForSeconds(2);

            var parent = GameObject.Find("Enemy")?.transform;
            if (parent != null) {
                        
            ProjectileController p =
                Instantiate(projectilePrefab, transform.position, Quaternion.identity, parent);
                var rb = p.GetComponent<Rigidbody2D>();
                rb.gravityScale = 0f;
                p.mode           = mode;
            }
        }
    }
}
