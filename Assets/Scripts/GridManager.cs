using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
    [SerializeField] private GameObject soundFX;
    [SerializeField] private Tilemap torchTilemap;
    [SerializeField] private GemManager gemManager;
    public static GridManager Instance { get; private set; }
    private Vector2Int buttonPosition;
    private Vector2Int torchPosition;
    public Vector2Int GetButtonPosition() => buttonPosition;
    public Vector2Int GetTorchPosition()  => torchPosition;
    public Vector2Int GetPlayerGridPosition() => player.GetGridPosition();
    private PlayerController player;
    private readonly HashSet<Vector2Int> blockedCells = new();
    private readonly HashSet<Vector2Int> deadlyCells = new();
    private readonly HashSet<Vector2Int> doorCells = new();
    private readonly HashSet<Vector2Int> enemyCells = new();
    private readonly Dictionary<Vector2Int, EnemyController> enemiesByCell = new Dictionary<Vector2Int, EnemyController>();
    private List<PlayerController> ghosts = new List<PlayerController>();
    private List<PlayerController> ghostsOld = new List<PlayerController>();

    // Player prefab spawn points for each level.
    // Level Number is incremented if player reaches torch.
    private int[,] spawnPoints = {{3, 4}, {13, 0}, {8, 0}, {2, 0}, {1, 6}};
    public static int levelNumber = 0;

    // Events for opening and closing the doors.
    // Managed in gate.cs.
    public event Action OnTorchRoomDoorOpened;
    public event Action OnTorchRoomDoorClosed;

    private bool attackLock;

    // Gets refrences to for all the items in the tilemap.
    // caches them into hash sets
    void Awake() {
        Instantiate(soundFX);

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
            CacheTorchPosition(torchTilemap, out torchPosition);
    }

    // On start it constructs the grid using the tiles, each cell managed in tile.cs.
    // spawns the player based on the level.
    void Start() {
        GenerateGrid();
        RespawnPlayer();
    }

    // Updates the energy text on screen every tick.
    void Update() {
        energyText.text = player.energy.ToString();
    }

    // Trys to attack and plays sound.
    // Removes adjacent enemy if present and stops the projectiles firing.
    // Manages the animation for the player.
    public bool TryAttackAt(Vector2Int origin, PlayerController p) {
        StartCoroutine(AttackDelay(p));
        sfx(3);
        foreach (var dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.right, Vector2Int.left }) {
            var cell = origin + dir;
            if (!enemiesByCell.TryGetValue(cell, out var enemy))
                continue;

            enemyCells.Remove(cell);
            enemiesByCell.Remove(cell);
            enemy.StopAllCoroutines();
            StartCoroutine(FadeThenDestroy(enemy.gameObject, 1.1f));
            return true;
        }
        return false;
    }
    
    // Fades the enemy slowly in time with the players animation.
    // After fade is complete enemy is destroyed.
    private IEnumerator FadeThenDestroy(GameObject obj, float duration) {
        var sr = obj.GetComponentInChildren<SpriteRenderer>();
        if (sr == null) {
            Destroy(obj);
            yield break;
        }

        float elapsed = 0f;
        var original = sr.color;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            sr.color = new Color(original.r, original.g, original.b, alpha);
            yield return null;
        }
        Destroy(obj);
    }

    // Prevents multiple attacks from being carried out.
    // This prevents errors with animations.
    private IEnumerator AttackDelay(PlayerController p)
    {
        if (attackLock) yield break;
        attackLock = true;
        p.transform.GetChild(0).gameObject.SetActive(false);
        p.transform.GetChild(1).gameObject.SetActive(true);

        yield return new WaitForSeconds(1);        
        p.transform.GetChild(1).gameObject.SetActive(false);
        p.transform.GetChild(0).gameObject.SetActive(true);
        attackLock = false;
    }

    // Maps each occupied cell to the girds co ordinates.
    // Rounds ecah position up for one to one mappign.
    // Used to prevent the player from going on occupied tiles.
    private void CacheTiles(Tilemap map, HashSet<Vector2Int> primary, HashSet<Vector2Int> secondary = null) {
        foreach (var cell in map.cellBounds.allPositionsWithin) {
            if (map.GetTile(cell) == null) continue;
            var world = map.CellToWorld(cell) + map.tileAnchor;
            var gp = new Vector2Int(Mathf.RoundToInt(world.x), Mathf.RoundToInt(world.y));
            primary.Add(gp);
            secondary?.Add(gp);
        }
    }
    // Maps the button position to the grids co ordinates.
    // Used to open/close the doors/gates.
    // gets the position from the game object.
    private void CacheButtonPosition(Tilemap map) {
        foreach (var cell in map.cellBounds.allPositionsWithin) {
            if (map.GetTile(cell) == null) continue;
            var world = map.CellToWorld(cell) + map.tileAnchor;
            buttonPosition = new Vector2Int(Mathf.RoundToInt(world.x), Mathf.RoundToInt(world.y));
            break;
        }
    }

    // Maps the torch position to the grids co ordinates.
    // Upon collision with the co ordinate advances the players level.
    // gets the position from the game object.
    private void CacheTorchPosition(Tilemap map, out Vector2Int pos) {
        pos = default;
        foreach (var cell in map.cellBounds.allPositionsWithin) {
            if (map.GetTile(cell) == null) continue;
            var world = map.CellToWorld(cell) + map.tileAnchor;
            pos = new Vector2Int(Mathf.RoundToInt(world.x), Mathf.RoundToInt(world.y));
            break;
        }
    }

    // creates the grid based on the tile prefab.
    // craetes a checkerboard pattern based on both colours.
    private void GenerateGrid() {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++) {
                var t = Instantiate(tilePrefab, new Vector3(x, y), quaternion.identity);
                t.name = $"Tile {x} {y}";
                t.Init((x % 2 == 0) ^ (y % 2 == 0), new Vector2Int(x, y));
            }
    }

    // Gets the positions the players cannot move on.
    public bool IsPositionValid(Vector2Int pos) =>
        pos.x >= 0 && pos.x < width &&
        pos.y >= 0 && pos.y < height &&
        !blockedCells.Contains(pos) &&
        !enemyCells.Contains(pos);

    // Finds which enemy is on which cell.
    // Used to figure out which enemy to destroy on attack.
    public bool IsEnemyAt(Vector2Int pos) => 
        enemiesByCell.ContainsKey(pos);

    // Checks if the player or ghosts is on the button.
    // The ghost can open the door as can the player.
    public bool IsButtonPressed() {
        if (player != null && player.GetGridPosition() == buttonPosition)
            return true;
        foreach (var g in ghostsOld) {
            if (g.gameObject.activeSelf && g.GetGridPosition() == buttonPosition)
                return true;
        }
        return false;
    }

    //  Gets all enemys positions to stop the player from going on that tile.
    public void RegisterEnemyCell(Vector2Int pos, EnemyController enemy) {
        enemyCells.Add(pos);
        enemiesByCell[pos] = enemy;
    }

    // If the player or shost kill the enemy, its cell becomes free.
    // This means the player can travel on it.
     public void UnregisterEnemyCell(Vector2Int pos, EnemyController enemy) {
        if (enemiesByCell.TryGetValue(pos, out var e) && e == enemy) {
            enemyCells.Remove(pos);
            enemiesByCell.Remove(pos);
        }
     }

    // Gets the position of the cells that can kill the player.
    // Is used to reset the player to the spawn point.
    // Makes sure ghost cannot enter this tile.
    public bool IsDeadly(Vector2Int pos) => deadlyCells.Contains(pos);

    // Clears the highlight managed in tile.cs.
    // Clears it when the player moves.
    public void ClearHighlights() {
        foreach (var tile in FindObjectsByType<Tile>(FindObjectsSortMode.None))
            tile.HideHighlight();
    }

    // The time loop mechanic for respawning the players ghost.
    // Minius an energy point when clicked, closes the door/gate and resets the gems.
    // Converts the current player into a ghost, marking it as a ghost, changes the sorting order, so player is on top.
    // Copys the move history in PlayerController and calls the ghost with 50% alpha.
    // Clears the ghosts histroy upon reset.
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

        for (int i = 0; i < 4; i++) {
            var sr = player.transform.GetChild(0).GetChild(i).GetComponent<SpriteRenderer>();
            sr.sortingOrder = 1;
            sr.color = new Color(1,1,1,1);
        }
        for (int i = 0; i < 8; i++) {
            var sr = player.transform.GetChild(1).GetChild(i).GetComponent<SpriteRenderer>();
            sr.sortingOrder = 1;
            sr.color = new Color(1,1,1,1);
        }

        player.energy = Mathf.Max(0, player.energy - ghosts.Count);

        foreach (var ghostPrefab in ghosts) {
            var ghost = Instantiate(ghostPrefab);
            ghost.Init(spawn, this);
            ghost.moveHistory = new List<PlayerController.MoveRecord>(ghostPrefab.moveHistory);
            
            for (int i = 0; i < 4; i++) {
                var sr = ghost.transform.GetChild(0).GetChild(i).GetComponent<SpriteRenderer>();
                sr.sortingOrder = 0;
                sr.color = new Color(1,1,1,0.5f);
            }
            for (int i = 0; i < 8; i++) {
                var sr = ghost.transform.GetChild(1).GetChild(i).GetComponent<SpriteRenderer>();
                sr.sortingOrder = 0;
                sr.color = new Color(1,1,1,0.5f);
            }
            
            ghost.gameObject.SetActive(true);
            ghost.GhostInit();
            ghostsOld.Add(ghost);
        }
        var spawner = UnityEngine.Object.FindAnyObjectByType<EnemySpawner>();
        spawner?.ResetEnemies();
        EventSystem.current.SetSelectedGameObject(null);
        sfx(0);
    }

    // Resets the games state, gems and closes the door/gate.
    // Destroys all the ghosts and recreates the player at spawn point.
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
        
        for (int i = 0; i < 4; i++) {
            var sr = player.transform.GetChild(0).GetChild(i).GetComponent<SpriteRenderer>();
            sr.sortingOrder = 1;
            sr.color = new Color(1,1,1,1);
        }
        for (int i = 0; i < 8; i++) {
            var sr = player.transform.GetChild(1).GetChild(i).GetComponent<SpriteRenderer>();
            sr.sortingOrder = 1;
            sr.color = new Color(1,1,1,1);
        }

        var spawner = UnityEngine.Object.FindAnyObjectByType<EnemySpawner>();
        spawner?.ResetEnemies();
        EventSystem.current.SetSelectedGameObject(null);
        sfx(0);
    }

    // Deactivate door visuals and collisions
    // Reduces the buttons alpha by 50% to show it has been clicked.
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

    // Activates door visuals and collisions
    // Increase the buttons alpha by 50% to show it has not been clicked.
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

    // Play sound based on prefabs index.
    public void sfx(int index) =>        
        soundFX.transform.GetChild(index).GetComponent<AudioSource>().Play();
    }

