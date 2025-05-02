using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Refresh : MonoBehaviour
{
    public GridManager gridManager;
    void Start() => gridManager.Restart();    
}
