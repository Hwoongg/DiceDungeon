using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guardian : Hero
{
    public Guardian()
    {
        name = "guardian";
        maxHp = 50;
        curHp = maxHp;
        defence = 3;
        aggro = 3;

        // 자신의 이미지 불러오기
        charImage = Resources.Load<Sprite>("UnitSprites/Heros/Char1_0");

        // 자신의 애니메이션 경로 초기화
        animPath = "UnitSprites/BattleSlot";

        // 스킬 정보 생성
        commands = new Command[3];
        commands[0] = new Cover();
        commands[1] = new Bash();
        commands[2] = new FullGuard();
        
    }
}
