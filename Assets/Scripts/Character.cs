using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Should break this into an enemy and a player class
public class Character : MonoBehaviour, IMoveable {
    public static float CHARACTER_MOVEMENT_SPEED = 0.1f; //Should probably scale based on length of the move
    
    public bool hostile = false;
    public int x;
    public int y;

    //Rpg Stats
    public int dodgeMax = 10;
    public int dodge = 10;

    public int Dodge {
        get {
            return dodge;
        }
        set {
            TextCallout.NewCallout(transform.position, $"{-(dodge - value)} Dodge");
            dodge = Mathf.Clamp(value, 0, int.MaxValue);
        }
    }
    public int health = 10;

    public int Health {
        get {
            return health;
        } set {
            TextCallout.NewCallout(transform.position, $"{-(health - value)} Health");
            health = Mathf.Clamp(value, 0, int.MaxValue);
            if(health == 0 && !hostile){
                TurnManager.manager.playerParty.Remove(this);
                Destroy(ui.uiPanel);
                Destroy(gameObject);
            } else if(health == 0){
                var indexOf =  TurnManager.manager.enemies.IndexOf(this);
                TurnManager.manager.enemies.Remove(this);
                TurnManager.manager.enemyAgents.RemoveAt(indexOf);
                Destroy(gameObject);
            }
        }
    }

    public int armor = 1;

    public WeaponData weapon;
    
    [SerializeField]
    private bool _action = true;
    
    //This really needs a rework, once some game logic has been figured out
    [SerializeField]
    public bool Action { 
        get{
            return _action;
        } 
        set{
            _action = value;
        }}

    public int movementRange = 5;
    public int movementRangeCurrent = 5;

    //UI Display
    public CharacterUI ui;

    //Character events
    public delegate void MoveHandler(int newX, int newY);
    public event MoveHandler onMove;
    public delegate void ActionHandler(ActionEnum type);
    public event ActionHandler onAction;
    public delegate void AttackHandler(Attack a, Character t);
    public event AttackHandler onAttack;

    public delegate void AttackReceivedHandler(Attack a);
    public event AttackReceivedHandler onAttackReceived;

    public delegate void AttackAppliedHandler(Attack a, bool dodged);
    public event AttackAppliedHandler onAttackApplied;

    public delegate void StatChangeHandler(bool applied); //probably need additional data to check what was changed
    public event StatChangeHandler onStatChange;

    private List<Vector3> path = null;

    //Prevent from moving into another character's space
    public bool Move(int endX, int endY){
        if(-1 > endX || endX > GridMap.map.columns || -1 > endY || endY > GridMap.map.rows){
            print($"{gameObject.name} cannot move outside of bounds ({endX}, {endY}).");
            return false;
        }
        GridMap.map.CalculateDistances(x, y);
        var list = GridMap.map.GetPath(endX, endY);
        if(list == null){
            print("Cannot move, path is not valid");
            return false;
        }
        if(!MouseController.controller.dodging && (list == null || list.Count - 1 > movementRangeCurrent)){ //Jank code, refactor(?)
            print($"{gameObject.name} cannot move {list.Count} tiles. {movementRangeCurrent} movement remaining.");
            return false;
        }
        path = new List<Vector3>();
        foreach(GridTile t in list){
            path.Add(t.transform.position);
        }
    
        if(onMove != null){
            onMove(endX, endY);
        }
        GridMap.map.grid[x,y].Character = null;
        x = list[path.Count - 1].x;
        y = list[path.Count - 1].y;
        list[path.Count - 1].Character = this;

        

        return true;
    }

    public bool MoveAction(int endX, int endY){
        if(Move(endX, endY)){
            movementRangeCurrent -= path.Count - 1;
            GridMap.map.CalculateDistances(x, y, this.MovementDisplay);

            if(onStatChange != null){
                onStatChange(true);
            }
            return true;
        }
        return false;
    }

    private void MoveAlongPath(){
        if(path != null){
            transform.position = Vector3.Lerp(transform.position, path[0], CHARACTER_MOVEMENT_SPEED);
                if(Vector3.Distance(transform.position, path[0]) < 0.01f){
                    path.RemoveAt(0);
                }
                if(path.Count == 0){
                    path = null;
                }
            }
        
    }

