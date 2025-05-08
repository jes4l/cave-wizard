using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Resets player position after cutscene.
// So when the respawn ghost button is clicked there is no delay.
// So movement during cutscene is reset.
public class Refresh : MonoBehaviour {
    public GridManager gridManager;
    void Start() => gridManager.Restart();    
}
