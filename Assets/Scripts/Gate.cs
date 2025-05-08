using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Manages when the gate opens and closes for the applied races mechanic.
// once the button is pressed one door opens and the other closes.
// Gets the open and close tilmap locations.

[RequireComponent(typeof(GridManager))]
public class Gate : MonoBehaviour {
    [SerializeField] private Tilemap gateClosedTilemap;
    [SerializeField] private Tilemap gateOpenTilemap;

    private GridManager gm;
    private readonly HashSet<Vector2Int> gateCells = new();

    // Sets each game object to on for the open gate and off for the close gate.
    // Initialsises at the beginning.
    void Awake() {
        gm = GetComponent<GridManager>();

        if (gateOpenTilemap   != null) gateOpenTilemap.gameObject.SetActive(true);
        if (gateClosedTilemap != null) gateClosedTilemap.gameObject.SetActive(false);

        gm.OnTorchRoomDoorOpened  += CloseGate;
        gm.OnTorchRoomDoorClosed  += OpenGate;
    }

    // destroys the gates based on state.
    void OnDestroy() {
        gm.OnTorchRoomDoorOpened  -= CloseGate;
        gm.OnTorchRoomDoorClosed  -= OpenGate;
    }

    // shows the open gate.
    // blocks the cell the close gate is on.
    // unblocks the cell so the player can go through it.
    private void OpenGate() {
        if (gateClosedTilemap != null) gateClosedTilemap.gameObject.SetActive(false);
        if (gateOpenTilemap   != null) gateOpenTilemap.gameObject.SetActive(true);

        var bc = (HashSet<Vector2Int>)typeof(GridManager)
            .GetField("blockedCells", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(gm);

        foreach (var cell in gateCells)
            bc.Remove(cell);
    }

    // shows the close gate.
    // blocks the cell the open gate is on.
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
