using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct TileIndex
{
    public int x;
    public int y;

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
        for(int i=0; i<raw; i++)
        {
            for(int j=0; j<column; j++)
            {
                pos.x = j * tileWidth;
                pos.y = i * tileHeight;
                GameObject o = Instantiate(tilePrefab, pos, Quaternion.identity);
                dungeonTiles[i, j] = o.GetComponent<DungeonTile>();

                // 타일 속성 부여
                int randNum = Random.Range(1, 4);
                dungeonTiles[i, j].SetEventState((DungeonTile.EventState)randNum);

                if (startIndex.IsCompare(i, j))
                {
                    // 시작 좌표라면 시작타일로
                    dungeonTiles[i, j].SetEventState(DungeonTile.EventState.START);
                    continue;
                }
                
                for (int k=0; k<wallIndices.Length; k++)
                {
                    if(wallIndices[k].IsCompare(i, j))
                    {
                        // 벽좌표라면 벽으로 설정
                        dungeonTiles[i, j].SetEventState(DungeonTile.EventState.WALL);
                        break;
                    }
                }

            }
        }
    }

    // 특정 인덱스의 타일 정보 조회
    public void CheckIndex(int x, int y)
    {
        dungeonTiles[x, y].GetEventState();
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
}
