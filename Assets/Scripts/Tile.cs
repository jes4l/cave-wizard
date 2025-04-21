using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private Color _baseColor, _offsetColor;
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private GameObject _highlight;

    private Vector2Int _gridPos;

    public void Init(bool isOffset, Vector2Int gridPos)
    {
        _gridPos = gridPos;
        _renderer.color = isOffset ? _offsetColor : _baseColor;
    }

    [System.Obsolete]//fixes warning
    public void OnMouseOver()
    {
        var player = FindObjectOfType<PlayerController>();
        if (player == null) return;

        Vector2Int playerPos = player.GetGridPosition();

        if (IsNextTo(playerPos, _gridPos))
        {
            _highlight.SetActive(true);
        }
    }

    public void OnMouseExit()
    {
        _highlight.SetActive(false);
    }

    private bool IsNextTo(Vector2Int a, Vector2Int b){
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);

        return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
    }
}
