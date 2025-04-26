using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public int speed = 5;
    public int mode = 0;    
    void Update()
    {
        if (mode == 0) // >
            transform.position += new Vector3(1, 0) * Time.deltaTime * speed;
        else if (mode == 1) // <
            transform.position += new Vector3(-1, 0) * Time.deltaTime * speed;
        else if (mode == 2) // ^
            transform.position += new Vector3(0, 1) * Time.deltaTime * speed;            
        else if (mode == 3) // v
            transform.position += new Vector3(0, -1) * Time.deltaTime * speed;
        //2 track
        
    }
}
