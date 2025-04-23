using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    private int energy = 100;
    public bool HasEnergy => energy > 0;

    private Vector2Int gridPosition;
    private GridManager gridManager;
    private static readonly Vector2Int ButtonPos = new Vector2Int(7, 1);

    private void Awake() => Instance = this;

    public void Init(Vector2Int startPos, GridManager gm)
    {
        gridPosition = startPos;
        gridManager = gm;
        transform.position = new Vector3(startPos.x, startPos.y, -1f);
    }

    void Update()
    {
        if (gridManager == null || !HasEnergy) return;

        var move = Vector2Int.zero;
        if (Input.GetKeyDown(KeyCode.UpArrow))    move.y = +1;
        if (Input.GetKeyDown(KeyCode.DownArrow))  move.y = -1;
        if (Input.GetKeyDown(KeyCode.LeftArrow))  move.x = -1;
        if (Input.GetKeyDown(KeyCode.RightArrow)) move.x = +1;

        if (move != Vector2Int.zero)
            MoveTo(gridPosition + move);
    }

    public void MoveTo(Vector2Int target)
    {
        var wasOnButton = gridPosition == ButtonPos;

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
            gridManager.RespawnPlayer();
            return;
        }

        gridManager.ClearHighlights();
        gridPosition = target;
        transform.position = new Vector3(target.x, target.y, -1f);
        energy--;
        Debug.Log($"Player moved to {gridPosition}. Energy: {energy}");

        var isOnButton = target == ButtonPos;
        if (isOnButton)
            gridManager.OpenTorchRoomDoor();
        else if (wasOnButton)
            gridManager.CloseTorchRoomDoor();
    }

    public Vector2Int GetGridPosition() => gridPosition;
}
