using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SkillInfoView : MonoBehaviour
{
    [SerializeField] Text skillName;
    [SerializeField] Text skillDice;
    [SerializeField] Text skillDesc;
    [SerializeField] GameObject CanvasObj;
    

    public void SetSkillInfo()
    {
        CanvasObj.SetActive(true);

        Command _comm = FindObjectOfType<TargettingManager>().GetCurrentCmd();

        if (_comm == null)
        {
            skillName.text = "None";
            skillDice.text = "None";
            skillDesc.text = "설정된 스킬이 없습니다.";
        }
        else
        {
            skillName.text = _comm.GetName();
            skillDesc.text = _comm.GetDesc();
            skillDice.text = _comm.GetDiceValue().ToString();
        }
    }

    public void CloseSkillInfo()
    {
        CanvasObj.SetActive(false);
    }
}
