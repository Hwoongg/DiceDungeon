using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DiceWindow : MonoBehaviour
{
    [SerializeField] Text dice1;
    [SerializeField] Text dice2;
    [SerializeField] Text dice3;




    

    private void OnEnable()
    {
        RollDice();

        StartCoroutine(WaitAndDisable());
    }



    void RollDice()
    {
        // 난수 3개 생성.
        int rand1 = Random.Range(1, 7);
        int rand2 = Random.Range(1, 7);
        int rand3 = Random.Range(1, 7);

        dice1.text = rand1.ToString();
        dice2.text = rand2.ToString();
        dice3.text = rand3.ToString();
    }

    IEnumerator WaitAndDisable()
    {
        yield return new WaitForSeconds(3.0f);

        gameObject.SetActive(false);
    }
}
