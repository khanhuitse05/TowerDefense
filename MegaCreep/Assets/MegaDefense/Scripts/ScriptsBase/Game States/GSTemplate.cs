using System.Collections;
using UnityEngine;

////////////////////////////////////////////////////////
//Author:
//TODO: a game state sample
////////////////////////////////////////////////////////

public class GSTemplate : IState
{
    public GameObject guiMain;
    bool isFirst;
    protected override void Awake()
    {
        base.Awake();
        guiMain.SetActive(false);
        isFirst = true;
    }
    /// <summary>
    /// One time when start
    /// </summary>
    protected virtual void init()
    {
    }
    public virtual void onBackKey()
    {
    }
    public override void onSuspend()
    {
        base.onSuspend();
        GameStatesManager.onBackKey = null;
    }
    public override void onResume()
    {
        base.onResume();
        GameStatesManager.Instance.InputProcessor = guiMain;
        GameStatesManager.onBackKey = onBackKey;
    }
    public override void onEnter()
    {
        base.onEnter();
        if (isFirst)
        {
            isFirst = false;
            init();
        }
        onResume();
        guiMain.SetActive(true);
    }
    public override void onExit()
    {
        base.onExit();
        onSuspend();
        guiMain.SetActive(false);
    }
}
