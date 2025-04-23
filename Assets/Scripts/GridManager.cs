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
    [SerializeField] private Tilemap torchRoomCollisionTilemap;
    [SerializeField] private Tilemap torchDoorCollisionTilemap;
    [SerializeField] private Tilemap torchDoorTilemap;

    private PlayerController player;
    private readonly HashSet<Vector2Int> blockedCells = new();
    private readonly HashSet<Vector2Int> deadlyCells = new();
    private readonly HashSet<Vector2Int> doorCells = new();

    void Awake()
    {
        obstacleTilemap           ??= GameObject.Find("TilemapObstacles")?.GetComponent<Tilemap>();
        torchRoomCollisionTilemap ??= GameObject.Find("TilemapTorchRoomCollision")?.GetComponent<Tilemap>();
        torchDoorCollisionTilemap ??= GameObject.Find("TilemapTorchRoomDoorCollisions")?.GetComponent<Tilemap>();
        torchDoorTilemap          ??= GameObject.Find("TilemapTorchRoomDoor")?.GetComponent<Tilemap>();

        if (obstacleTilemap != null)
            CacheTiles(obstacleTilemap, blockedCells);

        if (torchRoomCollisionTilemap != null)
            CacheTiles(torchRoomCollisionTilemap, deadlyCells);

        if (torchDoorCollisionTilemap != null)
            CacheTiles(torchDoorCollisionTilemap, deadlyCells, doorCells);
    }

    void Start()
    {
        GenerateGrid();
        RespawnPlayer();
        cam.position = new Vector3(width * .5f - .5f, height * .5f - .5f, -10f);
    }

    private void CacheTiles(Tilemap map, HashSet<Vector2Int> primary, HashSet<Vector2Int> secondary = null)
    {
        foreach (var cell in map.cellBounds.allPositionsWithin)
        {
            if (map.GetTile(cell) == null) continue;
            var world = map.CellToWorld(cell) + map.tileAnchor;
            var gp = new Vector2Int(Mathf.RoundToInt(world.x), Mathf.RoundToInt(world.y));
            primary.Add(gp);
            secondary?.Add(gp);
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
        foreach (var tile in FindObjectsByType<Tile>(FindObjectsSortMode.None))
            tile.HideHighlight();
    }

    public void RespawnPlayer()
    {
        if (player != null) Destroy(player.gameObject);
        player = Instantiate(playerPrefab);
        player.Init(new Vector2Int(3, 4), this);
    }

    public void OpenTorchRoomDoor()
    {
        if (torchDoorTilemap != null)          torchDoorTilemap.gameObject.SetActive(false);
        if (torchDoorCollisionTilemap != null) torchDoorCollisionTilemap.gameObject.SetActive(false);
        foreach (var cell in doorCells)
            deadlyCells.Remove(cell);
    }

    public void CloseTorchRoomDoor()
    {
        if (torchDoorTilemap != null)          torchDoorTilemap.gameObject.SetActive(true);
        if (torchDoorCollisionTilemap != null)
        {
            torchDoorCollisionTilemap.gameObject.SetActive(true);
            CacheTiles(torchDoorCollisionTilemap, deadlyCells, doorCells);
        }
    }
}
