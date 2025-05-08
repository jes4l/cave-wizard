using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Projectiles get destroy upon collision with walls.
// Destroyed on trigger so there is no lag or build up of them.
public class BackgroundCollisions : MonoBehaviour {
    void OnTriggerEnter2D(Collider2D other) {
        Destroy(other.gameObject);}
}
