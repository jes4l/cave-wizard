using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    private int energy = 10;
    public bool HasEnergy => energy > 0;

    private Vector2Int _gridPosition;
    private GridManager _gridManager;

    private void Awake() =>
        Instance = this;

    public void Init(Vector2Int startPos, GridManager gridManager)
    {
        _gridPosition = startPos;
        _gridManager = gridManager;
        transform.position = new Vector3(startPos.x, startPos.y, -1);
    }

    void Update()
    {
        if (_gridManager == null || !HasEnergy) 
            return;

        var m = Vector2Int.zero;
        if (Input.GetKeyDown(KeyCode.UpArrow))    m.y = +1;
        if (Input.GetKeyDown(KeyCode.DownArrow))  m.y = -1;
        if (Input.GetKeyDown(KeyCode.LeftArrow))  m.x = -1;
        if (Input.GetKeyDown(KeyCode.RightArrow)) m.x = +1;

        if (m != Vector2Int.zero)
            MoveTo(_gridPosition + m);
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

        _gridManager.ClearHighlights();
        _gridPosition = target;
        transform.position = new Vector3(target.x, target.y, -1);
        energy--;
        Debug.Log($"Player moved to {_gridPosition}. Energy: {energy}");
    }

    public Vector2Int GetGridPosition() => _gridPosition;
}
