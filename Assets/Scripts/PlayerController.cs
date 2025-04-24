using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public struct MoveRecord
    {
        public Vector2Int Position;
        public float      DeltaTime;

        public MoveRecord(Vector2Int position, float deltaTime)
        {
            Position  = position;
            DeltaTime = deltaTime;
        }
    }

    public static PlayerController Instance { get; private set; }

    public int energy = 0;
    public bool HasEnergy => energy > 0;

    private Vector2Int gridPosition;
    private GridManager gridManager;
    private Vector2Int buttonPos;

    public List<MoveRecord> moveHistory = new();
    private float lastMoveTime;

    public bool ghost = false;

    private void Awake()
    {
        Instance     = this;
        lastMoveTime = Time.time;
        energy = 10;
    }

    public void Init(Vector2Int startPos, GridManager gm)
    {
        gridPosition  = startPos;
        gridManager   = gm;
        transform.position = new Vector3(startPos.x, startPos.y, -1f);
        buttonPos     = gridManager.GetButtonPosition();
    }

    private void Update()
    {
        if (ghost) return;
        if (gridManager == null || !HasEnergy) return;

        Vector2Int move = Vector2Int.zero;
        if (Input.GetKeyDown(KeyCode.UpArrow))    move.y = +1;
        if (Input.GetKeyDown(KeyCode.DownArrow))  move.y = -1;
        if (Input.GetKeyDown(KeyCode.LeftArrow))  move.x = -1;
        if (Input.GetKeyDown(KeyCode.RightArrow)) move.x = +1;

        if (move != Vector2Int.zero)
            MoveTo(gridPosition + move);
    }

    public void GhostInit() => StartCoroutine(Walk());

    private IEnumerator Walk()
    {
        foreach (MoveRecord mr in moveHistory)
        {
            yield return new WaitForSeconds(mr.DeltaTime);
            MoveTo(mr.Position);
        }
    }

    public void MoveTo(Vector2Int target)
    {
        bool wasOnButton = gridPosition == buttonPos;
        if (!HasEnergy)
        {
            Debug.Log("No energy left – cannot move.");
            return;
        }
        if (!gridManager.IsPositionValid(target))
        {
            Debug.Log("Tried to move to invalid tile.");
            return;
        }
        if (gridManager.IsDeadly(target))
        {
            Debug.Log($"Entered deadly tile at {target} – respawning.");
            if (!ghost) gridManager.RespawnPlayer();
            return;
        }

        gridManager.ClearHighlights();

        float now    = Time.time;
        float delta  = now - lastMoveTime;
        lastMoveTime = now;
        if (!ghost) moveHistory.Add(new MoveRecord(target, delta));
        Debug.Log($"History: {target}, {delta}");

        gridPosition = target;
        transform.position = new Vector3(target.x, target.y, -1f);
        energy--;
        Debug.Log($"Player moved to {gridPosition} (Δt = {delta:F2}s). Energy: {energy}");

        bool isOnButton = gridPosition == buttonPos;
        if (isOnButton)
            gridManager.OpenTorchRoomDoor();
        else if (wasOnButton)
            gridManager.CloseTorchRoomDoor();
    }

    public Vector2Int GetGridPosition() => gridPosition;
}
