using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


/*
    Goals
    - Set of modules that can be easily created and configured from the editor
    - Modules strung together randomly to create levels
    - Modules should be able to be rotated
    - Module connections should be variable

    Approach
        - GridMap implemented as a 2d array
        - Receive 2d array of map modules
        - import map modules local grids into the gridmap

        - map generator selects a random spot in the 2d array
        - Gets connections from surrounding area
        - Gets a map module that matches those connects
        - Instantiates map module into world and places it into the 2d array
        - Add map module's connects to a queue
        - iterate queue and repeat above
*/


public class MapGen
{
    static List<GameObject> modulePrefabs;

    public static MapModule[,] GenerateLevel(int width = 2, int height = 2){
        modulePrefabs.AddRange(Resources.LoadAll<GameObject>("Prefabs/MapModules"));
        MapModule[,] level = new MapModule[width, height];

        var q = new  Queue<int[]>();


        var randPos = new int[]{Random.Range(0, width), Random.Range(0, height)};
        var adjConnections = GetAdjacentConnections(level, randPos[0], randPos[1]);
        level[randPos[0], randPos[1]] = GetModule(adjConnections).GetComponent<MapModule>();
        
        foreach(KeyValuePair<System.ValueTuple<int, int>, MapModuleConnection> c in level[randPos[0], randPos[1]].connections){
            if(!c.Value.completed){
                q.Enqueue(new int[]{randPos[0] + c.Key.Item1, randPos[1] + c.Key.Item2});
            }
        }

        while(q.Any()){
            randPos = q.Dequeue();
            adjConnections = GetAdjacentConnections(level, randPos[0], randPos[1]);
            level[randPos[0], randPos[1]] = GetModule(adjConnections).GetComponent<MapModule>();
            foreach(KeyValuePair<System.ValueTuple<int, int>, MapModuleConnection> c in level[randPos[0], randPos[1]].connections){
            if(!c.Value.completed){
                q.Enqueue(new int[]{randPos[0] + c.Key.Item1, randPos[1] + c.Key.Item2});
            }
        }
        }
        //Add Connections to queue
        //Loop through the queue

        return level;
    }

    public static List<MapModuleConnection> GetAdjacentConnections(MapModule[,] level, int x, int y){
        var adjConnections = new List<MapModuleConnection>();
        if(x + 1 == level.GetLength(0)){
            adjConnections.Add(MapModuleConnection.NewBlockedConnection(-1, 0));
        } else if(level[x + 1, y] != null) {
             adjConnections.Add(level[x + 1, y].connections[(-1, 0)]);
        }

        if(x - 1 == 0){
            adjConnections.Add(MapModuleConnection.NewBlockedConnection(1, 0));
        } else if(level[x - 1, y] != null) {
             adjConnections.Add(level[x + 1, y].connections[(1, 0)]);
        }

        if(y + 1 == level.GetLength(0)){
            adjConnections.Add(MapModuleConnection.NewBlockedConnection(0, -1));
        } else if(level[y + 1, x] != null) {
             adjConnections.Add(level[y + 1, x].connections[(0, -1)]);
        }

        if(y - 1 == 0){
            adjConnections.Add(MapModuleConnection.NewBlockedConnection(0, 1));
        } else if(level[y - 1, x] != null) {
             adjConnections.Add(level[y + 1, x].connections[(0, 1)]);
        }


        return adjConnections;
    }
    
    public static GameObject GetModule(List<MapModuleConnection> connections){
        var options = new List<GameObject>();
        foreach(GameObject module in modulePrefabs){
            var mapModule = module.GetComponent<MapModule>();
            bool valid = true;
            foreach(MapModuleConnection c in connections){
                valid = valid && mapModule.connections[(-c.offset[0], -c.offset[1])].CheckConnection(c);
            }
            if(valid) options.Add(module);
        }
        if (options.Count == 0) throw new System.Exception("Level generation error");
        return options[Random.Range(0, options.Count)];
    }
}

public struct MapModuleConnection{
    public int[] offset;
    public bool[] openings;

    public bool completed = false;

    public bool CheckConnection(MapModuleConnection other){
        return openings.SequenceEqual(other.openings.Reverse());
    }

    public static MapModuleConnection NewBlockedConnection(int offsetX, int offsetY){
        var connection = new MapModuleConnection();
        connection.offset = new int[]{offsetX, offsetY};
        return connection;
    }
}

// Load module prefabs
    // Construct 2d array of map modules
    // Iterate each tile in the grid
    // Check adjacent tiles for connections
    // If no adjacent connections, place random tile
    // Otherwise, find all map modules that fit the required connections
    // Pick on from that subset and place it in the array
    // Once module has been added, add it to a list of open connections


    //Select random cell in the array
    //Place a random valid module (No connections pointing outside the array)
    //Add the x,y positions in the array that the module has connections for
    //Iterate the queue, placing a valid tile at each of the connections and adding that tile's connection to the queue
