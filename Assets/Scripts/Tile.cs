using UnityEngine;

// Represents a single grid cell.
public class Tile : MonoBehaviour {
    [SerializeField] private Color baseColor, offsetColor;
    [SerializeField] private SpriteRenderer spriterenderer;
    [SerializeField] private GameObject highlight;

    private Vector2Int _gridPos;

    public void Init(bool isOffset, Vector2Int gridPos) {
        _gridPos = gridPos;
        spriterenderer.color = isOffset ? offsetColor : baseColor;
    }
    // When the mouse is hovered over tiles adjacent to player valid tiles to move on highlight white.
    // This shows where the player can move.
    public void OnMouseOver() {
        var p = PlayerController.Instance;
        if (p != null && p.HasEnergy && IsNextTo(p.GetGridPosition(), _gridPos))
            highlight.SetActive(true);
    }

    // Checks if the tile clicked is valid and moves the player to the tile.
    // If the tile has an enemy on it, it is able to be killed.
    public void OnMouseDown() {
        var p = PlayerController.Instance;
        if (p == null || !p.HasEnergy) 
            return;

        var playerPos = p.GetGridPosition();
        if (!IsNextTo(playerPos, _gridPos)) 
            return;

        if (p.Grid.IsEnemyAt(_gridPos)) {
            p.Attack();}
        else { p.MoveTo(_gridPos);}
    }
    // Disables cursor if the player moves of the tile.
    // This is so only valid tiles the player can move on can be highlighted.
    public void OnMouseExit() =>
        highlight.SetActive(false);

    // Calulates whether the two grid positions are orthogonally adjacent.
    private bool IsNextTo(Vector2Int a, Vector2Int b) =>
        (Mathf.Abs(a.x - b.x) == 1 && a.y == b.y) ||
        (Mathf.Abs(a.y - b.y) == 1 && a.x == b.x);

    // Resets the tiles highlight and called in onMouseExit.
    public void HideHighlight() =>
        highlight.SetActive(false);
}
