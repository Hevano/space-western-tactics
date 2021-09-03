using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterData
{
    public string name;
    public int dodge = 10;
    public int dodgeMax = 10;

    public int movementRange = 10;
    public int movementRangeMax= 10;

    public int health = 10;
    public int healthMax = 10;

    public int armor = 1;
    public int armorMax = 1;

    public bool action = true;
}
