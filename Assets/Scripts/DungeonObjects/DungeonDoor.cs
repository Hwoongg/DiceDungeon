using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonDoor : MonoBehaviour, IClickInteraction
{
    enum EventType
    {
        BATTLE,
        GATHER,
        SPECIAL
    }

    EventType eventType;

    private void Start()
    {
        int randNum = Random.Range(0, 3);

        eventType = (EventType)randNum;

    }
    public void Interaction()
    {
        FindObjectOfType<DungeonCamera>().SetTarget(
            transform.GetChild(0).gameObject);

        StartCoroutine(WaitAndEvent());
    }

    IEnumerator WaitAndEvent()
    {

        yield return new WaitForSeconds(1.0f);
        switch (eventType)
        {
            case EventType.BATTLE:
                Debug.Log("전투 이벤트 발생");
                SceneManager.LoadScene("BattleScene");
                break;

            case EventType.GATHER:
                Debug.Log("채집 이벤트 발생");
                SceneManager.LoadScene("GatherScene");
                break;

            case EventType.SPECIAL:
                Debug.Log("특수 이벤트 발생");
                SceneManager.LoadScene("SpecialScene");
                break;

        }

        yield break;
    }


}
