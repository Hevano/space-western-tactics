using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider))]
public class HoverableObject : MonoBehaviour
{
    public delegate void MouseHoverHandler();
    public event MouseHoverHandler onMouseEnter;
    public event MouseHoverHandler onMouseExit;
    public event MouseHoverHandler onMouseOver;
    private void OnMouseEnter() {
        if(onMouseEnter != null){
            onMouseEnter();
        }
    }

    private void OnMouseExit() {
        if(onMouseExit != null){
            onMouseExit();
        }
    }

    private void OnMouseOver() {
        if(onMouseOver != null && !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2)){
            onMouseEnter();
        }
    }
}
