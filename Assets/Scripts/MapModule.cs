using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapModule : MonoBehaviour
{
    public GridTile[,] grid;
    public Dictionary<System.ValueTuple<int, int>, MapModuleConnection> connections;

    /*
        Helper Functions:
        - Get Edge information for map generation
        - Get enemy spawn locations (?)
        - Delete self when no longer in use
        - Rotate gameobject and grid

        
    */
}
