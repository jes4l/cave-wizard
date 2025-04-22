using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int _width, _height;
    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private Transform _cam;
    [SerializeField] private PlayerController _playerPrefab;

    private PlayerController _player;
    private readonly List<Tile> _tiles = new List<Tile>();

    public bool IsPositionValid(Vector2Int pos) =>
        pos.x >= 0 && pos.x < _width && pos.y >= 0 && pos.y < _height;

    void Start() =>
        GenerateGrid();

    void GenerateGrid()
    {
        for (int x = 0; x < _width; x++)
            for (int y = 0; y < _height; y++)
            {
                var spawned = Instantiate(_tilePrefab, new Vector3(x, y), quaternion.identity);
                spawned.name = $"Tile {x} {y}";
                var isOffset = (x % 2 == 0) ^ (y % 2 == 0);
                spawned.Init(isOffset, new Vector2Int(x, y));
                _tiles.Add(spawned);
            }

        _player = Instantiate(_playerPrefab);
        _player.Init(new Vector2Int(3, 4), this);
        _cam.position = new Vector3(_width/2f - .5f, _height/2f - .5f, -10);
    }

    public void ClearHighlights()
    {
        foreach (var t in _tiles)
            t.HideHighlight();
    }
}
