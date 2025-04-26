using System;
using UnityEngine;

public class ProjectileController : MonoBehaviour {
    public int speed = 5;
    public int mode = 0;
    private Vector3 Aimbot;

    void Start() {
        if (mode == 4) {
            PlayerController realPlayer = null;
            foreach (var pc in UnityEngine.Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None)) {
                if (!pc.ghost) {
                    realPlayer = pc;
                    break;
                }
            }

            Vector3 playerPosition = (realPlayer != null)
                ? realPlayer.transform.position
                : transform.position;

            Aimbot = (playerPosition - transform.position).normalized;
        }
    }

    void Update() {
        Action<Vector3> f = v =>
            transform.position += v * Time.deltaTime * speed;

        switch (mode) {
            case 0: f(new Vector3(1, 0)); break;    // →
            case 1: f(new Vector3(-1, 0)); break;   // ←
            case 2: f(new Vector3(0, 1)); break;    // ↑
            case 3: f(new Vector3(0, -1)); break;   // ↓
            case 4: f(Aimbot * 0.5f); break;        // [o]
        }
    }
}