    //Separate into 2 functions, one that handles gameplay mechanics and one that only applies the attack to the target
    public void Attack(Character target, Attack specialAttack = null){
        if(!weapon.CheckTarget(target)) return;
        Attack attack = (specialAttack != null) ? specialAttack : weapon.GetAttack();
         
        
        if(attack.keywords.Contains(WeaponKeywordEnum.Heavy) && movementRangeCurrent != movementRange) return;

        if(onAction != null){
            onAction(ActionEnum.Attack);
        }

        if(onAttack != null){
            onAttack(attack, target);
        }

        if(attack.keywords.Contains(WeaponKeywordEnum.Recoil)){
            if(Dodge - 2 < 0){
                attack.accuracy = Mathf.Max(attack.accuracy - 2, 0); //reduce accuracy to a minimum of 0
            } else {
                Dodge = Mathf.Max(Dodge - 2, 0);
            }
        }

        if(attack.keywords.Contains(WeaponKeywordEnum.Heavy)){
            movementRangeCurrent = 0;
        }


        weapon.shotsCurrent -= 1;
        //Attack effects or animation
        target.ReceiveAttack(attack);

        if(onStatChange != null){
            onStatChange(false);
        }
    }

    public void ReceiveAttack(Attack attack){

        if(onAttackReceived != null){
            onAttackReceived(attack);
        }
        StartCoroutine(MouseController.controller.DodgeCoroutine(attack, this));
    }

    public void ApplyAttack(Attack attack, bool dodged){
        if(onAttackApplied != null){
            onAttackApplied(attack, dodged);
        }
        if(dodged){
            Dodge -= attack.accuracy;
        } else {
            if(attack.keywords.Contains(WeaponKeywordEnum.ShallowDamage) && armor > 0){
                attack.damage /= 2;
            }
            attack.damage -= armor;
            Health -= attack.damage;
            if(attack.keywords.Contains(WeaponKeywordEnum.Powerful)){
                Vector3 direction = GridMap.map.grid[x, y].transform.position - GridMap.map.grid[attack.owner.x, attack.owner.y].transform.position;
                direction.Normalize();
                var behind = GridMap.map.GetTilesFromLine(GridMap.map.grid[x, y].transform.position, direction);
                if(behind.Count > 0 && !behind[0].blocked){
                    Move(behind[0].x, behind[0].y);
                }
            }
        }
        if(onStatChange != null){
            onStatChange(false);
        }

        if(attack.keywords.Contains(WeaponKeywordEnum.Collateral)){
            //Find behind location XD
            Vector3 direction = GridMap.map.grid[x, y].transform.position - GridMap.map.grid[attack.owner.x, attack.owner.y].transform.position;
            direction.Normalize();
            var behind = GridMap.map.GetTilesFromLine(GridMap.map.grid[x, y].transform.position, direction);
            if(behind.Count > 0 && behind[0].Character != null){
                attack.keywords.Remove(WeaponKeywordEnum.Collateral);
                attack.owner.Attack(behind[0].Character, attack);
            }
        }
    }

    public void NewTurn(){
        movementRangeCurrent = movementRange;
        Action = true;
        if(onAction != null){
            onAction(ActionEnum.Refresh);
        }

        if(onStatChange != null){
            onStatChange(true);
        }
    }

    public void Reload(){
        Action = false;

        weapon.shotsCurrent = weapon.shotsMax;
        if(onAction != null){
            onAction(ActionEnum.Reload);
        }
        if(onStatChange != null){
            onStatChange(true);
        }
    }

    public void Hunker(){
        Dodge = 0;
        Action = false;
        print("Hunkering down");
        if(onAction != null){
            onAction(ActionEnum.Hunker);
        }
        if(onStatChange != null){
            onStatChange(true);
        }
        ActionHandler dodgeRefresh = null;
        dodgeRefresh = (actionType) => {
            if(actionType == ActionEnum.Refresh){
                print("Hunker down completed");
                Dodge = dodgeMax;
                onAction -= dodgeRefresh;
            }
        };
        onAction += dodgeRefresh;
    }

    public void MovementDisplay(GridTile t){ //Could this be moved into the mouse controller class, or even the gridmap class?
        var mat = t.gameObject.GetComponent<Renderer>().material;
        if(!t.blocked && t.visited <= movementRangeCurrent && t.visited != -1){
            //mat.SetColor("HighlightColor", Color.green);
            mat.SetFloat("Enabled", 1.0f);
        } else {
            //mat.SetColor("HighlightColor", Color.red);
            mat.SetFloat("Enabled", 0.0f);
        }
    }

    private void Start() {
        weapon = WeaponData.GetRandomWeapon();
        weapon.owner = this;
        if(hostile){
            weapon.shotsCurrent = 99;
        }
    }

    private void Update() {
        MoveAlongPath();
    }
}

public enum ActionEnum{
    Attack,
    Refresh, //Perhaps could be renamed, denotes an action has been refreshed
    Reload,
    Hunker
}