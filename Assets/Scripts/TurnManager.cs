using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager manager;
    public bool turnBased = true;
    public int turnNum = 1;

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
        while(turnNum % 2 != 0){
            yield return new WaitForFixedUpdate();
        }
    }
    IEnumerator EnemyTurn(){
        foreach(Character c in enemies){
            c.NewTurn();
        }
        while(turnNum % 2 == 0){
            foreach(Character c in enemies){
                var ai = enemyAgents[enemies.IndexOf(c)];
                while(ai.Evaluate() == NodeState.RUNNING){
                    yield return new WaitForSeconds(1.0f);
                }
            }
            turnNum++;
            // foreach(Character c in enemies){
            //     GridMap.map.CalculateDistances(c.x, c.y);
            //     if(GridMap.map.grid[MouseController.controller.character.x, MouseController.controller.character.y].visited < 5){
            //         Character.AttackAppliedHandler attackHandler = null;
            //         attackHandler = (a, dodged)=>{
            //             c.Action = false;
            //             print("Attack handled");
            //             MouseController.controller.character.onAttackApplied -= attackHandler;
            //         };
            //         MouseController.controller.character.onAttackApplied += attackHandler;
            //         c.Attack(MouseController.controller.character);
            //         while(c.Action){
            //             yield return new WaitForSeconds(1.0f);
            //         }
            //     } else {
            //         var x = MouseController.controller.character.x;
            //         var y = MouseController.controller.character.y;
            //         x = Random.Range(Mathf.Clamp(x-5, 0, GridMap.map.columns), Mathf.Clamp(x+5, 0, GridMap.map.columns));
            //         y = Random.Range(Mathf.Clamp(y-5, 0, GridMap.map.columns), Mathf.Clamp(y+5, 0, GridMap.map.columns));
            //         c.Move(x,y);
            //     }
            //     yield return new WaitForSeconds(1.0f);
            // }
            //turnNum++;
        }
    }

    public void EndPlayerTurn(){
        if(turnNum % 2 != 0){
            turnNum++;
        }
    }
}
