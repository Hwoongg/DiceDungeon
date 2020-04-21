using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : BattleUnit
{
    public Hero()
    {
        name = "hero";
        maxHp = 20;
        curHp = maxHp;
        power = 1;
        defence = 1;
        agility = 1;
        aggro = 1;
        
    }
    
}
