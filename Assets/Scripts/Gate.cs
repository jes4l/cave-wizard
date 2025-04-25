using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(GridManager))]
public class Gate : MonoBehaviour {
    [SerializeField] private Tilemap gateClosedTilemap;
    [SerializeField] private Tilemap gateOpenTilemap;

    private GridManager gm;
    private readonly HashSet<Vector2Int> gateCells = new();

    void Awake() {
        gm = GetComponent<GridManager>();

        if (gateOpenTilemap   != null) gateOpenTilemap.gameObject.SetActive(true);
        if (gateClosedTilemap != null) gateClosedTilemap.gameObject.SetActive(false);

        gm.OnTorchRoomDoorOpened  += CloseGate;
        gm.OnTorchRoomDoorClosed  += OpenGate;
    }

    void OnDestroy() {
        gm.OnTorchRoomDoorOpened  -= CloseGate;
        gm.OnTorchRoomDoorClosed  -= OpenGate;
    }

    private void OpenGate() {
        if (gateClosedTilemap != null) gateClosedTilemap.gameObject.SetActive(false);
        if (gateOpenTilemap   != null) gateOpenTilemap.gameObject.SetActive(true);

        var bc = (HashSet<Vector2Int>)typeof(GridManager)
            .GetField("blockedCells", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(gm);

        foreach (var cell in gateCells)
            bc.Remove(cell);
    }

    private void CloseGate() {
        if (gateOpenTilemap   != null) gateOpenTilemap.gameObject.SetActive(false);

        if (gateClosedTilemap != null) {
            gateClosedTilemap.gameObject.SetActive(true);
            gm.GetType()
              .GetMethod("CacheTiles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
              .Invoke(gm, new object[]{
                  gateClosedTilemap,
                  typeof(GridManager)
                    .GetField("blockedCells", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .GetValue(gm),
                  gateCells
              });
        }
    }
}
