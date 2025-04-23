// PlayerController.cs
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    private int energy = 100;
    public bool HasEnergy => energy > 0;

    private Vector2Int _gridPosition;
    private GridManager _gridManager;

    private void Awake() =>
        Instance = this;

    public void Init(Vector2Int startPos, GridManager gridManager)
    {
        _gridPosition = startPos;
        _gridManager = gridManager;
        transform.position = new Vector3(startPos.x, startPos.y, -1f);
    }

    void Update()
    {
        if (_gridManager == null || !HasEnergy) return;

        var move = Vector2Int.zero;
        if (Input.GetKeyDown(KeyCode.UpArrow))    move.y = +1;
        if (Input.GetKeyDown(KeyCode.DownArrow))  move.y = -1;
        if (Input.GetKeyDown(KeyCode.LeftArrow))  move.x = -1;
        if (Input.GetKeyDown(KeyCode.RightArrow)) move.x = +1;

        if (move != Vector2Int.zero)
            MoveTo(_gridPosition + move);
    }

    public void MoveTo(Vector2Int target)
    {
        if (!HasEnergy)
        {
            Debug.Log("No energy left – cannot move.");
            return;
        }

        if (!_gridManager.IsPositionValid(target))
        {
            Debug.Log("Tried to move to invalid tile.");
            return;
        }

        if (_gridManager.IsDeadly(target))
        {
            Debug.Log($"Entered deadly tile at {target} – respawning.");
            _gridManager.RespawnPlayer();
            return;
        }

        _gridManager.ClearHighlights();
        _gridPosition = target;
        transform.position = new Vector3(target.x, target.y, -1f);
        energy--;
        Debug.Log($"Player moved to {_gridPosition}. Energy: {energy}");
    }

    public Vector2Int GetGridPosition() => _gridPosition;
}
