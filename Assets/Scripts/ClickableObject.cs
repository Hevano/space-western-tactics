using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ClickableObject : MonoBehaviour
{
    public static bool clicked = false;
    public delegate void MouseDownHandler(int button);
    public event MouseDownHandler onMouseDown;
    private void OnMouseOver() {
        if(onMouseDown != null && !clicked){
            if(Input.GetMouseButton(0)){ //left click
                clicked = true;
                onMouseDown(0);
            } else if(Input.GetMouseButton(1)){ //right click
                clicked = true;
                onMouseDown(1);
            } else if(Input.GetMouseButton(2)){ //middle click
                clicked = true;
                onMouseDown(2);
            }
        }
        
    }
}
