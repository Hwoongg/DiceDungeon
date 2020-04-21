using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//
// 하위 클래스 샌드박스 패턴으로 작성할 필요 있어보임
// 

public abstract class Command 
{

    protected string name; // 스킬명
    protected string description; // 스킬 내용
    

    // 타겟 정보 (필드 정보)
    protected GameObject caster; // 시전자
    protected List<GameObject> targets;
    protected int diceValue;

    
    public enum Target // 마커 생성용 대상 범주
    {
        ENEMY, // 적
        HERO, // 영웅측
        GLOBAL // 전체
    }

    public enum Type // 범위 표시 색상 설정용
    {
        ATTACK, //공격
        HEAL, //회복
        BUFF // 버프 및 특수효과
    }

    protected Target target; // 대상의 종류
    protected Type type; // 스킬 타입

    // 마커가 찍은 범위로부터 어떤식의 범위인지
    // 0은 찍힌 대상 인덱스
    protected int targetIndex;
    protected int[] rangeIndex;
    
    

    public abstract void Execute();

    public Command()
    {
        targets = new List<GameObject>();
    }

    
    public string GetName()
    {
        return name;
    }

    public int GetDiceValue()
    {
        return diceValue;
    }

    public string GetDesc()
    {
        return description;
    }
    

    public int[] GetRangeIndex()
    {
        return rangeIndex;
    }

    public Target GetTargetType()
    {
        return target;
    }

    public void SetTargetIndex(int _idx)
    {
        targetIndex = _idx;
    }
    public int GetTargetIndex()
    {
        return targetIndex;
    }

    public void AddTarget(GameObject _o)
    {
        targets.Add(_o);
    }

    public void ClearTargetList()
    {
        targets.Clear();
    }

    public void SetCaster(GameObject _o)
    {
        caster = _o;
    }

    public GameObject GetCaster()
    {
        return caster;
    }

    public void SetDiceValue(int _val)
    {
        diceValue = _val;
    }

    // 진영 전체 정보를 받아서 자동 타겟팅. 몬스터꺼만 만듬
    virtual public void SetAutoTarget(List<GameObject> heros, List<GameObject> enemies)
    {
        // 상속받아 알아서. 히어로측은 안만들거라 순수가상으로 안만듬.
        Debug.Log("자동시전 AI가 설정되지 않았습니다.");

        // 기본적으로 영웅측 1번을 때리도록 설정한다
        targets.Clear();
        targetIndex = 0;

        for (int i = 0; i < rangeIndex.Length; i++)
        {
            // 슬롯 인덱스 추출
            int idx = targetIndex + rangeIndex[i];

            // 유효성 판단
            if (idx > heros.Count || idx < 0)
                continue;
            

            // 대상도 그냥 등록해버릴까? 생각보다 범위 판단하는게 귀찮다
            // 스킬마다 이거 검사하는것도 일일듯
            AddTarget(heros[targetIndex].gameObject);

        }

    }
}


// 가디언 1번
public class Cover : Command
{
    public Cover()
    {
        target = Target.HERO;
        type = Type.BUFF;

        name = "Cover";
        description = "보호 스킬";

        rangeIndex = new int[2];
        rangeIndex[0] = 0;
        rangeIndex[1] = 1;
    }

    public override void Execute()
    {
        // 시전자 어그로 증가
        // ...

        for(int i=0; i<targets.Count; i++)
        {
            targets[i].GetComponent<BattleSlot>().GetBattleUnit().aggro -= diceValue;
        }
    }
}

// 가디언 2번
public class Bash : Command
{
    public Bash()
    {
        target = Target.ENEMY;
        type = Type.ATTACK;

        name = "Bash";
        description = "공격 스킬";

        rangeIndex = new int[1];
        rangeIndex[0] = 0;
    }

    public override void Execute()
    {
        // 시전자 어그로 증가
        caster.GetComponent<BattleSlot>().GetBattleUnit().aggro += 2;

        // 대상들에게 데미지
        for (int i = 0; i < targets.Count; i++)
        {
            targets[i].GetComponent<BattleSlot>().TakeDamage(diceValue);
            Debug.Log("배쉬 실행");
        }
    }
}

// 가디언 3번
public class FullGuard : Command
{
    public FullGuard()
    {
        target = Target.HERO;
        type = Type.BUFF;

        name = "FullGuard";
        description = "방어 버프";

        rangeIndex = new int[1];
        rangeIndex[0] = 0;
    }

    public override void Execute()
    {
        
    }
}

// 아처 1번
public class ArrowRain:Command
{
    public override void Execute()
    {
        for(int i=0; i<targets.Count; i++)
        {
            targets[i].GetComponent<BattleSlot>().GetBattleUnit().curHp -= diceValue;
        }
    }
}

// 아처 2번
public class PowerShot : Command
{
    public override void Execute()
    {
        for(int i=0; i<targets.Count; i++)
        {
            targets[i].GetComponent<BattleSlot>().GetBattleUnit().curHp -= diceValue;
        }
    }
}

// 아처 3번
public class Charge:Command
{
    public override void Execute()
    {
        
    }
}

// 으르렁대기 스킬
public class Growl : Command
{
    
    public Growl()
    {
        rangeIndex = new int[1];
        rangeIndex[0] = 0;
    }
    public override void Execute()
    {
        for (int i = 0; i < targets.Count; i++)
        {

            targets[i].GetComponent<BattleSlot>().TakeDamage(diceValue);
        }
    }

    
}

// 물기 스킬
public class Bite : Command
{
    public Bite()
    {
        rangeIndex = new int[1];
        rangeIndex[0] = 0;
    }
    public override void Execute()
    {
        for (int i = 0; i < targets.Count; i++)
        {

            targets[i].GetComponent<BattleSlot>().TakeDamage(diceValue);
        }
    }

    
}

// 베어가르기
public class Slash : Command
{
    public Slash()
    {
        rangeIndex = new int[1];
        rangeIndex[0] = 0;
    }
    public override void Execute()
    {
        for (int i = 0; i < targets.Count; i++)
        {

            targets[i].GetComponent<BattleSlot>().TakeDamage(diceValue);
        }
    }
}