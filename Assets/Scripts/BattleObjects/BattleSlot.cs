﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class BattleSlot : MonoBehaviour
{
    bool isSelectable;

    //Button buttonComp;
    GameObject highlightObj;

    // 전투 유닛 정보
    BattleUnit battleUnit;

    Slider hpBar;

    BattleSceneManager bcm;

    Transform diceSlot;

    EventDice dice;

    Animator animator;

    TargettingManager tm;

    int slotIndex; // 자신의 슬롯 번호

    [SerializeField] GameObject MarkerButton;
    [SerializeField] GameObject ActionButton;
    [SerializeField] GameObject RangePannel;

    bool isAnimating;

    List<Buff> buffs;

    private void Awake()
    {
        animator = transform.Find("Img_Char").GetComponent<Animator>();
        slotIndex = 0;

    }
    void Start()
    {
        //buttonComp = GetComponent<Button>();
        highlightObj = transform.Find("Highlight").gameObject;
        //buttonComp.enabled = false;

        hpBar = GetComponentInChildren<Slider>();
        hpBar.value = battleUnit.curHp / battleUnit.maxHp;

        bcm = FindObjectOfType<BattleSceneManager>();

        diceSlot = transform.Find("DiceSlot");

        if (battleUnit.animPath != null)
        {
            animator.runtimeAnimatorController =
                Resources.Load<RuntimeAnimatorController>(battleUnit.animPath);
        }
        tm = FindObjectOfType<TargettingManager>();
    }

    public void Init(BattleUnit _unit)
    {
        battleUnit = _unit;
        
    }
    
    public void SetSelectable()
    {
        isSelectable = true;
        
    }

    public void SetDice(EventDice _dice)
    {
        // 이미 슬롯이 차있으면
        if(dice != null)
        {
            // 슬롯 비우기
            ClearSlot();
        }

        dice = _dice;
        _dice.transform.SetParent(diceSlot);
        _dice.transform.localPosition = Vector3.zero;
    }

    public void ClearSlot()
    {
        // 주사위 존으로 되돌린다.
        if (dice)
        {
            dice.SetIsOnSlot(false);
            bcm.SetDiceZone(dice);
        }

        // 주사위 정보 초기화
        dice = null;

        // 타겟관련 버튼들 비활성화
        MarkerButton.SetActive(false);
        ActionButton.SetActive(false);
        RangePannel.SetActive(false);
        
    }

    public void OpenSkillInfo()
    {
        Debug.Log("스킬창 호출");
        FindObjectOfType<SkillInfoView>().SetSkillInfo();
    }

    public BattleUnit GetBattleUnit()
    {
        return battleUnit;
    }

    public Command GetCurrentCommand()
    {
        try
        {
            int diceVal = dice.GetDiceValue();
            battleUnit.commands[diceVal / 2].SetDiceValue(diceVal); 
            return battleUnit.commands[diceVal / 2];
        }
        catch
        {
            Debug.Log("GetCurrentCommand Get Anomally");
            return null;
        }
        
        
    }

    #region Targetting Pannel Functions
    public void EnableMarker()
    {
        ActionButton.SetActive(false);
        MarkerButton.SetActive(true);
    }

    public void MarkerFunc()
    {
        // 다른쪽 선택 눌려있는 경우 처리. 전부 비활성화
        bcm.DisableTargetInfoAll();
        
        // 눌린 인덱스 기반 범위 표시 활성화
        // tm으로부터 현재 스킬의 정보를 읽는다.
        Command cmd = tm.GetCurrentCmd();

        // tm에 대상 등록. 슬롯번호
        tm.SetTargetIndex(slotIndex);

        // 인덱싱 정보 읽어 bcm에게 범위패널 활성화를 요청한다
        bcm.EnableRangePannel(cmd);
        
        // 결정 버튼 활성화
        ActionButton.SetActive(true);
        
    }

    // 실행 버튼 이벤트
    public void ActionFunc()
    {
        // 모든 타겟팅 인터페이스 비활성화
        bcm.DisableTargetInfoAll();

        // 전투 시작 요청
        bcm.BattleEvent();
    }

    public void DisableTargetInfo()
    {
        // 마커 버튼 비활성화
        MarkerButton.SetActive(false);

        // 결정 버튼 비활성화
        ActionButton.SetActive(false);

        // 범위 표시 패널 비활성화
        RangePannel.SetActive(false);
    }

    public void EnableRangePannel()
    {
        RangePannel.SetActive(true);
    }
    #endregion

    // BCM측에서 생성 시점에 슬롯 번호 설정
    public void SetSlotIndex(int _idx)
    {
        slotIndex = _idx;
    }
    public int GetSlotIndex()
    {
        return slotIndex;
    }


    #region Battle Events
    public void PlaySkill()
    {
        // 전투 애니메이션 재생
        animator.SetTrigger("AttackTrigger"); // 애셋 애니메이터에 맞춰 수정하도록
        isAnimating = true;
        // 애니메이션에서 CommandEffect()
        // 를 실행시켜 모든 데미지 처리가 되도록 한다

        // 주사위 삭제
        Destroy(dice.gameObject);
    }

    // 이 부분이 애니메이션에서 실행되어야함. 아니면 딜레이를 줘야할 것
    public void CommandEffect()
    {
        //Debug.Log("스킬 발동 : " + tm.GetCurrentCmd().GetName());
        //tm.GetCurrentCmd().Execute();
        GetCurrentCommand().Execute();

        // bcm에게 슬롯 재정렬 요청
        bcm.SortSlotIndex();

        // 슬롯 정렬이 끝났을때 false되도록 변경해야함
        isAnimating = false;
    }

    // 데미지를 입을때.
    public void TakeDamage(int _dmg)
    {
        // 피격 애니 재생
        Debug.Log("피격 발생");

        battleUnit.curHp -= _dmg;

        // UI에 적용. float로 수동 형변환 필요
        hpBar.value = (float)battleUnit.curHp / battleUnit.maxHp;
        
        if(battleUnit.curHp < 1)
        {
            // 슬롯에서 삭제
            // 리스트에서 지우고
            bcm.RemoveSlot(transform.parent.name, slotIndex);

            
            // 오브젝트 삭제
            Destroy(this.gameObject);
        }
    }

    public void TurnEnd()
    {
        for(int i=0; i< buffs.Count; i++)
        {
            buffs[i].EndTurn(this);
        }
    }

    public void TakeTrueDmg(int _dmg)
    {
        Debug.Log("방어력 무시 피해");

        battleUnit.curHp -= _dmg;

        // UI에 적용. float로 수동 형변환 필요
        hpBar.value = (float)battleUnit.curHp / battleUnit.maxHp;

        if (battleUnit.curHp < 1)
        {
            // 슬롯에서 삭제
            // 리스트에서 지우고
            bcm.RemoveSlot(transform.parent.name, slotIndex);


            // 오브젝트 삭제. 애니메이션 처리로 위임
            Destroy(this.gameObject);
        }
    }

    public void AddBuff(Buff _buff)
    {
        // 추가
        buffs.Add(_buff);
    }

    public void RemoveBuff(Buff _buff)
    {
        buffs.Remove(_buff);
    }
    #endregion

    // 적 행동 마침 여부 체크에 사용중
    public bool IsAnimating()
    {
        return isAnimating;
    }

    
    
}
