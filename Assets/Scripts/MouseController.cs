using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Contains all the click and hover handlers
public class MouseController : MonoBehaviour
{
    public static MouseController controller;

    public AttackIntent intent;
    public Character character; //Should be a property so the proper logic can occur when it is selected;

    public bool dodging = false; //Should probably be a character property
    private GridTile dodgeTile;

    //Should probably be in turn manager
    public IEnumerator DodgeCoroutine(Attack attack, Character target) {
        if(attack.owner.hostile && attack.accuracy > target.Dodge){
            target.ApplyAttack(attack, false);
        } else if(attack.owner.hostile){
            //Enable dodge UI here
            character = target;
            CombatUI.ui.SetCharacter(character);
            intent.ActivateDodgeIntent(character, attack);
            GridMap.map.CalculateDistances(character.x, character.y, DodgeDisplay);
            dodging = true;
            while(dodging){
                yield return new WaitForFixedUpdate();
            }
            character.ApplyAttack(attack, dodgeTile != null);
            dodgeTile = null;
            //Disbale dodge UI here
            intent.gameObject.SetActive(false);
        } else {
            //Include AI Tree for dodging
            if(target.Dodge > attack.accuracy){
                //Get Valid Tiles (Could do this manually for each direction for better performance)
                GridMap.map.CalculateDistances(target.x, target.y);
                var openings = GridMap.map.GetAdjacent(target.x, target.y, (g) => {return g.Character == null;});

                if(openings.Count == 0){
                    target.ApplyAttack(attack, false);
                }

                dodgeTile = openings[Random.Range(0, openings.Count)];
                target.Move(dodgeTile.x, dodgeTile.y);
                dodgeTile = null;
                //Move to a random tile in that list
                target.ApplyAttack(attack, true);
            } else {
                target.ApplyAttack(attack, false);
            }
        }
    }

    private void DodgeDisplay(GridTile t){
        var mat = t.gameObject.GetComponent<Renderer>().material;
        if(!t.blocked && t.visited == 1){
            //mat.SetColor("HighlightColor", Color.green);
            mat.SetFloat("Enabled", 1.0f);
        } else {
            //mat.SetColor("HighlightColor", Color.red);
            mat.SetFloat("Enabled", 0.0f);
        }
    }

    private void Awake() {
        if(controller != null){
            Destroy(this);
        }
        controller = this;
    }
    
    public void GridTileClick(GridTile tile){
        if(!dodging ){
            bool moved = character.MoveAction(tile.x, tile.y);
            //TODO: Effects if move was successful / unsucessful
        }else if(dodging && tile.Character == character){
            dodging = false;
        } else if(dodging && !tile.blocked && tile.Character == null && tile.visited == 1){
            character.Move(tile.x, tile.y);
            dodgeTile = tile;
            dodging = false;
        } else if(tile.Character != null){
            CharacterClick(tile.Character, 0);
        }
    }

    public void CharacterClick(Character c, int button){
        if(button != 0){
            return;
        } else if(dodging){
            if(c == character){
                dodging = false;
            }
        } else if(!c.hostile){
            character = c;
            CombatUI.ui.SetCharacter(character);
            GridMap.map.CalculateDistances(character.x, character.y, character.MovementDisplay);
        } else if(character != null && character.Action){
            character.Action = false;
            character.Attack(c);
            //attack should return bool of success, then we can do some feedback based on result
        }
    }

    public void CharacterHoverEnter(Character c){
        if(!dodging && c.hostile){
            intent.ActivateIntent(c, character.weapon);
        }
    }

    public void CharacterHoverExit(Character c){
        if(!dodging && c.hostile){
            intent.gameObject.SetActive(false);
        }
    }

    private void Update() {
        if(Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2)) ClickableObject.clicked = false;
    }
}
