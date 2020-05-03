using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TempSceneManager : MonoBehaviour
{
    public void EscEvent()
    {
        SceneManager.LoadScene("Dungeon2");
    }
}
