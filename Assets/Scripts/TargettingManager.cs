using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//
// 스킬 정보를 입력받아 타겟팅 환경을 세팅하는 클래스
// 

public class TargettingManager : MonoBehaviour
{
    BattleSceneManager bcm; // bcm의 커맨드 리스트에 등록을 위한 접근
    Command curCommand; // 마커 측에서 이걸 읽게 될것임
    


    private void Start()
    {
        bcm = FindObjectOfType<BattleSceneManager>();
    }

    // 타겟들에게 타겟팅 마커 생성
    public void CreateTargetWindow(Command _comm)
    {
        // 현재 타겟팅 설정중인 스킬을 등록
        curCommand = _comm;


        // 스킬 정보에 맞춰 마커 생성
        // 마커 스폰? 혹은 전투슬롯에 내장?
        // 스폰시키는 쪽이 스킬 범위에 맞춰 세팅할 수 있는가?

        BattleSlot[] slots;

        switch (_comm.GetTargetType())
        {
            case Command.Target.ENEMY: // 적측에 생성
                slots = bcm.GetEnemySlots();
                for (int i = 0; i < slots.Length; i++)
                {
                    slots[i].EnableMarker();
                }
                break;

            case Command.Target.HERO: // 아군측에 생성
                slots = bcm.GetHeroSlots();
                for (int i = 0; i < slots.Length; i++)
                {
                    slots[i].EnableMarker();
                }
                break;

            case Command.Target.GLOBAL: // 모든 측에 생성
                slots = bcm.GetAllSlots();
                for (int i = 0; i < slots.Length; i++)
                {
                    slots[i].EnableMarker();
                }
                break;
        }
        
    }

    
    public Command GetCurrentCmd()
    {
        if (curCommand != null)
        {
            return curCommand;
        }
        else
        {
            return null;
        }
    }

    public void SetTargetIndex(int _idx)
    {
        curCommand.SetTargetIndex(_idx);
    }
    
}
