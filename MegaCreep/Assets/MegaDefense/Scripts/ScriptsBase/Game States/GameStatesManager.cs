using UnityEngine;
using System;
using System.Collections;

public class GameStatesManager : MonoBehaviour
{
    static GameStatesManager _instance;
    public static GameStatesManager Instance { get { return _instance; } }
    public static bool enableBackKey = true;
    public GameObject InputProcessor { get; set; }
    public static Action onBackKey { get; set; }
    public StateMachine stateMachine;
    void Awake()
    {
        _instance = this;
        Time.timeScale = 1;
    }
    void Start()
    {
        if (SceneControl.showLevel == false)
        {
            SceneControl.showLevel = true;
            stateMachine.PushState(GSHome.Instance);
        }
        else
        {
            stateMachine.PushState(GSSelectLevel.Instance);
        }
    }
    void Update()
    {
        if (onBackKey != null && Input.GetKeyDown(KeyCode.Escape))
        {
            if (enableBackKey)
            {
                onBackKey();
            }
        }
    }

    public IEnumerator checkInternetConnection(Action<bool> action)
    {
        WWW www = new WWW("http://google.com");
        yield return www;
        if (www.error != null)
        {
            action(false);
        }
        else {
            action(true);
        }
    }

}