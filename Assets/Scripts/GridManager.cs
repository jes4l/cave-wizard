using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class GridManager : MonoBehaviour {
    [SerializeField] private int width, height;
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private Transform cam;
    [SerializeField] private PlayerController playerPrefab;
    [SerializeField] private Tilemap obstacleTilemap;
    [SerializeField] private Tilemap torchRoomCollisionTilemap;
    [SerializeField] private Tilemap torchDoorCollisionTilemap;
    [SerializeField] private Tilemap torchDoorTilemap;
    [SerializeField] private TextMeshProUGUI energyText;

    [SerializeField] private Tilemap buttonTilemap;
    private Vector2Int buttonPosition;

    [SerializeField] private Tilemap torchTilemap;
    private Vector2Int torchPosition;

    [SerializeField] private GemManager gemManager;

    private PlayerController player;
    private readonly HashSet<Vector2Int> blockedCells = new();
    private readonly HashSet<Vector2Int> deadlyCells = new();
    private readonly HashSet<Vector2Int> doorCells = new();
    private readonly HashSet<Vector2Int> enemyCells = new();

    private List<PlayerController> ghosts = new List<PlayerController>();
    private List<PlayerController> ghostsOld = new List<PlayerController>();

    public Vector2Int GetButtonPosition() => buttonPosition;
    public Vector2Int GetTorchPosition()  => torchPosition;

    private int[,] spawnPoints = {{3, 4}, {13, 0}, {8,0}};
    public static int levelNumber = 0;

    public event Action OnTorchRoomDoorOpened;
    public event Action OnTorchRoomDoorClosed;

    void Awake() {
        gemManager ??= GameObject.Find("GemManager")?.GetComponent<GemManager>();

        obstacleTilemap           ??= GameObject.Find("TilemapObstacles")?.GetComponent<Tilemap>();
        torchRoomCollisionTilemap ??= GameObject.Find("TilemapTorchRoomCollision")?.GetComponent<Tilemap>();
        torchDoorCollisionTilemap ??= GameObject.Find("TilemapTorchRoomDoorCollisions")?.GetComponent<Tilemap>();
        torchDoorTilemap          ??= GameObject.Find("TilemapTorchRoomDoor")?.GetComponent<Tilemap>();
        buttonTilemap             ??= GameObject.Find("TilemapButton")?.GetComponent<Tilemap>();
        torchTilemap              ??= GameObject.Find("TilemapTorch")?.GetComponent<Tilemap>();

        if (obstacleTilemap != null)
            CacheTiles(obstacleTilemap, blockedCells);

        if (torchRoomCollisionTilemap != null)
            CacheTiles(torchRoomCollisionTilemap, deadlyCells);

        if (torchDoorCollisionTilemap != null)
            CacheTiles(torchDoorCollisionTilemap, deadlyCells, doorCells);

        if (buttonTilemap != null)
            CacheButtonPosition(buttonTilemap);

        if (torchTilemap != null)
            CacheSingleTilePosition(torchTilemap, out torchPosition);
    }

    void Start() {
        GenerateGrid();
        RespawnPlayer();
        //cam.position = new Vector3(width * .5f - .5f, height * .5f - .5f, -10f);
    }

    void Update() {
        energyText.text = player.energy.ToString();
    }

    private void CacheTiles(Tilemap map, HashSet<Vector2Int> primary, HashSet<Vector2Int> secondary = null) {
        foreach (var cell in map.cellBounds.allPositionsWithin) {
            if (map.GetTile(cell) == null) continue;
            var world = map.CellToWorld(cell) + map.tileAnchor;
            var gp = new Vector2Int(Mathf.RoundToInt(world.x), Mathf.RoundToInt(world.y));
            primary.Add(gp);
            secondary?.Add(gp);
        }
    }

    private void CacheButtonPosition(Tilemap map) {
        foreach (var cell in map.cellBounds.allPositionsWithin) {
            if (map.GetTile(cell) == null) continue;
            var world = map.CellToWorld(cell) + map.tileAnchor;
            buttonPosition = new Vector2Int(Mathf.RoundToInt(world.x), Mathf.RoundToInt(world.y));
            break;
        }
    }

    private void CacheSingleTilePosition(Tilemap map, out Vector2Int pos) {
        pos = default;
        foreach (var cell in map.cellBounds.allPositionsWithin) {
            if (map.GetTile(cell) == null) continue;
            var world = map.CellToWorld(cell) + map.tileAnchor;
            pos = new Vector2Int(Mathf.RoundToInt(world.x), Mathf.RoundToInt(world.y));
            break;
        }
    }

    private void GenerateGrid() {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++) {
                var t = Instantiate(tilePrefab, new Vector3(x, y), quaternion.identity);
                t.name = $"Tile {x} {y}";
                t.Init((x % 2 == 0) ^ (y % 2 == 0), new Vector2Int(x, y));
            }
    }

    public bool IsPositionValid(Vector2Int pos) =>
        pos.x >= 0 && pos.x < width &&
        pos.y >= 0 && pos.y < height &&
        !blockedCells.Contains(pos) &&
        !enemyCells.Contains(pos);

    public bool IsButtonPressed() {
        if (player != null && player.GetGridPosition() == buttonPosition)
            return true;
        foreach (var g in ghostsOld) {
            if (g.gameObject.activeSelf && g.GetGridPosition() == buttonPosition)
                return true;
        }
        return false;
    }


    public void RegisterEnemyCell(Vector2Int pos) =>
        enemyCells.Add(pos);
    public void UnregisterEnemyCell(Vector2Int pos) =>
        enemyCells.Remove(pos);


    public bool IsDeadly(Vector2Int pos) =>
        deadlyCells.Contains(pos);

    public void ClearHighlights() {
        foreach (var tile in FindObjectsByType<Tile>(FindObjectsSortMode.None))
            tile.HideHighlight();
    }

    public void RespawnPlayer() {
        gemManager?.ResetGems();

        CloseTorchRoomDoor();
        if (player != null) {
            player.gameObject.GetComponent<PlayerController>().ghost = true;
            ghosts.Add(player);
            player.gameObject.SetActive(false);
            foreach (var g in ghostsOld)
                g.gameObject.SetActive(false);
        }

        var spawn = new Vector2Int(spawnPoints[levelNumber, 0], spawnPoints[levelNumber, 1]);
        player = Instantiate(playerPrefab);
        player.Init(spawn, this);
        var sr = player.GetComponent<SpriteRenderer>();
        sr.sortingOrder = 1;
        sr.color = new Color(1,1,1,1);
        player.energy = Mathf.Max(0, player.energy - ghosts.Count);

        foreach (var ghostPrefab in ghosts) {
            var ghost = Instantiate(ghostPrefab);
            ghost.Init(spawn, this);
            ghost.moveHistory = new List<PlayerController.MoveRecord>(ghostPrefab.moveHistory);
            var gsr = ghost.GetComponent<SpriteRenderer>();
            gsr.sortingOrder = 0;
            gsr.color = new Color(1,1,1,0.5f);
            ghost.gameObject.SetActive(true);
            ghost.GhostInit();
            ghostsOld.Add(ghost);
        }
    }

    public void Restart() {
        gemManager?.ResetGems();

        CloseTorchRoomDoor();
        if (player != null) player.gameObject.SetActive(false);

        foreach (var g in ghostsOld) Destroy(g.gameObject);
        ghostsOld.Clear();
        foreach (var gp in ghosts) Destroy(gp.gameObject);
        ghosts.Clear();

        var spawn = new Vector2Int(spawnPoints[levelNumber, 0], spawnPoints[levelNumber, 1]);
        player = Instantiate(playerPrefab);
        player.Init(spawn, this);
        var sr = player.GetComponent<SpriteRenderer>();
        sr.sortingOrder = 1;
        sr.color = new Color(1,1,1,1);
    }

    public void OpenTorchRoomDoor() {
        if (torchDoorTilemap          != null) torchDoorTilemap.gameObject.SetActive(false);
        if (torchDoorCollisionTilemap != null) torchDoorCollisionTilemap.gameObject.SetActive(false);
        foreach (var cell in doorCells) deadlyCells.Remove(cell);
        if (buttonTilemap != null) {
            var c = buttonTilemap.color;
            c.a = .5f;
            buttonTilemap.color = c;
        }
        OnTorchRoomDoorOpened?.Invoke();
    }

    public void CloseTorchRoomDoor() {
        if (torchDoorTilemap          != null) torchDoorTilemap.gameObject.SetActive(true);
        if (torchDoorCollisionTilemap != null) {
            torchDoorCollisionTilemap.gameObject.SetActive(true);
            CacheTiles(torchDoorCollisionTilemap, deadlyCells, doorCells);
        }
        if (buttonTilemap != null) {
            var c = buttonTilemap.color;
            c.a = 1f;
            buttonTilemap.color = c;
        }
        OnTorchRoomDoorClosed?.Invoke();
    }
}
