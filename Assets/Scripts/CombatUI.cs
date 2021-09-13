using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D.Animation;

public class CombatUI : MonoBehaviour
{
    public static CombatUI ui;
    public Canvas canvas;
    public SpriteLibraryAsset characterAssetLibrary;
    public Text characterName;
    public Text dodgeText;
    public Slider dodgeSlider;
    public Text armorText;
    public Text healthText;
    public Slider healthSlider;

    public Text weaponNameText;
    public Image weaponSprite;
    public Text accuracyText;
    public Text damageText;
    public Text keywordText;
    public Text rangeText;
    public Text shotsText;
    public GameObject movementHolder;

    public Text turnCountText;


    private Character selectedCharacter;

    private void Awake() {
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
        weaponNameText.text = selectedCharacter.weapon.name;
        weaponSprite.sprite = characterAssetLibrary.GetSprite("Weapon", selectedCharacter.weapon.spriteName);
        weaponSprite.rectTransform.sizeDelta = new Vector2(weaponSprite.sprite.rect.width, weaponSprite.sprite.rect.height);

        updateDodge(true);
        updateArmor(true);
        updateHealth(true);
        updateShots(true);
        selectedCharacter.onStatChange += updateDodge;
        selectedCharacter.onStatChange += updateArmor;
        selectedCharacter.onStatChange += updateHealth;
        selectedCharacter.onStatChange += updateShots;

        accuracyText.text = $"{selectedCharacter.weapon.accuracy.Min} - {selectedCharacter.weapon.accuracy.Max}";
        damageText.text = $"{selectedCharacter.weapon.damage}";
        keywordText.text = "";
        foreach(WeaponKeywordEnum keyword in selectedCharacter.weapon.keywords){
            keywordText.text += System.Enum.GetName(typeof(WeaponKeywordEnum), keyword) + ", ";
        }

        string min = (selectedCharacter.weapon.minRange < 0 ? 0 : selectedCharacter.weapon.minRange).ToString();
        string max = (selectedCharacter.weapon.maxRange == int.MaxValue ? "\u221e" : selectedCharacter.weapon.maxRange.ToString());

        rangeText.text = $"{min} - {max}";
    }

    private void updateDodge(bool applied){
        dodgeText.text = $"{selectedCharacter.Dodge} / {selectedCharacter.data.dodgeMax}";
        dodgeSlider.value = (float) selectedCharacter.Dodge / selectedCharacter.data.dodgeMax;
    }

    private void updateArmor(bool applied){
        armorText.text = $"{selectedCharacter.Armor}";
    }

    private void updateHealth(bool applied){
        healthText.text = $"{selectedCharacter.Health} / {selectedCharacter.data.healthMax}";
        healthSlider.value = (float) selectedCharacter.Health / selectedCharacter.data.healthMax;
    }

    private void updateShots(bool applied){
        shotsText.text = $"{selectedCharacter.weapon.shotsCurrent} / {selectedCharacter.weapon.shotsMax}";
    }

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
    public static GameObject NewCallout(Vector3 location, string text){
        Vector2 screenPos = Camera.main.WorldToScreenPoint(location + TextCallout.offset);
        if(TextCallout.textCalloutPrefab == null){
            TextCallout.textCalloutPrefab = Resources.Load<GameObject>("prefabs/textCalloutPrefab");
        }
        GameObject calloutObj = Instantiate(TextCallout.textCalloutPrefab, screenPos, Quaternion.identity, ui.canvas.transform);
        calloutObj.transform.SetAsLastSibling();
        var callout = calloutObj.GetComponent<TextCallout>();
        callout.SetText(text);
        return calloutObj;
    }

    public static void SetTurnNum(int num){
        ui.turnCountText.text = $"Turn {num}";
    }
}
