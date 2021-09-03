using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeaponData {
    public Character owner;
    public ValueRange accuracy;

    public int damage;
    public int shotsCurrent;
    public int shotsMax;
    public int maxRange;
    public int minRange;
    public HashSet<WeaponKeywordEnum> keywords;

    public WeaponData(Character owner, ValueRange accuracy, List<WeaponKeywordEnum> keywords, int damage = 1, int shots = 1, int maxRange = int.MaxValue, int minRange = -1){
        this.owner = owner;
        this.damage = damage;
        this.accuracy = accuracy;
        this.minRange = minRange;
        this.maxRange = maxRange;
        this.shotsMax = this.shotsCurrent = shots;
        this.keywords = new HashSet<WeaponKeywordEnum>(keywords);
    }

    public Attack GetAttack(){
        return new Attack(owner, this, damage);
    }

    public bool CheckTarget(Character target){

        if(shotsCurrent <= 0) return false;

        //Check range
        var xDiff = Mathf.Abs(owner.x - target.x);
        var yDiff = Mathf.Abs(owner.y - target.y);

        if(xDiff < minRange || yDiff < minRange || xDiff > maxRange || yDiff > maxRange) { //This results in square ranges rather than circular radiuses
            MonoBehaviour.print($"attack out of range {xDiff}, {yDiff}");
            return false;
        }

        return GridMap.map.CheckLos(owner.gameObject.transform.position, target.transform.position);
    }

    //Temp
    public static WeaponData GetRandomWeapon(){
        var choice = Random.Range(0, 3);
        ValueRange accuracy;
        List<WeaponKeywordEnum> keywords = new List<WeaponKeywordEnum>();
        int damage;
        int shots;
        switch(choice){
            case 0: //Cannon
                accuracy = new ValueRange(2,4);
                damage = 6;
                shots = 1;
                keywords.Add(WeaponKeywordEnum.Heavy);
                keywords.Add(WeaponKeywordEnum.Collateral);
                return new WeaponData(null, accuracy, keywords, damage, shots);
            case 1: //AR
                accuracy = new ValueRange(2,6);
                damage = 3;
                shots = 5;
                keywords.Add(WeaponKeywordEnum.Recoil);
                return new WeaponData(null, accuracy, keywords, damage, shots);
            case 2: //Shotgun
                accuracy = new ValueRange(1,12);
                damage = 5;
                shots = 2;
                keywords.Add(WeaponKeywordEnum.ShallowDamage);
                return new WeaponData(null, accuracy, keywords, damage, shots);
            default: //Revolver
                accuracy = new ValueRange(3,4);
                damage = 2;
                shots = 6;
                return new WeaponData(null, accuracy, keywords, damage, shots);
        }
    }
}

public class Attack {
    public Character owner;
    public WeaponData source;
    public HashSet<WeaponKeywordEnum> keywords;
    public int damage;
    public int accuracy;

    public Attack(Character owner, WeaponData source, int damage = 1){
        this.owner = owner;
        this.source = source;
        this.damage = damage;
        accuracy = source.accuracy.Value;
        this.keywords = new HashSet<WeaponKeywordEnum>(source.keywords);
    }
}

public struct ValueRange {
    private int dieNum;
    private int dieSize;
    private int bonus;
    public int Max {
        get {
            return dieNum * dieSize + bonus;
        }
    }

    public int Min {
        get {
            return dieNum + bonus;
        }
    }

    public int Value {
        get {
            int sum = 0;
            for(int i = 0; i < dieNum; i++){
                sum += Random.Range(1, dieSize+1);
            }
            return sum + bonus;
        }
    }

    public ValueRange(int dieNum, int dieSize, int bonus = 0){
        this.dieNum = dieNum;
        this.dieSize = dieSize;
        this.bonus = bonus;
    }
}

public enum WeaponKeywordEnum{
    Powerful, //Tested
    Collateral, //Tested
    Recoil, //Tested
    Heavy, //Tested
    ShallowDamage //Tested
}

