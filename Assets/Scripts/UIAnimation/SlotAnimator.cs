using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotAnimator : MonoBehaviour
{
    BattleSceneManager bcm; // 슬롯 정보 획득, 앵커 위치 획득

    // 슬롯 위치 앵커들
    Transform[] heroAnchors;
    Transform[] EnemyAnchors;


    private void Start()
    {
        bcm = FindObjectOfType<BattleSceneManager>();

        Transform AnchorHost = bcm.GetZoneObject().transform.Find("AnchorGroup");

        heroAnchors = new Transform[AnchorHost.childCount];
        for(int i=0; i<AnchorHost.childCount; i++)
        {
            heroAnchors[i] = AnchorHost.GetChild(i);
        }

        AnchorHost = bcm.GetZoneObject(false).transform.Find("AnchorGroup");
        EnemyAnchors = new Transform[AnchorHost.childCount];
        for (int i = 0; i < AnchorHost.childCount; i++)
        {
            EnemyAnchors[i] = AnchorHost.GetChild(i);
        }

        ReplaceSlotsAll(0.5f);
    }

    // 모든 슬롯 위치 갱신 이동 애니메이션 실행
    public void ReplaceSlotsAll(float _speed = 1.0f)
    {
        // 현재 돌아가고 있는 모든 애니 코루틴 중단
        StopAllCoroutines();

        // 각 슬롯들 제 위치로 이동 애니메이션
        BattleSlot[] heros = bcm.GetHeroSlots();
        for(int i=0; i<heros.Length; i++)
        {
            StartCoroutine(
                UIAnimator.Move(heros[i].gameObject, 
                heros[i].transform.localPosition, 
                heroAnchors[heros[i].GetSlotIndex()].localPosition, _speed));
        }

        BattleSlot[] enemies = bcm.GetEnemySlots();
        for (int i = 0; i < enemies.Length; i++)
        {
            StartCoroutine(
                UIAnimator.Move(enemies[i].gameObject,
                enemies[i].transform.localPosition,
                EnemyAnchors[enemies[i].GetSlotIndex()].localPosition, _speed));
        }
    }
    
}
