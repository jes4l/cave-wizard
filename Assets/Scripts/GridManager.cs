using System.Collections;
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

    public bool IsPositionValid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < _width && pos.y >= 0 && pos.y < _height;
    }


    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid(){
        for (int x = 0; x < _width; x++){
            for (int y = 0; y < _height; y++){
                var spawnedTile = Instantiate(_tilePrefab, new Vector3(x, y), quaternion.identity);
                spawnedTile.name = $"Tile {x} {y}";
                var isOffset = (x % 2 == 0 && y % 2 !=0 ) || (x % 2 != 0 && y % 2 ==0 ); 
                spawnedTile.Init(isOffset, new Vector2Int(x, y));

            }
        }

        _player = Instantiate(_playerPrefab);
        _player.Init(new Vector2Int(5, 5), this);


        _cam.transform.position = new Vector3((float)_width/2 -0.5f, (float)_height/2 - 0.5f, -10);
    }
}