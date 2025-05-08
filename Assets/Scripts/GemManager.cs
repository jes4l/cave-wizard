using System.Collections.Generic;
using UnityEngine;

// Manages all gem prefabs and spawn points. 
// Resets the gem positons.
public class GemManager : MonoBehaviour {
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GameObject gem1Prefab;
    [SerializeField] private GameObject gem2Prefab;
    [SerializeField] private GameObject gem3Prefab;

    private readonly List<GameObject> allGems = new();
    private readonly HashSet<Vector2Int> occupied = new();

    // 3D array for co ordinatates for all spawn points for the correct path for the gems.
    // Stored as Gem1, Gem2 and Gem3 for each level.
    int[,,] gemSpawnPoints = {{{5, 7}, {10, 2}, {2, 1}},
                              {{9, 1}, {13, 4}, {14,3}},
                              {{14, 7}, {2, 0}, {13, 3}},
                              {{7, 7}, {9, 7}, {3, 6}},
                              {{8, 5}, {9, 7}, {2, 5}},
                              };

    // 3D array for co ordinatates for all the decoy gems that aim to trick the user to taking incorrect path.
    // Some of these gem co ordinates actually do help the player but most do not.
    int[,,] decoySpawnPoints = {{{6, 4}, {-1, -1}, {-1, -1}},
                              {{12, 4}, {3, 7}, {1, 5}},
                              {{9, 6}, {5, 4}, {9, 8}},
                              {{10, 2}, {6, 2}, {11, 4}},
                              {{13, 5}, {12, 8}, {6, 1}},
                              };


    // Spawns the gems on start and decoy gems on start for each level.
    void Start() {
        GameObject[] gems = {gem1Prefab, gem2Prefab, gem3Prefab};

        for (int i = 0; i < 3; i++) {
            int a = gemSpawnPoints[GridManager.levelNumber, i, 0],
                b = gemSpawnPoints[GridManager.levelNumber, i, 1];
            SpawnGem(gems[i], a, b);
        }

        for (int i = 0; i < 3; i++) {
            int a = decoySpawnPoints[GridManager.levelNumber, i, 0],
                b = decoySpawnPoints[GridManager.levelNumber, i, 1];
            SpawnGem(gems[i], a, b);
        }
    }
       // Instantiates gem prefab at grid co ordinates.
       // If its valid, it records it to track its positions, for when the player resets.
       // If position is invalid, the gems do not spawn.
        private void SpawnGem(GameObject prefab, int x, int y) {
        if (x == -1) return;
        Vector2Int pos = new Vector2Int(x, y);
        var gem = Instantiate(prefab, new Vector3(pos.x, pos.y, -1f), Quaternion.identity);
        allGems.Add(gem);
        occupied.Add(pos);
    }

    // Creates all the gems again so the player or ghost can collect it.
    // retags them for the energy move points.
    public void ResetGems() {
        foreach (var gem in allGems) {
            if (gem == null) continue;
            if (gem.name.Contains(gem1Prefab.name))      gem.tag = "Gem1";
            else if (gem.name.Contains(gem2Prefab.name)) gem.tag = "Gem2";
            else if (gem.name.Contains(gem3Prefab.name)) gem.tag = "Gem3";
            gem.SetActive(true);
        }
    }
}
