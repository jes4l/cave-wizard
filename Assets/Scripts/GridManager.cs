using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int width, height;
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private Transform cam;
    [SerializeField] private PlayerController playerPrefab;
    [SerializeField] private Tilemap obstacleTilemap;

    private PlayerController player;
    private readonly List<Tile> tiles = new();
    private readonly HashSet<Vector2Int> blockedCells = new();

    void Awake() 
    { 
        obstacleTilemap ??= GameObject.Find("TilemapObstacles")?.GetComponent<Tilemap>(); 
        if (obstacleTilemap != null) CacheObstacles();
    }

    void Start()
    { 
        GenerateGrid(); player = Instantiate(playerPrefab); 
        player.Init(new Vector2Int(3, 4), this); 
        cam.position = new Vector3(width / 2f - .5f, height / 2f - .5f, -10); 
    }

    private void CacheObstacles()
    {
        var bounds = obstacleTilemap.cellBounds;
        foreach (var cell in bounds.allPositionsWithin)
            if (obstacleTilemap.GetTile(cell) != null)
            {
                var w = obstacleTilemap.CellToWorld(cell) + obstacleTilemap.tileAnchor;
                var gp = new Vector2Int(Mathf.RoundToInt(w.x), Mathf.RoundToInt(w.y));
                blockedCells.Add(gp);
                Debug.Log($"Blocked grid cell: {gp.x}, {gp.y}");
            }
    }

    private void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                var t = Instantiate(tilePrefab, new Vector3(x, y), quaternion.identity);
                t.name = $"Tile {x} {y}";
                t.Init((x % 2 == 0) ^ (y % 2 == 0), new Vector2Int(x, y));
                tiles.Add(t);
            }
    }

    public bool IsPositionValid(Vector2Int pos) =>
        pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height && !blockedCells.Contains(pos);

    public void ClearHighlights() => tiles.ForEach(t => t.HideHighlight());
}
