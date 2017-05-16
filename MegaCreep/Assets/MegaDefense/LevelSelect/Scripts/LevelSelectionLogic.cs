using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LevelSelectionLogic : MonoBehaviour
{

    static LevelSelectionLogic _instance;
    public static LevelSelectionLogic Instance { get { return _instance; } }

    public List<LevelList> levelList = new List<LevelList>(); //List of level class;
    public static LevelList currentLevel;
    public Animator animPlay;

    void Awake()
    {
        _instance = this;
    }
    public void init()
    {
        if (levelList.Count != PlayerPrefsLevel.maxLevel)
        {
            Utils.LogWarning("levelList.Count != MAX_LEVEL");
        }
        for (int i = 0; i < levelList.Count; i++)
        {
            levelList[i].SetInfo(PlayerPrefsLevel.levelData.GetLevelInfo(i));
        }
        currentLevel = null;
    }
    public void onEnter()
    {
        if (currentLevel != null)
        {
            currentLevel.Highlight(false);
            currentLevel = null;
        }
    }
    public void OnClickLevel(LevelList _item)
    {
        if (_item.info.isUnlock)
        {
            if (currentLevel == null)
            {
                _item.Highlight(true);
                animPlay.SetTrigger("isShow");
                currentLevel = _item;
            }
            else
            {
                currentLevel.Highlight(false);
                _item.Highlight(true);
                currentLevel = _item;
            }
        }
    }
    public void OnClickPlay()
    {
        SceneControl.LoadLevel(currentLevel.info.LevelIndex);
    }
}


