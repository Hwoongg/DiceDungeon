using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonSceneManager : MonoBehaviour
{
    TileSpawner tileSpawner;

    [SerializeField] GameObject PlayerPrefab;

    private void Start()
    {
        tileSpawner = FindObjectOfType<TileSpawner>();

        // 플레이어 배치
        TileIndex startIdx = tileSpawner.GetStartIndex();
        float tileWidth = tileSpawner.GetTileWidth();
        float tileHeight = tileSpawner.GetTileHeight();

        Vector3 startPos = new Vector3();
        startPos.x = startIdx.y * tileWidth;
        startPos.y = startIdx.x * tileHeight;
        GameObject o = Instantiate(PlayerPrefab, startPos, Quaternion.identity);

        FindObjectOfType<DungeonCamera>().SetTarget(o);
    }
}
