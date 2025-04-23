// GridManager.cs
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
    [SerializeField] private Tilemap torchRoomTilemap;
    [SerializeField] private Tilemap torchDoorTilemap;

    private PlayerController player;
    private readonly HashSet<Vector2Int> blockedCells = new();
    private readonly HashSet<Vector2Int> deadlyCells = new();

    void Awake()
    {
        obstacleTilemap    ??= GameObject.Find("TilemapObstacles")?.GetComponent<Tilemap>();
        torchRoomTilemap   ??= GameObject.Find("TilemapTorchRoom")?.GetComponent<Tilemap>();
        torchDoorTilemap   ??= GameObject.Find("TilemapTorchDoor")?.GetComponent<Tilemap>();

        if (obstacleTilemap != null) CacheTiles(obstacleTilemap, blockedCells);
        if (torchRoomTilemap != null) CacheTiles(torchRoomTilemap, deadlyCells);
        if (torchDoorTilemap != null) CacheTiles(torchDoorTilemap, deadlyCells);
    }

    void Start()
    {
        GenerateGrid();
        RespawnPlayer();
        cam.position = new Vector3(width * 0.5f - 0.5f, height * 0.5f - 0.5f, -10f);
    }

    private void CacheTiles(Tilemap map, HashSet<Vector2Int> set)
    {
        var bounds = map.cellBounds;
        foreach (var cell in bounds.allPositionsWithin)
        {
            if (map.GetTile(cell) == null) continue;
            var worldPos = map.CellToWorld(cell) + map.tileAnchor;
            var gp = new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));
            set.Add(gp);
        }
    }

    private void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                var t = Instantiate(tilePrefab, new Vector3(x, y), quaternion.identity);
                t.name = $"Tile {x} {y}";
                t.Init(((x % 2 == 0) ^ (y % 2 == 0)), new Vector2Int(x, y));
            }
    }

    public bool IsPositionValid(Vector2Int pos) =>
        pos.x >= 0 && pos.x < width &&
        pos.y >= 0 && pos.y < height &&
        !blockedCells.Contains(pos);

    public bool IsDeadly(Vector2Int pos) =>
        deadlyCells.Contains(pos);

    public void ClearHighlights()
    {
        var tiles = Object.FindObjectsByType<Tile>(FindObjectsSortMode.None);
        foreach (var tile in tiles)
            tile.HideHighlight();
    }


    public void RespawnPlayer()
    {
        if (player != null)
            Destroy(player.gameObject);
        player = Instantiate(playerPrefab);
        player.Init(new Vector2Int(3, 4), this);
    }
}
