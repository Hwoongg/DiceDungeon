using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.EventSystems;


//
// 던전 내 각종 이벤트에 사용될 주사위 오브젝트
// 주 상호작용 인터페이스가 될 것
//
// BattleDice라는 형태로 파생시키고 추상클래스로 전환할지 고려 필요함
//

public class EventDice : MonoBehaviour
{

    int diceValue;

    [SerializeField] Sprite[] DiceImages;
    Image image;

    Vector3 defPos;

    GraphicRaycaster graphicRaycaster;
    BattleSceneManager bcm;

    bool isOnBattleSlot;

    // Instantiate된 즉시 일어남. Start는 안일어난다.
    private void Awake()
    {
        defPos = transform.position;
        image = GetComponent<Image>();
        isOnBattleSlot = false;
    }

    private void Start()
    {
        graphicRaycaster = FindObjectOfType<GraphicRaycaster>();
        bcm = FindObjectOfType<BattleSceneManager>();
    }

    public void SetNumber()
    {
        int randNum = Random.Range(0, 6);
        diceValue = randNum;

        if (image)
            image.sprite = DiceImages[randNum];

        
    }

    public void BeginDragEvent()
    {
        //defPos = transform.position;

        // 전투슬롯 위에 있던 주사위일 경우 있던 슬롯 비우기
        if (isOnBattleSlot)
        {
            // 전투슬롯 컴포넌트는 부모의 부모에 있음 주의
            transform.parent.parent.GetComponent<BattleSlot>().ClearSlot();
        }

        bcm.SetDragZone(this.gameObject);
        bcm.SetDragDice(this);


    }

    public void DragEvent()
    {
        Debug.Log("주사위 드래그 이벤트");
        Vector3 newPos = Input.mousePosition;
        //newPos.z = 0;
        transform.localPosition = newPos;
        
    }

    

    public void DropEvent()
    {
        Debug.Log("주사위 드랍");


        // 주사위를 쥐고있지 않을 경우 스킵한다
        if (bcm.GetDragDice() != null)
        {
            bcm.SetDragDice(null);

            // 드랍된 위치 체크. 전투슬롯의 위치인가?
            List<RaycastResult> results = new List<RaycastResult>();
            PointerEventData ped = new PointerEventData(null);
            ped.position = Input.mousePosition;

            graphicRaycaster.Raycast(ped, results);
            
            for(int i= 0; i<results.Count; i++)
            {
                BattleSlot bs = results[i].gameObject.GetComponent<BattleSlot>();
                if(bs)
                {
                    // 적쪽에 못올라가도록
                    if(bs.transform.parent.name == "EnemyZone")
                    {
                        break;
                    }

                    bcm.ResetDiceSlot();
                    
                    // 전투 슬롯에 주사위 설정
                    bs.SetDice(this);

                    isOnBattleSlot = true;

                    
                    // 타겟팅 시스템 설정
                    TargettingManager tm = FindObjectOfType<TargettingManager>();
                    if(tm)
                    {
                        bs.GetCurrentCommand().SetCaster(bs.gameObject);
                        tm.CreateTargetWindow(bs.GetCurrentCommand());
                    }
                    


                    return;
                }
            }

            
        }

        //transform.position = defPos;
        bcm.ResetDiceSlot();
        bcm.SetDiceZone(this); // 주사위 존으로 되돌린다
        bcm.SetDragDice(null);
        isOnBattleSlot = false;
    }

    public void SetIsOnSlot(bool _b)
    {
        isOnBattleSlot = _b;
    }

    public int GetDiceValue()
    {
        return diceValue;
    }
}
