using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
// Dungeon Scene Second Prototype
//

public class DungeonSceneManager2 : MonoBehaviour
{
    // Camera Anchors
    [SerializeField] GameObject anchorGroup;

    bool isEventing;
    
    void Start()
    {
        FindObjectOfType<DungeonCamera>().SetTarget(
            anchorGroup.transform.GetChild(0).gameObject);

        isEventing = false;
    }


    void Update()
    {
        // /////////////////////////////
        //
        // Click Event
        // 다른 클래스로 내보내는 것이 깔끔할 듯
        //
        // /////////////////////////////

        // 마우스 좌클릭시 레이 쏴서 타일 검출
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, 1000);

            // 클릭 위치 체크용 가시화
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);

            for (int i = 0; i < hits.Length; i++)
            {

                IClickInteraction t = hits[i].transform.GetComponent<IClickInteraction>();

                if (t != null)
                {
                    t.Interaction();
                }
            }

        }
    }
}
