using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttackIntent : MonoBehaviour
{
    public LineRenderer line;
    public Gradient validColor;
    public Gradient invalidColor;

    public GameObject uiPanel;
    public Text accuracyValue;
    public Text damageValue;
    private Character target;
    private WeaponData weapon;
    private Attack attack;


    void OnEnable()
    {
        uiPanel.SetActive(true);
    }

    private void OnDisable() {
        uiPanel.SetActive(false);
    }

    public void ActivateIntent(Character t, WeaponData w){ //Probably should cache the results
        if(target == t && weapon == w && gameObject.activeSelf){
            return;
        }
        weapon = w;
        target = t;
        attack = null;
        if(weapon.CheckTarget(target)){
            line.colorGradient = validColor;
        } else {
            line.colorGradient = invalidColor;
        }
        accuracyValue.text = $"{weapon.accuracy.Min}-{weapon.accuracy.Max}";
        damageValue.text = weapon.damage.ToString();
        SetPositions(target.gameObject.transform.position, weapon.owner.gameObject.transform.position);
        gameObject.SetActive(true);
    }

    public void ActivateDodgeIntent(Character t, Attack a){
        target = t;
        attack = a;
        weapon = null;
        line.colorGradient = validColor;
        accuracyValue.text = $"{attack.accuracy}";
        damageValue.text = attack.damage.ToString();
        t.onAttackApplied += (a, b)=>{gameObject.SetActive(false);};
        SetPositions(target.gameObject.transform.position, attack.owner.gameObject.transform.position);
        gameObject.SetActive(true);
    }

    private void SetPositions(Vector3 start, Vector3 end){
        line.SetPosition(0, start + Vector3.up);
        line.SetPosition(1, end + Vector3.up);
        uiPanel.transform.position = Camera.main.WorldToScreenPoint((start + end) / 2);
    }
}
