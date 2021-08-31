using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Character))]
public class CharacterUI : MonoBehaviour
{
    static GameObject uiPanelPrefab;
    static GameObject canvas; //Replace with singleton ui class
    public GameObject uiPanel; //Should probably be a UI layout
    public Text actionText;
    public Text movementText;

    public Character character;

    public Vector3 offset = Vector3.forward;

    private void Start() {
        character = GetComponent<Character>();
        if(character == null){
            print("ERROR | Character ui on object without character");
            Destroy(this);
        }

        if(uiPanelPrefab == null){
            uiPanelPrefab = Resources.Load<GameObject>("Prefabs/CharacterUIPanel");
            canvas = GameObject.FindGameObjectWithTag("UI");
        }
        uiPanel = Instantiate(uiPanelPrefab, Vector3.zero, Quaternion.identity, canvas.transform);
        movementText = uiPanel.GetComponent<CharacterPanelUI>().movementText;
        actionText = uiPanel.GetComponent<CharacterPanelUI>().actionText;

        character.ui = this;
        character.onStatChange += UpdateAction;
        character.onStatChange += UpdateMovement;
    }

    private void Update() {
        uiPanel.transform.position = Camera.main.WorldToScreenPoint(character.transform.position + offset);
    }

    private void UpdateMovement(bool applied){
        movementText.text = $"movement: {character.MovementRange} / {character.data.movementRangeMax}";
    }

    private void UpdateAction(bool applied){
        if(character.Action){
            actionText.text = "Action Available";

        } else {
            actionText.text = "Action Spent";
        }
    }
}
