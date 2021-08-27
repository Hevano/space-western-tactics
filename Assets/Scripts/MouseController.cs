using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour
{
    public static MouseController controller;

    public AttackIntent intent;
    public Character character; //Should be a property so the proper logic can occur when it is selected;

    public bool dodging = false; //Should probably be a character property
    private GridTile dodgeTile;

    //Should probably be in turn manager
    public IEnumerator DodgeCoroutine(Attack attack, Character target) {
        if(attack.owner.hostile){
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
            intent.ActivateDodgeIntent(target, attack);
            if(target.dodge > attack.accuracy){
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
            yield return new WaitForSeconds(1.0f);
            intent.gameObject.SetActive(false);
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

    void Update () {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit);
        //Probably better off writing an interface for clickable and hoverable objects, would be cleaner and more extensible
        if (hit.collider != null) { 

            GridTile tile;
            Character characterTarget;
            if(Input.GetMouseButtonDown(0)){ //Mouse down cases
                if(hit.collider.gameObject.TryGetComponent<GridTile>(out tile)){ //Tile cases
                    if(dodging){ //Dodging Logic
                        if(tile.Character == character){
                            dodging = false;
                        } else if(!tile.blocked && tile.Character == null && tile.visited == 1){
                            character.Move(tile.x, tile.y);
                            dodgeTile = tile;
                            dodging = false;
                        }
                    } else if(character != null && !tile.blocked && tile.Character == null) { //Movement Logic
                        character.MoveAction(tile.x, tile.y);
                    }

                } else if (hit.collider.gameObject.TryGetComponent<Character>(out characterTarget)){ //Character Cases
                    if(dodging && characterTarget == character){
                        dodging = false;
                    } else if (!characterTarget.hostile) { //Character selection logic
                        character = characterTarget;
                        CombatUI.ui.SetCharacter(character);
                        GridMap.map.CalculateDistances(character.x, character.y, character.MovementDisplay);
                    } else if (characterTarget.hostile && character != null && character.Action){ //Attack logic
                        character.Action = false;
                        character.Attack(characterTarget);
                    }
                }

            } else { //Hover cases
                if(dodging){
                    //dodging has no hover behaviour
                } else if(hit.collider.gameObject.TryGetComponent<GridTile>(out tile)){ //Tile cases
                    //Implement movement path intents
                } else if (hit.collider.gameObject.TryGetComponent<Character>(out characterTarget)){ //Character Cases
                    if(characterTarget.hostile){ //Inefficient
                        intent.ActivateIntent(characterTarget, character.weapon);
                        return;
                    }
                }
            }

            if(!dodging && intent.gameObject.activeSelf){
                intent.gameObject.SetActive(false);
            }
        }
    }
}
