using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
// 던전 타일 클래스.
//

public class DungeonTile : MonoBehaviour
{
    // 자신의 상태
    // 보이는 상태인가 아닌가
    public enum ViewState
    {
        BRIGHT, // 보임, 이동가능
        SHADOW, // 봤었던 타일, 이동불가
        DARK // 아직 못본 타일, 이동불가
    }
    ViewState viewState;

    // 타일의 종류
    // 전투, 채집, 휴식 등
    public enum EventState
    {
        START,
        BATTLE, // 전투
        GATHERING, // 채집
        TRAP = 3,
        UNKNOWN = 3,
        WALL
            
    }
    EventState eventState;

    // 타일 이미지들
    [SerializeField] Sprite[] tileSprites;
    SpriteRenderer childRenderer; // 타일 속성 표시용은 자식에 붙일것임

    TileIndex tileIndex;

    private void Start()
    {
        
        childRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        SetSprite(eventState);
    }

    private void Initialize(/*ViewState _vs,*/ EventState _es)
    {
        //viewState = _vs;
        eventState = _es;
    }

    private void SetSprite(EventState _es)
    {
        // es를 sprite index로 변환, 렌더러에 세트
        childRenderer.sprite = tileSprites[(int)_es];
    }

    // 타일 상태 액세스 함수 제작
    public EventState GetEventState()
    {
        return eventState;
    }

    public void SetEventState(EventState _es)
    {
        eventState = _es;
    }

    public void MoveToThisTile()
    {
        DungeonPlayer dp = FindObjectOfType<DungeonSceneManager>().GetPlayer();
        dp.SetIndex(tileIndex);
        dp.Move(transform.position);
    }

    public void SetTileIndex(int _x, int _y)
    {
        TileIndex idx = new TileIndex(_x, _y);
        tileIndex = idx;
    }
    public void SetTileIndex(TileIndex _idx)
    {
        tileIndex = _idx;
    }

}
