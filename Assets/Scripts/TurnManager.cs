using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager manager;
    public bool turnBased = true;
    public int _turnNum = 1;
    public int TurnNum {
        get{
            return _turnNum;
        } 
        set{
            _turnNum = value;
            CombatUI.SetTurnNum(value);
        }
    }
    

    public List<Character> playerParty;
    public List<Character> enemies;
    public List<AIAgent> enemyAgents;


    private void Start() {
        if(manager != null){
            Destroy(this);
        }
        manager = this;
        int[] randomLocation = {0, 0};
        foreach(Character c in playerParty){
            do{
                randomLocation[0] = Random.Range(0, GridMap.map.columns);
                randomLocation[1] = Random.Range(0, GridMap.map.rows);
                c.x = randomLocation[0];
                c.y = randomLocation[1];
            } while(GridMap.map.grid[randomLocation[0], randomLocation[1]].blocked);
            c.Move(c.x, c.y);
        }
        enemyAgents = new List<AIAgent>();
        foreach(Character c in enemies){
            do{
                randomLocation[0] = Random.Range(0, GridMap.map.columns);
                randomLocation[1] = Random.Range(0, GridMap.map.rows);
                c.x = randomLocation[0];
                c.y = randomLocation[1];
            } while(GridMap.map.grid[randomLocation[0], randomLocation[1]].blocked);
            c.Move(c.x, c.y);
            enemyAgents.Add(new BasicEnemyAgent(c));
        }
        StartCoroutine(TurnBasedCombat());
    }

    IEnumerator TurnBasedCombat(){
        while(turnBased){
            yield return PlayerTurn();
            yield return EnemyTurn();
        }
    }

    IEnumerator PlayerTurn(){
        foreach(Character c in playerParty){
            c.NewTurn();
        }
        if(MouseController.controller.character != null){
            GridMap.map.CalculateDistances(MouseController.controller.character.x, MouseController.controller.character.y, MouseController.controller.character.MovementDisplay);
        }
        while(TurnNum % 2 != 0){
            yield return new WaitForFixedUpdate();
        }
    }
    IEnumerator EnemyTurn(){
        foreach(Character c in enemies){
            c.NewTurn();
        }
        while(TurnNum % 2 == 0){
            foreach(Character c in enemies){
                var ai = enemyAgents[enemies.IndexOf(c)];
                while(ai.Evaluate() == NodeState.RUNNING){
                    yield return new WaitForSeconds(1.0f);
                }
            }
            TurnNum++;
        }
    }

    public void EndPlayerTurn(){
        if(TurnNum % 2 != 0){
            TurnNum++;
        }
    }
}
