using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    float speed = 0.1f;

    // Update is called once per frame
    void FixedUpdate()
    {
        if(Input.GetKey(KeyCode.W)){
            transform.position += transform.up * speed;
        }

        if(Input.GetKey(KeyCode.A)){
            transform.position -= transform.right * speed;
        }

        if(Input.GetKey(KeyCode.S)){
            transform.position -= transform.up * speed;
        }
        if(Input.GetKey(KeyCode.D)){
            transform.position += transform.right * speed;
        }
        
    }
}
