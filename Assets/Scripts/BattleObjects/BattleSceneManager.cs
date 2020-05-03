using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleSceneManager : MonoBehaviour
{
    // 전투중인 유닛들의 정보. 필드 시스템 생길때 뺄 것.
    List<BattleUnit> heros;
    List<BattleUnit> enemies; 

    List<GameObject> heroSlots;
    List<GameObject> enemySlots;


    // 생성용 프리팹
    [SerializeField] GameObject SlotPrefab;
    [SerializeField] GameObject DicePrefab;

    // 슬롯 생성 좌표용. Transform으로 변경해도 무관
    [SerializeField] GameObject PlayerZone; // 플레이어측
    [SerializeField] GameObject EnemyZone; // 적측
    [SerializeField] GameObject DiceZone; // 주사위 슬롯
    [SerializeField] GameObject DragZone; // 드래그 중인 오브젝트 정렬용



    // 한 턴에 설정한 명령들이 보관되는 리스트.
    // 포인터 리스트로 변경해야 하는가? 
    // clear 시행 시 GC에서 회수가 되는지 체크 필요.
    //List<Command> commands; 
    Command curCommand;


    // 현재 전투 시행중인가?
    bool isBattle;


    EventDice dragDice; // 드래그 중인 주사위 정보


    private void Awake()
    {
        // 임시 플레이어 데이터 생성
        heros = new List<BattleUnit>();
        for (int i = 0; i < 3; i++)
        {
            heros.Add(new Guardian());
        }

        // 임시 적 데이터 생성
        enemies = new List<BattleUnit>();
        for (int i = 0; i < 4; i++)
        {
            enemies.Add(new Enemy());
        }



        // 슬롯 오브젝트 배열
        //heroSlots = new GameObject[heros.Count];
        heroSlots = new List<GameObject>();
        //enemySlots = new GameObject[enemies.Count];
        enemySlots = new List<GameObject>();

        isBattle = false;


        CreateDice(heros.Count);

    }

    void Start()
    {
        Vector3 spawnPos;

        // 슬롯 생성
        spawnPos = PlayerZone.transform.position;
        spawnPos.x -= 50;
        //spawnPos.y -= 50;
        for (int i = 0; i < heros.Count; i++)
        {
            // 아군측
            heroSlots.Add(Instantiate(SlotPrefab, spawnPos, Quaternion.identity, PlayerZone.transform));
            BattleSlot slot = heroSlots[i].GetComponent<BattleSlot>();
            slot.Init(heros[i]);
            slot.SetSlotIndex(i);
        }

        spawnPos = EnemyZone.transform.position;
        spawnPos.x += 50;
        //spawnPos.y -= 50;

        for (int i = 0; i < enemies.Count; i++)
        {
            // 적군측
            enemySlots.Add(Instantiate(SlotPrefab, spawnPos, Quaternion.identity, EnemyZone.transform));
            BattleSlot slot = enemySlots[i].GetComponent<BattleSlot>();
            slot.Init(enemies[i]);
            slot.SetSlotIndex(i);
        }

    }

    // 입력된 수만큼 주사위 생성.
    // Enemy측에서 장착하기 위해 주사위 배열 반환기능 추가함.
    public EventDice[] CreateDice(int amount)
    {
        EventDice[] dices = new EventDice[amount];

        // 아군 갯수만큼 주사위 생성
        for (int i = 0; i < amount; i++)
        {
            GameObject o = Instantiate(DicePrefab, DiceZone.transform);
            dices[i] = o.GetComponent<EventDice>();
            dices[i].SetNumber();
        }

        return dices;
    }

    public void SetDragDice(EventDice _d)
    {
        dragDice = _d;
    }

    public EventDice GetDragDice()
    {
        if (dragDice == null)
            return null;

        return dragDice;
    }

    public void SetDiceZone(EventDice _d)
    {
        _d.transform.SetParent(DiceZone.transform);
    }

    public void SetDragZone(GameObject _o)
    {
        _o.transform.SetParent(DragZone.transform);
    }

    public BattleSlot[] GetHeroSlots()
    {
        BattleSlot[] res = new BattleSlot[heroSlots.Count];

        for (int i = 0; i < res.Length; i++)
        {
            res[i] = heroSlots[i].GetComponent<BattleSlot>();
        }

        return res;
    }

    public BattleSlot[] GetEnemySlots()
    {
        BattleSlot[] res = new BattleSlot[enemySlots.Count];

        for (int i = 0; i < res.Length; i++)
        {
            res[i] = enemySlots[i].GetComponent<BattleSlot>();
        }

        return res;
    }

    public BattleSlot[] GetAllSlots()
    {
        int length = enemySlots.Count + heroSlots.Count;
        BattleSlot[] res = new BattleSlot[length];

        int i = 0;

        for (int j = 0; i < heroSlots.Count; i++, j++)
        {
            res[i] = heroSlots[i].GetComponent<BattleSlot>();
        }

        for (int j = 0; j < enemySlots.Count; i++, j++)
        {
            res[i] = enemySlots[j].GetComponent<BattleSlot>();
        }

        return res;
    }

    public void ResetDiceSlot()
    {
        BattleSlot[] slots = GetAllSlots();

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].ClearSlot();
        }
    }

    public void DisableTargetInfoAll()
    {
        // 전체 결정버튼+범위패널 비활성화
        for (int i = 0; i < heroSlots.Count; i++)
        {
            heroSlots[i].GetComponent<BattleSlot>().DisableTargetInfo();
        }

        for (int i = 0; i < enemySlots.Count; i++)
        {
            enemySlots[i].GetComponent<BattleSlot>().DisableTargetInfo();
        }
    }

    // 명령의 타겟 정보를 읽어 범위 패널 활성화
    public void EnableRangePannel(Command _cmd)
    {
        int target = _cmd.GetTargetIndex();
        int[] ranges = _cmd.GetRangeIndex();
        Command.Target t = _cmd.GetTargetType();
        curCommand = _cmd;

        BattleSlot[] slots;
        _cmd.ClearTargetList();

        switch (t)
        {
            case Command.Target.ENEMY:
                slots = GetEnemySlots();
                for (int i = 0; i < ranges.Length; i++)
                {
                    // 슬롯 인덱스 추출
                    int idx = target + ranges[i];

                    // 유효성 판단
                    if (idx > slots.Length || idx < 0)
                        continue;

                    slots[target].EnableRangePannel();

                    // 대상도 그냥 등록해버릴까? 생각보다 범위 판단하는게 귀찮다
                    // 스킬마다 이거 검사하는것도 일일듯
                    _cmd.AddTarget(slots[target].gameObject);

                }
                break;

            case Command.Target.HERO:
                slots = GetHeroSlots();
                for (int i = 0; i < ranges.Length; i++)
                {
                    // 슬롯 인덱스 추출
                    int idx = target + ranges[i];

                    // 유효성 판단
                    if (idx > slots.Length || idx < 0)
                        continue;

                    slots[target].EnableRangePannel();
                    _cmd.AddTarget(slots[target].gameObject);
                }
                break;

            case Command.Target.GLOBAL:
                slots = GetAllSlots();
                for (int i = 0; i < ranges.Length; i++)
                {
                    // 슬롯 인덱스 추출
                    int idx = target + ranges[i];

                    // 유효성 판단
                    if (idx > slots.Length || idx < 0)
                        continue;

                    slots[target].EnableRangePannel();
                    _cmd.AddTarget(slots[target].gameObject);
                }
                break;
        }

    }

    public void BattleEvent()
    {
        // 아직 전투 실행중인지 체크
        if (isBattle)
        {
            return;
        }

        // 시전자측 전투 애니메이션 재생. 애니메이션 측에서 모든 처리가 될것임
        curCommand.GetCaster().GetComponent<BattleSlot>().PlaySkill();

        
    }

    // 슬롯들의 자신 위치 다시 매기기
    public void ResetSlotIndex()
    {
        for (int i = 0; i < heroSlots.Count; i++)
        {
            heroSlots[i].GetComponent<BattleSlot>().SetSlotIndex(i);
        }

        for (int i = 0; i < enemySlots.Count; i++)
        {
            enemySlots[i].GetComponent<BattleSlot>().SetSlotIndex(i);
        }
    }

    // 슬롯에서 삭제
    public void RemoveSlot(string _zoneName, int _index)
    {
        if (_zoneName == "PlayerZone")
        {
            heroSlots.RemoveAt(_index);
        }

        if (_zoneName == "EnemyZone")
        {
            enemySlots.RemoveAt(_index);
        }

        SortSlotIndex();

    }


    public void SortSlotIndex()
    {
        // 오름차순 정렬. 람다 빼는게 나을수도?
        heroSlots.Sort((GameObject A, GameObject B) =>
        {
            if (A.GetComponent<BattleSlot>().GetBattleUnit().aggro <
                B.GetComponent<BattleSlot>().GetBattleUnit().aggro)
                return 1;
            else if (A.GetComponent<BattleSlot>().GetBattleUnit().aggro >
                B.GetComponent<BattleSlot>().GetBattleUnit().aggro)
                return -1;
            return 0;
        });

        enemySlots.Sort((GameObject A, GameObject B) =>
        {
            if (A.GetComponent<BattleSlot>().GetBattleUnit().aggro <
                B.GetComponent<BattleSlot>().GetBattleUnit().aggro)
                return 1;
            else if (A.GetComponent<BattleSlot>().GetBattleUnit().aggro >
                B.GetComponent<BattleSlot>().GetBattleUnit().aggro)
                return -1;
            return 0;
        });

        

        // 인덱스 다시 매기기
        ResetSlotIndex();

        // 바뀐 인덱스에 맞춰 재정렬 애니메이션 실행
        FindObjectOfType<SlotAnimator>().ReplaceSlotsAll(3.0f);
    }

    public void RollEvent()
    {
        CreateDice(heros.Count);

        EnemyAI enemyAI = FindObjectOfType<EnemyAI>();

        // AI 클래스 세팅
        enemyAI.SetEnemySlot();

        // 몬스터 측 주사위 입력
        enemyAI.SetEnemyDice();

        // 몬스터 턴 시작
        enemyAI.StartEnemyTurn();
        
    }

    public List<GameObject> GetSlotObjects(bool isHero = true)
    {
        if (isHero)
        {
            return heroSlots;
        }
        else
        {
            return enemySlots;
        }
    }

    public GameObject GetZoneObject(bool isHero = true)
    {
        if (isHero)
        {
            return PlayerZone;
        }
        else
        {
            return EnemyZone;
        }
    }

    public void EscEvent()
    {
        SceneManager.LoadScene("Dungeon2");
    }
}
