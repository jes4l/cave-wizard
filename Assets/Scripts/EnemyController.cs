using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private GridManager gridManager;
    private Vector2Int gridPosition;

    void Start()
    {
        gridManager = Object.FindFirstObjectByType<GridManager>();
        if (gridManager == null) return;

        var worldPos = transform.position;
        gridPosition = new Vector2Int(
            Mathf.RoundToInt(worldPos.x),
            Mathf.RoundToInt(worldPos.y)
        );
        gridManager.RegisterEnemyCell(gridPosition);
    }

    void OnDestroy()
    {
        if (gridManager != null)
            gridManager.UnregisterEnemyCell(gridPosition);
    }
}
