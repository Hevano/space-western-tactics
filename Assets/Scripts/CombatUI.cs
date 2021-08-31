using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatUI : MonoBehaviour
{
    public static CombatUI ui;
    public Canvas canvas;
    public Text characterName;
    public Text dodgeText;
    public Text armorText;
    public Text healthText;
    public Text accuracyText;
    public Text damageText;
    public Text keywordText;
    public Text rangeText;
    public Text shotsText;


    private Character selectedCharacter;

    private void Start() {
        if(ui != null){
            Destroy(this);
        }
        ui = this;
        
    }

    public void SetCharacter(Character c){
        if(selectedCharacter != null){
            selectedCharacter.onStatChange -= updateDodge;
            selectedCharacter.onStatChange -= updateArmor;
            selectedCharacter.onStatChange -= updateHealth;
            selectedCharacter.onStatChange -= updateShots;
        }
        selectedCharacter = c;
        characterName.text = selectedCharacter.name;
        updateDodge(true);
        updateArmor(true);
        updateHealth(true);
        updateShots(true);
        selectedCharacter.onStatChange += updateDodge;
        selectedCharacter.onStatChange += updateArmor;
        selectedCharacter.onStatChange += updateHealth;
        selectedCharacter.onStatChange += updateShots;

        accuracyText.text = $"Accuracy: {selectedCharacter.weapon.accuracy.Min} - {selectedCharacter.weapon.accuracy.Max}";
        damageText.text = $"Damage: {selectedCharacter.weapon.damage}";
        keywordText.text = "";
        foreach(WeaponKeywordEnum keyword in selectedCharacter.weapon.keywords){
            keywordText.text += System.Enum.GetName(typeof(WeaponKeywordEnum), keyword) + ", ";
        }

        string min = (selectedCharacter.weapon.minRange < 0 ? 0 : selectedCharacter.weapon.minRange).ToString();
        string max = (selectedCharacter.weapon.maxRange == int.MaxValue ? "\u221e" : selectedCharacter.weapon.maxRange.ToString());

        rangeText.text = $"Range: {min} - {max}";
    }

    private void updateDodge(bool applied){
        dodgeText.text = $"Dodge: {selectedCharacter.Dodge}";
    }

    private void updateArmor(bool applied){
        armorText.text = $"Armor: {selectedCharacter.Armor}";
    }

    private void updateHealth(bool applied){
        healthText.text = $"Health: {selectedCharacter.Health}";
    }

    private void updateShots(bool applied){
        shotsText.text = $"Shots: {selectedCharacter.weapon.shotsCurrent} / {selectedCharacter.weapon.shotsMax}";
    }

    //Temp
    public static void Reload(){
        if(ui != null && ui.selectedCharacter != null && ui.selectedCharacter.Action){
            ui.selectedCharacter.Reload();
        }
    }

    public static void Hunker(){
        if(ui != null && ui.selectedCharacter != null && ui.selectedCharacter.Action){
            ui.selectedCharacter.Hunker();
        }
    }
}
