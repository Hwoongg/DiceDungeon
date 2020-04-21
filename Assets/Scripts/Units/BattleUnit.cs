using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 전투 유닛 데이터 클래스
public class BattleUnit
{

    public string name;
    public int maxHp; // 체력
    public int curHp; // 현재체력
    public int power; // 데미지
    public int defence; // 방어력
    public int agility; // 민첩성. 행동력
    public int aggro; // 어그로 수치

    public Sprite charImage; // 캐릭터 이미지
    public string animPath;

    public Command[] commands;
    
    public BattleUnit()
    {
        name = "default";
        maxHp = 0;
        curHp = 0;
        power = 0;
        agility = 0;
        aggro = 0;
        defence = 0;
    }
}
