using System.Collections.Generic;
using UnityEngine;

public class GemManager : MonoBehaviour
{
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private GameObject _gem1Prefab;
    [SerializeField] private GameObject _gem2Prefab;
    [SerializeField] private GameObject _gem3Prefab;

    private readonly List<GameObject> _allGems = new();

    void Start()
    {
        SpawnGem(_gem1Prefab, 1, 3, 5, 7);
        SpawnGem(_gem2Prefab, 7, 9, 0, 2);
        SpawnGem(_gem3Prefab, 11, 14, 2, 3);
    }

    private void SpawnGem(GameObject prefab, int xMin, int xMax, int yMin, int yMax)
    {
        Vector2Int pos;
        do
        {
            pos = new Vector2Int(
                Random.Range(xMin, xMax + 1),
                Random.Range(yMin, yMax + 1)
            );
        }
        while (!_gridManager.IsPositionValid(pos));

        var gem = Instantiate(prefab, new Vector3(pos.x, pos.y, -1f), Quaternion.identity);
        _allGems.Add(gem);
    }

    public void ResetGems()
    {
        foreach (var gem in _allGems)
        {
            if (gem == null) continue;

            if (gem.name.Contains(_gem1Prefab.name)) gem.tag = "Gem1";
            else if (gem.name.Contains(_gem2Prefab.name)) gem.tag = "Gem2";
            else if (gem.name.Contains(_gem3Prefab.name)) gem.tag = "Gem3";

            gem.SetActive(true);
        }
    }
}
