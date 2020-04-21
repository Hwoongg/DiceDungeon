using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : BattleUnit
{
    
    public Enemy()
    {
        name = "Enemy";
        maxHp = 10;
        curHp = maxHp;
        power = 0;
        aggro = 1;
        defence = 0;
        agility = 1;

        animPath = "UnitSprites/Enemy1";

        commands = new Command[3];
        commands[0] = new Growl();
        commands[1] = new Bite();
        commands[2] = new Slash();
    }


}
