using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
// 캐릭터 애니메이션 패널에서 스킬 효과 사용을 위해 만든 클래스
//

public class CharacterAnimator : MonoBehaviour
{
    BattleSlot bs;

    private void Start()
    {
        bs = transform.parent.GetComponent<BattleSlot>();
    }
    public void CommandEfx()
    {
        Debug.Log("스킬 애니메이션 실행");

        bs.CommandEffect();
    }
    
}
