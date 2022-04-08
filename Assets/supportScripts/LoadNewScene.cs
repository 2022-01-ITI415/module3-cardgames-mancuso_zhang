using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadNewScene : MonoBehaviour
{
    public void LoadScene0()
    {
        SceneManager.LoadScene("__Main_Scene_0");
    }
    public void LoadScene1()
    {
        SceneManager.LoadScene("_Prospector_Scene_1");
    }
    public void LoadScene2()
    {
        SceneManager.LoadScene("Poker_Scene_2");
    }
}
