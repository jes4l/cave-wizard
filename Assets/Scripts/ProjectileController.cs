using UnityEngine;

// Handles the speed and direction in which the projectile spawns.
// Gives the enemy spawner the mode in which the projectile will shoot.
public class ProjectileController : MonoBehaviour {
    public int speed = 5;
    public int mode = 0;

    private Vector3 aimbot;
    private SpriteRenderer spriteRenderer;

    void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
     
    // On start, if the mode is 4, it will find the players position.
    // It will then fire a projector directly at the player.
    // Doesnt not lock onto the players position.
    void Start() {
        if (mode == 4) {
            PlayerController realPlayer = null;
            foreach (var pc in Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
                if (!pc.ghost) {
                    realPlayer = pc;
                    break;
                }

            Vector3 target = realPlayer != null
                ? realPlayer.transform.position
                : transform.position;

            aimbot = (target - transform.position).normalized;
        }
    }

    // The modes for all the projectiles.
    // Each mode contains the direction for the projectile to fire.
    // The projectile is rotated before firing so the sprites faces the same way.
    void Update() {
        Vector3 dir;
        float zRot;

        switch (mode) {
            case 0: // → 
                dir  = Vector3.right;
                zRot = 180f;
                break;

            case 1: // ← 
                dir  = Vector3.left;
                zRot =   0f;
                break;

            case 2: // ↑ 
                dir  = Vector3.up;
                zRot = -90f;
                break;

            case 3: // ↓ 
                dir  = Vector3.down;
                zRot =  90f;
                break;

            case 4: // [o]
                dir  = aimbot * 0.5f;
                zRot = Mathf.Atan2(aimbot.y, aimbot.x) * Mathf.Rad2Deg + 180f;
                break;

            default:
                return;
        }

        transform.rotation = Quaternion.Euler(0f, 0f, zRot);
        transform.position += dir * speed * Time.deltaTime;
    }
}
