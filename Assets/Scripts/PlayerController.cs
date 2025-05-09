using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class PlayerController : MonoBehaviour {
    // Records player actions for the ghost to replay.
    // Stored in Moverecord and played in GridManager.cs.
    public struct MoveRecord {
        public Vector2Int Position;
        public float      DeltaTime;
        public bool       IsAttack;

        public MoveRecord(Vector2Int position, float deltaTime, bool isAttack = false){
            Position  = position;
            DeltaTime = deltaTime;
            IsAttack  = isAttack;
        }
    }

    public static PlayerController Instance { get; private set; }

    // Tracks the energy, cannot go below 0.
    // Every tile that the player moves to costs an energy point.
    // Flag to seperate ghost from player.
    public int energy = 0;
    public bool HasEnergy => energy > 0;

    private Vector2Int gridPosition;
    private GridManager gridManager;
    private Vector2Int buttonPos;

    public List<MoveRecord> moveHistory = new();
    private float lastMoveTime;
    public GridManager Grid => gridManager;

    public bool ghost = false;

    // Every level begins with 10 energy move points.
    private void Awake() {
        if (!ghost) Instance = this;
        lastMoveTime = Time.time;
        energy = 10;
    }

    // Initialise the spawn position. 
    // Tranforms it to the grid map.
    public void Init(Vector2Int startPos, GridManager gm) {
        gridPosition  = startPos;
        gridManager   = gm;
        transform.position = new Vector3(startPos.x, startPos.y, -1f);
        buttonPos     = gridManager.GetButtonPosition();
    }

    // Prevents input on ghost or when unable to move due to 0 energy.
    // Only move on adjacent tiles.
    // Space triggers attack handled in gridmanager.
    // M to go to menu and doesnt store any progress.
    private void Update() {
        if (ghost) return;
        if (gridManager == null || !HasEnergy) return;

        Vector2Int move = Vector2Int.zero;
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))    move.y = +1;
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))  move.y = -1;
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))  move.x = -1;
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) move.x = +1;

        if (move != Vector2Int.zero)
        {
            gridManager.sfx(2);
            MoveTo(gridPosition + move);
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            energy--;
            float now = Time.time;
            float delta = now - lastMoveTime;
            lastMoveTime = now;
            moveHistory.Add(new MoveRecord(gridPosition, delta, true));
            gridManager.TryAttackAt(gridPosition, this);
        }

        if (Input.GetKeyDown(KeyCode.M))
            SceneManager.LoadScene(0);
    }
    // If player dies on collision with wall then respawn.
    // If player gets hit with a projectile then respawn.
    void OnTriggerEnter2D(Collider2D other) {
        Destroy(other.gameObject);
        if (!ghost) gridManager.RespawnPlayer();
    }

    // Replays ghosts movement.
    // Walk replays the movement with timing.
    public void GhostInit() => StartCoroutine(Walk());

    private IEnumerator Walk() {
        foreach (MoveRecord mr in moveHistory) {
            yield return new WaitForSeconds(mr.DeltaTime);
            if (mr.IsAttack) {
                gridManager.TryAttackAt(gridPosition, this);
            } else {
                MoveTo(mr.Position);
            }
        }
    }

    // If the player has attacked the ghosts can attack.
    // Records when the ghost has to attack.
    public void Attack() {
        if (!HasEnergy) return;
        energy--;
        float now        = Time.time;
        float delta      = now - lastMoveTime;
        lastMoveTime     = now;
        moveHistory.Add(new MoveRecord(gridPosition, delta, isAttack: true));
        gridManager.TryAttackAt(gridPosition, this);
    }

    // Move player to a position in the grid.
    // If player has no energy, cannot move.
    // If the player tires to move to an invalid tile, in gridmanager, then cannot move.
    // If player collides with wall instantly respawn.
    // Clears highlight on adjacent tiles when moved.
    // Records player movement.
    public void MoveTo(Vector2Int target) {
        bool wasOnButton = gridPosition == buttonPos;
        if (!HasEnergy) {
            Debug.Log("No energy left – cannot move.");
            return;
        }
        if (!gridManager.IsPositionValid(target)) {
            Debug.Log("Tried to move to invalid tile.");
            return;
        }
        if (gridManager.IsDeadly(target)) {
            Debug.Log($"Entered deadly tile at {target} – respawning.");
            if (!ghost) gridManager.RespawnPlayer();
            return;
        }

        gridManager.ClearHighlights();

        float now    = Time.time;
        float delta  = now - lastMoveTime;
        lastMoveTime = now;
        if (!ghost) moveHistory.Add(new MoveRecord(target, delta));

        gridPosition = target;
        transform.position = new Vector3(target.x, target.y, -1f);
        
        // If player colides with torch delay by 1 second to play sound effects.
        if (gridPosition == gridManager.GetTorchPosition()) {
            gridManager.sfx(1);
            StartCoroutine(Delay());
        }
        // Upon collision with the the gems players gain energy.
        // either +2, +4, or +7
        // Player movement costs +1 energy movepoint
        // If ghost is on button and player goes on button and comes of door remains open.
        Collider2D[] hits = Physics2D.OverlapPointAll(transform.position);
        foreach (var hit in hits) {
            if (hit.CompareTag("Gem1")) {
                energy += 2;
                hit.tag = "Untagged";
                hit.gameObject.SetActive(false);
            }
            else if (hit.CompareTag("Gem2")) {
                energy += 4;
                hit.tag = "Untagged";
                hit.gameObject.SetActive(false);
            }
            else if (hit.CompareTag("Gem3")) {
                energy += 7;
                hit.tag = "Untagged";
                hit.gameObject.SetActive(false);
            }
        }

        energy--;
        Debug.Log($"Player moved to {gridPosition} (Δt = {delta:F2}s). Energy: {energy}");

        bool isOnButton = gridPosition == buttonPos;
        if (isOnButton) {
            gridManager.OpenTorchRoomDoor();
        }
        else if (wasOnButton && !gridManager.IsButtonPressed()) {
            gridManager.CloseTorchRoomDoor();
        }
    }
    
    // Waits 2 seconds before moving to next level.
    // On collision with Torch level number increments.
    // Reset to 0 on M click.
    private IEnumerator Delay() {
        yield return new WaitForSeconds(2);        
        GridManager.levelNumber++;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public Vector2Int GetGridPosition() => gridPosition;
}
