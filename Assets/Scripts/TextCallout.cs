using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Should be decoupled from the character class
public class TextCallout : MonoBehaviour
{
    public static Vector3 offset = Vector3.forward;
    public static GameObject textCalloutPrefab;

    public Text text;
    public void SetText(string newText){
        text.text = newText;
    }

    private void Update() {
        var newColor = text.color;
        newColor.a -= 0.001f;
        text.color = newColor;
        if(text.color.a < 0.001){
            Destroy(gameObject);
        }
    }
}
