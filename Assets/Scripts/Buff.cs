using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ///////////////////////////////////////////////////////////////////////////
//
// 기능이 일부씩만 존재할 것이기 때문에 인터페이스나 추상클래스로 작성하지 않음.
//
// ///////////////////////////////////////////////////////////////////////////

public class Buff
{
    public string name;

    public Buff()
    {
        name = "Default Buff";
    }

    public virtual void EndTurn(BattleSlot bs) { }// 턴 종료시 효과

    public virtual void Attack(BattleSlot bs) { }//공격시 효과

    public virtual void TakeDot(BattleSlot bs) { }// 피격시 효과
    
    public virtual void Add(BattleSlot bs)
    {
        // 이미 있는 버프인지 검사. 있다면 중첩처리 재정의
        // ...

        bs.AddBuff(this);
    }
}

public class Exhaust : Buff// 소진
{
    int factor;

    public Exhaust()
    {
        name = "Exhaust";
    }

    public void SetFactor(int _v)
    {
        factor = _v;
    }

    public override void EndTurn(BattleSlot bs)
    {
        // 턴종료시 인자값만큼 데미지 입는다. 취약효과 중첩 안되도록 도트피해함수 분리 필요
        bs.TakeTrueDmg(factor);

        // 인자값 감소
        factor -= 1;

        if(factor < 1)
        {
            // 디버프 소멸
            bs.RemoveBuff(this);
        }
    }
}

public class Stealth : Buff // 은신
{
    // 어그로를 0으로 취급한다.
    // 공격이나 어그로 최상위가 된다면 해제된다.

    public Stealth()
    {
        name = "Stealth";
    }
}

public class Weakening : Buff// 취약
{
    int factor;

    public int Factor
    {
        set
        {
            factor = value;
        }
    }

    public Weakening()
    {
        name = "Weakening";
    }

    public override void TakeDot(BattleSlot bs)
    {
        bs.TakeTrueDmg(factor);
    }
}

public class Reinforce : Buff // 증강
{
    public Reinforce()
    {
        name = "Reinforce";
    }
}