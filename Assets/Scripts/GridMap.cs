using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMap : MonoBehaviour
{
    public static GridMap map;

    public static int MODULE_SIZE = 5;

    public GridTile[,] grid;
    public int rows;
    public int columns;
    public int scale;
    public GameObject gridPrefab;
    public Vector3 gridOrigin;

    public delegate void OnVisit(GridTile tile);
    public delegate bool OnEvaluate(GridTile tile); //poorly named


    private void Start() {
        if(map != null){
            Destroy(this);
        }
        map = this;
    }
    

    // Start is called before the first frame update
    void Awake()
    {
        GenerateGrid();
        CalculateDistances(0, 0);
    }

    public void GenerateGrid(){
        grid = new GridTile[rows, columns];
        for(int i = 0; i < rows; i++){
            for(int j = 0; j < columns; j++){
                GameObject tile = Instantiate(gridPrefab, new Vector3(gridOrigin.x + (j * scale), 0, gridOrigin.y + (i * scale)), Quaternion.identity);
                tile.transform.SetParent(gameObject.transform);
                grid[j, i] = tile.GetComponent<GridTile>();
                grid[j, i].x = j;
                grid[j, i].y = i;
                //Randomly block grid tiles
                grid[j, i].blocked = Random.Range(0.0f, 1.0f) > 0.9f;
                grid[j, i].gameObject.name = $"{grid[j, i].gameObject.name} {j} {i}";
                if(grid[j, i].blocked){
                    grid[j, i].gameObject.GetComponent<MeshRenderer>().enabled = false;
                }
            }
        }
    }

    public void GenerateGrid(MapModule[,] modules){
        rows = modules.GetLength(0);
        columns = modules.GetLength(1);
        for(int i = 0; i < rows; i++){ //Instantiate Props
            for(int j = 0; j < columns; j++){
                GameObject module = Instantiate(modules[j, i].gameObject, new Vector3(gridOrigin.x + (j * scale), 0, gridOrigin.y + (i * scale)), Quaternion.identity);
            }
        }
        rows *= MODULE_SIZE;
        columns *= MODULE_SIZE;
        grid = new GridTile[rows, columns];
        for(int i = 0; i < rows; i++){
            for(int j = 0; j < columns; j++){
                //Get Module coordinates
                int moduleX = (int) Mathf.Floor(j / MODULE_SIZE);
                int tileX = j - moduleX * MODULE_SIZE;
                int moduleY = (int) Mathf.Floor(i / MODULE_SIZE);
                int tileY = i - moduleY * MODULE_SIZE;
                GridTile tile = modules[moduleX, moduleY].grid[tileX, tileY];
                grid[j, i] = tile.GetComponent<GridTile>();
                grid[j, i].x = j;
                grid[j, i].y = i;
            }
        }

    }

    private void ClearPathData(int x, int y){
        foreach(GridTile g in grid){
            g.visited = -1;
            g.gameObject.GetComponent<Renderer>().material.SetFloat("Enabled", 0.0f);
        }
        grid[x,y].visited = 0;
    }

    //We could store the distance values in a hashmap with character - distance so as we can save the distances we calculate each turn
    public void CalculateDistances(int x, int y, OnVisit onVisitHandler = null){ //Add an optional delegate to perfrom on each grid tile
        var character = grid[x, y].Character;
        ClearPathData(x,y);
        var queue = new Queue<GridTile>();
        queue.Enqueue(grid[x,y]);
        while(queue.Count > 0){
            var tile = queue.Dequeue();
            if(tile.x - 1 >= 0 && grid[tile.x-1, tile.y].visited == -1 && !grid[tile.x-1, tile.y].blocked && (grid[tile.x-1, tile.y].Character == null || grid[tile.x-1, tile.y].Character == character)){
                grid[tile.x-1, tile.y].visited = tile.visited+1;
                queue.Enqueue(grid[tile.x-1, tile.y]);
            }
            if(tile.x + 1 < columns && grid[tile.x+1, tile.y].visited == -1 && !grid[tile.x+1, tile.y].blocked && (grid[tile.x+1, tile.y].Character == null || grid[tile.x+1, tile.y].Character == character)){
                grid[tile.x+1, tile.y].visited = tile.visited+1;
                queue.Enqueue(grid[tile.x+1, tile.y]);
            }
            if(tile.y - 1 >= 0 && grid[tile.x, tile.y-1].visited == -1 && !grid[tile.x, tile.y-1].blocked && (grid[tile.x, tile.y-1].Character == null || grid[tile.x, tile.y-1].Character == character)){
                grid[tile.x, tile.y-1].visited = tile.visited+1;
                queue.Enqueue(grid[tile.x, tile.y-1]);
            }
            if(tile.y + 1 < rows && grid[tile.x, tile.y+1].visited == -1 && !grid[tile.x, tile.y+1].blocked && (grid[tile.x, tile.y+1].Character == null || grid[tile.x, tile.y+1].Character == character)){
                grid[tile.x, tile.y+1].visited = tile.visited+1;
                queue.Enqueue(grid[tile.x, tile.y+1]);
            }
            if(onVisitHandler != null){
                onVisitHandler(tile);
            }
        }
    }

    //Potentially Cache the results so we can provide ui showing path before movement
    public List<GridTile> GetPath(int x, int y){
        if(x < 0 || x > columns || y < 0 || y > rows){
            return null;
        }

        var destination = grid[x,y];
        if(destination.visited == 0){
            var list = new List<GridTile>();
            list.Add(destination);
            return list;
        }


        if(x-1 >= 0 && grid[x-1,y].visited < destination.visited && !grid[x-1,y].blocked && grid[x-1,y].visited != -1){
            var list = new List<GridTile>();
            list.AddRange(GetPath(x-1,y));
            list.Add(destination);
            return list;
        } else if(x+1 < columns && grid[x+1,y].visited < destination.visited && !grid[x+1,y].blocked && grid[x+1,y].visited != -1){
            var list = new List<GridTile>();
            list.AddRange(GetPath(x+1,y));
            list.Add(destination);
            return list;
        } else if(y-1 >= 0 && grid[x,y-1].visited < destination.visited && !grid[x,y-1].blocked && grid[x,y-1].visited != -1){
            var list = new List<GridTile>();
            list.AddRange(GetPath(x,y-1));
            list.Add(destination);
            return list;
        } else if(y+1 < rows && grid[x,y+1].visited < destination.visited && !grid[x,y+1].blocked && grid[x,y+1].visited != -1){
            var list = new List<GridTile>();
            list.AddRange(GetPath(x,y+1));
            list.Add(destination);
            return list;
        }

        return null;
    }

    //These two methods could be optimized by filtering raycast by layer (?)
    public bool CheckLos(Vector3 pos, Vector3 target){
        Vector3 direction = target - pos;
        Ray los = new Ray(pos, direction);
        var tilesBetween = Physics.RaycastAll(los, direction.magnitude);
        foreach(RaycastHit hit in tilesBetween){
            GridTile tile; 
            if(hit.collider.gameObject.TryGetComponent<GridTile>(out tile)){
                if(tile.blocked) {
                    return false;
                };
            }
        }
        return true;
    }

    public List<GridTile> GetTilesFromLine(Vector3 start, Vector3 direction){
        var tiles = new List<GridTile>();
        Ray line = new Ray(start, direction);
        var hits = Physics.RaycastAll(line, direction.magnitude);
        foreach(RaycastHit hit in hits){
            GridTile tile;
            if(hit.collider.gameObject.TryGetComponent<GridTile>(out tile)){
                tiles.Add(tile);
            }
        }
        return tiles;
    }

    public List<GridTile> GetAdjacent(int x, int y, OnEvaluate evaluateHandler = null){
        if(x < 0 || x > columns || y < 0 || y > rows){
            return null;
        }

        if(evaluateHandler == null){
            evaluateHandler = (g) => { return true; };
        }

        var adj = new List<GridTile>();
        if(x-1 >= 0 && !grid[x-1,y].blocked && evaluateHandler(grid[x-1,y])){
            adj.Add(grid[x-1,y]);
        } else if(x+1 < columns && !grid[x+1,y].blocked && evaluateHandler(grid[x+1,y])){
            adj.Add(grid[x+1,y]);
        } else if(y-1 >= 0 && !grid[y-1,y].blocked && evaluateHandler(grid[y-1,y])){
            adj.Add(grid[x,y-1]);
        } else if(y+1 < rows && !grid[y+1,y].blocked && evaluateHandler(grid[y+1,y])){
            adj.Add(grid[x,y+1]);
        }

        return (adj.Count > 0) ? adj : null;
    }
}
