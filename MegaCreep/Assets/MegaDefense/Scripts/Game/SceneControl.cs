using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneControl{

    public const int maxLevel = 3;
    public static bool showLevel = false;
    public static void LoadMenu()
    {
        Load(SceneName.menu);
    }
    public static void LoadLevel(int _lv)
    {
        PlayerPrefsLevel.levelData.CurrentLevel = _lv;
        string _idTxt = _lv < 10 ? "0" + _lv : "" + _lv;
        Load(SceneName.level + _idTxt);
    }
    public static void LoadNextLevel()
    {
        int _lv = PlayerPrefsLevel.levelData.CurrentLevel += 1;
        if (_lv < 15)
        {
            PlayerPrefsLevel.levelData.CurrentLevel = _lv;
            string _idTxt = _lv < 10 ? "0" + _lv : "" + _lv;
            Load(SceneName.level + _idTxt);
        }
        else
        {
            Load(SceneName.menu);
        }
    }
    public static void ResetLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public static void Load(string _name)
    {
        SceneManager.LoadScene(_name);
    }
}
public class SceneName
{
    public const string loading = "Loading";
    public const string menu = "Menu";
    public const string level = "";

}