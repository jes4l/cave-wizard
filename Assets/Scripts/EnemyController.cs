using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour {
    public ProjectileController projectilePrefab;
    private GridManager gridManager;
    private Vector2Int gridPosition;

    public int mode = 0;

    void Start() {
        gridManager = Object.FindFirstObjectByType<GridManager>();
        if (gridManager == null) return;

        var worldPos = transform.position;
        gridPosition = new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));
        gridManager.RegisterEnemyCell(gridPosition, this);

        StartCoroutine(SpawnProjectile());
    }

    void OnDestroy() {
        if (gridManager != null)
            gridManager.UnregisterEnemyCell(gridPosition, this);
    }

    private IEnumerator SpawnProjectile() {
        while (true) {
            yield return new WaitForSeconds(2);

            var parent = GameObject.Find("TilemapObstacles")?.transform;
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
