using UnityEngine;

public class ProjectileController : MonoBehaviour {
    public int speed = 5;
    public int mode = 0;

    private Vector3 Aimbot;
    private SpriteRenderer spriteRenderer;

    void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

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

            Aimbot = (target - transform.position).normalized;
        }
    }

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
                dir  = Aimbot * 0.5f;
                zRot = Mathf.Atan2(Aimbot.y, Aimbot.x) * Mathf.Rad2Deg + 180f;
                break;

            default:
                return;
        }

        transform.rotation = Quaternion.Euler(0f, 0f, zRot);
        transform.position += dir * speed * Time.deltaTime;
    }
}
