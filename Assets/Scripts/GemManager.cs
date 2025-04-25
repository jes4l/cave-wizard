using System.Collections.Generic;
using UnityEngine;

public class GemManager : MonoBehaviour {
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private GameObject _gem1Prefab;
    [SerializeField] private GameObject _gem2Prefab;
    [SerializeField] private GameObject _gem3Prefab;

    private readonly List<GameObject> _allGems = new();
    private readonly HashSet<Vector2Int> _occupied = new();

    int[,,] gemSpawnPoints = {{{3, 7}, {8, 5}, {10, 2}},
                              {{3, 7}, {8, 5}, {10, 2}},
                              {{3, 7}, {8, 5}, {10, 2}}}; //...
    
    int[,,] decoySpawnPoints = {{{3, 7}, {8, 5}, {10, 2}},
                              {{-1, -1}, {8, 5}, {10, 2}},
                              {{3, 7}, {8, 5}, {10, 2}}}; //...

    void Start() {
        GameObject[] gems = {_gem1Prefab, _gem2Prefab, _gem3Prefab};

        for (int i = 0; i < 3; i++)
        {
            int a = gemSpawnPoints[GridManager.levelNumber, i, 0],
                b = gemSpawnPoints[GridManager.levelNumber, i, 1];
            SpawnGem(gems[i], a, b);
        }

        for (int i = 0; i < 3; i++)
        {
            int a = decoySpawnPoints[GridManager.levelNumber, i, 0],
                b = decoySpawnPoints[GridManager.levelNumber, i, 1];
            SpawnGem(gems[i], a, b);
        }
    }

        private void SpawnGem(GameObject prefab, int x, int y)
    {
        if (x == -1) return;
        Vector2Int pos = new Vector2Int(x, y);
        var gem = Instantiate(prefab, new Vector3(pos.x, pos.y, -1f), Quaternion.identity);
        _allGems.Add(gem);
        _occupied.Add(pos);
    }


    public void ResetGems() {
        foreach (var gem in _allGems) {
            if (gem == null) continue;
            if (gem.name.Contains(_gem1Prefab.name))      gem.tag = "Gem1";
            else if (gem.name.Contains(_gem2Prefab.name)) gem.tag = "Gem2";
            else if (gem.name.Contains(_gem3Prefab.name)) gem.tag = "Gem3";
            gem.SetActive(true);
        }
    }
}
