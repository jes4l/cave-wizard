using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

[RequireComponent(typeof(Tilemap))]
public class ObstacleFinder : MonoBehaviour
{
    private Tilemap tilemap;
    private List<Vector3> obstaclePositions = new List<Vector3>();
    public IReadOnlyList<Vector3> ObstaclePositions => obstaclePositions;

    void Awake()
    {
        tilemap = GetComponent<Tilemap>();
        BoundsInt bounds = tilemap.cellBounds;

        foreach (Vector3Int cell in bounds.allPositionsWithin)
        {
            if (tilemap.GetTile(cell) != null)
            {
                Vector3 worldPos = tilemap.CellToWorld(cell) + tilemap.tileAnchor;
                obstaclePositions.Add(worldPos);
                Debug.Log($"Obstacle at: {worldPos}");
            }
        }
    }
}
