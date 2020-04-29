using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct TileIndex
{
    public int x;
    public int y;

    public TileIndex(int _x, int _y)
    {
        x = _x;
        y = _y;
    }

    public bool IsCompare(int _x, int _y)
    {
        if(x==_x && y==_y)
        {
            return true;
        }

        return false;
    }
}

public class TileSpawner : MonoBehaviour
{
    // 전체 맵 크기. 가로 세로
    [SerializeField] int raw;
    [SerializeField] int column;

    // 타일 오브젝트 프리팹
    [SerializeField] GameObject tilePrefab;

    // 던전 타일들 저장용. 2차원배열
    DungeonTile[,] dungeonTiles;

    // 빈칸과 출발지 인덱스
    [SerializeField] TileIndex[] wallIndices;
    [SerializeField] TileIndex startIndex;

    // 타일 한장의 크기 (배치용)
    [SerializeField] float tileWidth;
    [SerializeField] float tileHeight;

    private void Start()
    {
        CreateTileMap();
    }

    public void CreateTileMap()
    {
        dungeonTiles = new DungeonTile[raw, column];

        Vector3 pos = new Vector3();

        // 타일 생성
        for(int y=0; y<raw; y++)
        {
            for(int x=0; x<column; x++)
            {
                pos.x = x * tileWidth;
                pos.y = y * tileHeight;
                GameObject o = Instantiate(tilePrefab, pos, Quaternion.identity);
                dungeonTiles[y, x] = o.GetComponent<DungeonTile>();
                dungeonTiles[y, x].SetTileIndex(x, y);

                // 타일 속성 부여
                int randNum = Random.Range(1, 4);
                dungeonTiles[y, x].SetEventState((DungeonTile.EventState)randNum);

                if (startIndex.IsCompare(x, y))
                {
                    // 시작 좌표라면 시작타일로
                    dungeonTiles[y, x].SetEventState(DungeonTile.EventState.START);
                    continue;
                }
                
                for (int k=0; k<wallIndices.Length; k++)
                {
                    if(wallIndices[k].IsCompare(x, y))
                    {
                        // 벽좌표라면 벽으로 설정
                        dungeonTiles[y, x].SetEventState(DungeonTile.EventState.WALL);
                        break;
                    }
                }

            }
        }
    }

    // 특정 인덱스의 타일 정보 조회
    public DungeonTile.EventState GetTileState(int x, int y)
    {
        return dungeonTiles[y, x].GetEventState();
    }
    public DungeonTile.EventState GetTileState(TileIndex _idx)
    {
        return dungeonTiles[_idx.y, _idx.x].GetEventState();
    }


    public TileIndex GetStartIndex()
    {
        return startIndex;
    }

    public float GetTileWidth()
    {
        return tileWidth;
    }
    public float GetTileHeight()
    {
        return tileHeight;
    }
    
    // 
    public void DisableTiles()
    {

    }
}
