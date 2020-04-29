using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonSceneManager : MonoBehaviour
{
    TileSpawner tileSpawner;

    [SerializeField] GameObject PlayerPrefab;

    GameObject playerObj;
    DungeonPlayer dunPlayer;

    private void Start()
    {
        tileSpawner = FindObjectOfType<TileSpawner>();

        // 플레이어 배치
        TileIndex startIdx = tileSpawner.GetStartIndex();
        float tileWidth = tileSpawner.GetTileWidth();
        float tileHeight = tileSpawner.GetTileHeight();

        // 플레이어 배치 좌표 계산
        Vector3 startPos = new Vector3();
        startPos.x = startIdx.x * tileWidth;
        startPos.y = startIdx.y * tileHeight;

        playerObj = Instantiate(PlayerPrefab, startPos, Quaternion.identity);
        dunPlayer = playerObj.GetComponent<DungeonPlayer>();
        dunPlayer.SetIndex(startIdx);

        // 카메라에 추적대상 설정
        FindObjectOfType<DungeonCamera>().SetTarget(dunPlayer.GetAnchor());
    }

    private void Update()
    {
        // 마우스 좌클릭시 레이 쏴서 타일 검출
        if(Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, 1000);

            // 클릭 위치 체크용 가시화
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);

            for (int i = 0; i < hits.Length; i++)
            {
                DungeonTile t = hits[i].transform.GetComponent<DungeonTile>();
                if (t)
                {
                    t.MoveToThisTile();
                }
            }
            
        }
    }
    
    public DungeonPlayer GetPlayer()
    {
        return dunPlayer;
    }

    public TileSpawner GetSpawner()
    {
        return tileSpawner;
    }
}
