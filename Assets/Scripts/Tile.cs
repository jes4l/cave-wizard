using UnityEngine;

public class Tile : MonoBehaviour {
    [SerializeField] private Color _baseColor, _offsetColor;
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private GameObject _highlight;

    private Vector2Int _gridPos;

    public void Init(bool isOffset, Vector2Int gridPos) {
        _gridPos = gridPos;
        _renderer.color = isOffset ? _offsetColor : _baseColor;
    }

    public void OnMouseOver() {
        var p = PlayerController.Instance;
        if (p != null && p.HasEnergy && IsNextTo(p.GetGridPosition(), _gridPos))
            _highlight.SetActive(true);
    }

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

    public void OnMouseExit() =>
        _highlight.SetActive(false);

    private bool IsNextTo(Vector2Int a, Vector2Int b) =>
        (Mathf.Abs(a.x - b.x) == 1 && a.y == b.y) ||
        (Mathf.Abs(a.y - b.y) == 1 && a.x == b.x);

    public void HideHighlight() =>
        _highlight.SetActive(false);
}
