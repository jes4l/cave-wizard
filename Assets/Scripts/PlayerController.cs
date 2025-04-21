using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private int energy = 10;
    private Vector2Int _gridPosition;
    private GridManager _gridManager;

    public void Init(Vector2Int startPos, GridManager gridManager)
    {
        _gridPosition = startPos;
        _gridManager = gridManager;
        transform.position = new Vector3(_gridPosition.x, _gridPosition.y, -1);
    }

    void Update()
    {
        if (_gridManager == null) return;

        Vector2Int movement = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.UpArrow)) movement.y += 1;
        if (Input.GetKeyDown(KeyCode.DownArrow)) movement.y -= 1;
        if (Input.GetKeyDown(KeyCode.LeftArrow)) movement.x -= 1;
        if (Input.GetKeyDown(KeyCode.RightArrow)) movement.x += 1;

        if (movement != Vector2Int.zero)
        {
            Vector2Int newPos = _gridPosition + movement;

            if (_gridManager.IsPositionValid(newPos))
            {
                _gridPosition = newPos;
                transform.position = new Vector3(_gridPosition.x, _gridPosition.y, -1);

                energy--;
                Debug.Log($"Player moved to {_gridPosition}. Energy: {energy}");
            }
            else
            {
                Debug.Log("Tried to move to invalid tile.");
            }
        }

    }
    public Vector2Int GetGridPosition()
    {
        return _gridPosition;
    }

}