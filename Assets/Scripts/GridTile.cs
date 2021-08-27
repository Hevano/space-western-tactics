using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile : MonoBehaviour
{
    public int x;
    public int y;
    public int visited = -1;

    public bool blocked = false;
    
    private Character _character;
    public Character Character{
        get{
            return _character;
        }
        set{
            _character = value;
        }
    }

    private void Start() {
        //Copy material so each tile can have it's own instance
        var renderer = GetComponent<Renderer>();
        renderer.material = new Material(renderer.material);
    }
}
