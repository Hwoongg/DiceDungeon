using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// //////////////////////////////////////////
//
// 전투 시 적측 AI 실행시켜주는 클래스
//
// //////////////////////////////////////////

public class EnemyAI : MonoBehaviour
{
    // BCM 참조 필요. 전장 상태 받아와야 함.
    BattleSceneManager bcm;

    BattleSlot[] EnemySlots;

    int playIndex;

    bool isEnemyTurn;
    bool isSetAction;

    private void Start()
    {
        bcm = FindObjectOfType<BattleSceneManager>();
    }

    private void Update()
    {
        if (isEnemyTurn)
        {
            if (isSetAction == false)
            {
                EnemyAction(EnemySlots[playIndex]);
                isSetAction = true;
            }

            // 액션 세팅된 상태에서
            if (isSetAction == true)
            {
                // 애니메이션 꺼지면
                if (EnemySlots[playIndex].IsAnimating() == false)
                {
                    // 다음 인덱스로 이동
                    playIndex++;
                    isSetAction = false;

                    if (playIndex >= EnemySlots.Length)
                        isEnemyTurn = false;
                }
            }

        }
    }

    public void SetEnemySlot()
    {
        EnemySlots = bcm.GetEnemySlots();
    }

    public void EnemyAction(BattleSlot _bs)
    {
        Command curCommand = _bs.GetCurrentCommand();

        // 시전자 설정
        curCommand.SetCaster(_bs.gameObject);

        // 대상 자동 설정
        curCommand.SetAutoTarget(bcm.GetSlotObjects(), bcm.GetSlotObjects(false));

        // 전투 실행
        curCommand.GetCaster().GetComponent<BattleSlot>().PlaySkill();

        // 타겟팅 시스템 참조할 필요 없을 것.
        
    }

    public void SetEnemyDice()
    {
        // 적 개수만큼 주사위 생성
        EventDice[] dices = bcm.CreateDice(EnemySlots.Length);
        

        // 각 슬롯에 세트 SetDice() 해야한다
        for (int i=0; i<EnemySlots.Length; i++)
        {
            EnemySlots[i].SetDice(dices[i]);
        }
    }

    public void StartEnemyTurn()
    {
        playIndex = 0;
        isEnemyTurn = true;
    }
}
