using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonPlayer : MonoBehaviour
{
    GameObject camAnchor;
    bool isAnimating;

    TileIndex nowIndex;

    DungeonSceneManager dcm;

    private void Awake()
    {
        camAnchor = transform.Find("CamAnchor").gameObject;
    }

    private void Start()
    {
        dcm = FindObjectOfType<DungeonSceneManager>();
    }

    public GameObject GetAnchor()
    {
        return camAnchor;
    }

    
    // 특정 좌표로 이동
    public void Move(Vector3 _movePos)
    {
        StopAllCoroutines();

        StartCoroutine(MoveRoutine(_movePos));
    }

    public void SetAnimating(bool _b)
    {
        isAnimating = _b;
    }

    public void SetIndex(TileIndex _idx)
    {
        nowIndex = _idx;
    }

    public IEnumerator MoveRoutine(Vector3 _movePos)
    {
        Debug.Log("이동 시작");

        yield return StartCoroutine(UIAnimator.Move(
            gameObject, transform.position, _movePos, 2.0f));

        // 뒷타일들 벽상태로 만든다
        // ...

        // 타일 인덱스의 이벤트 실행
        Debug.Log("타일 도착");
        DungeonTile.EventState eventState= dcm.GetSpawner().GetTileState(nowIndex);

        Debug.Log("이벤트 감지");
        switch (eventState)
        {
            case DungeonTile.EventState.BATTLE:
                Debug.Log("전투 이벤트");
                break;
            case DungeonTile.EventState.GATHERING:
                Debug.Log("채집 이벤트");
                break;
            case DungeonTile.EventState.TRAP:
                Debug.Log("함정 이벤트");
                break;
        }

        yield break;

    }
}
