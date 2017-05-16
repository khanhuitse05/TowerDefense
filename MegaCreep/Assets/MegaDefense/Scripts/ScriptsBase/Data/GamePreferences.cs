using UnityEngine;
using System;
using System.Collections.Generic;
using TDTK;

public class GamePreferences
{
    public static void Load()
    {
        LoadSetting();
        LoadUserInfo();
        LoadGameInfo();
        LoadPerkList();
    }
    /// <summary>
    /// Setting
    /// </summary>
    public static Setting setting { get; set; }
    static Setting LoadSetting()
    {
        setting = SaveGameManager.loadData<Setting>(GameTags.settingDataKey);
        if (setting == null)
        {
            setting = new Setting();
            SaveSetting();
        }
        return setting;
    }
    public static void SaveSetting()
    {
        SaveGameManager.saveData<Setting>(GameTags.settingDataKey, setting);
    }
    /// <summary>
    /// UserInfo
    /// </summary>
    public static UserInfo userInfo { get; set; }
    static UserInfo LoadUserInfo()
    {
        userInfo = SaveGameManager.loadData<UserInfo>(GameTags.userInfoDataKey);
        if (userInfo == null)
        {
            userInfo = new UserInfo();
            SaveUserInfo();
        }
        return userInfo;
    }
    public static void SaveUserInfo()
    {
        SaveGameManager.saveData<UserInfo>(GameTags.userInfoDataKey, userInfo);
    }
    /// <summary>
    /// gameInfo
    /// </summary>
    public static List<Perk> perkList;
    static void LoadPerkList()
    {
        perkList = new List<Perk>();
        List<Perk> dbList = PerkDB.Load();
        for (int i = 0; i < dbList.Count; i++)
        {
            if (dbList[i].disablePerk == false)
            {
                Perk perk = dbList[i].Clone();
                PerkSave level = gameInfo.GetLevel(perk.ID);
                perk.level = level.level;
                perkList.Add(perk);
            }
        }
    }
    public static void UpdateLevelPerk(int id, int level)
    {
        for (int i = 0; i < perkList.Count; i++)
        {
            if (perkList[i].ID == id)
            {
                perkList[i].level = level;
                break;
            }
        }
        for (int i = 0; i < gameInfo.perkLevel.Count; i++)
        {
            if (gameInfo.perkLevel[i].id == id)
            {
                gameInfo.perkLevel[i].level = level;
                break;
            }
        }
        SaveGameInfo();
    }
    public static GameInfo gameInfo { get; set; }
    static GameInfo LoadGameInfo()
    {
        gameInfo = SaveGameManager.loadData<GameInfo>(GameTags.gameInfoDataKey);
        if (gameInfo == null)
        {
            gameInfo = new GameInfo();
            SaveGameInfo();
        }
        return gameInfo;
    }
    public static void SaveGameInfo()
    {
        SaveGameManager.saveData<GameInfo>(GameTags.gameInfoDataKey, gameInfo);
    }
}

public class Setting
{
    public string version;
    public float soundVolume;
    public bool enableTutorial;
    public int rate;

    public Setting()
    {
        version = GameConstants.gameVersion;
        soundVolume = 1.0f;
        enableTutorial = true;
        rate = 0;
    }
}
public class UserInfo
{
    public string name;
    public int level;
    public int coin;

    public UserInfo()
    {
        name = "Ping";
        level = 1;
        coin = 0;
    }
    public void UpgrageCoin(int _value)
    {
        coin += _value;
        GamePreferences.SaveUserInfo();
    }
}
public class GameInfo
{
    public List<PerkSave> perkLevel;
    public GameInfo()
    {
        perkLevel = new List<PerkSave>();
    }
    public PerkSave GetLevel(int id)
    {
        for (int i = 0; i < perkLevel.Count; i++)
        {
            if (perkLevel[i].id == id)
            {
                return perkLevel[i];
            }
        }
        PerkSave _perk = new PerkSave();
        _perk.level = 0;
        _perk.id = id;
        perkLevel.Add(_perk);
        GamePreferences.SaveGameInfo();
        return _perk;
    }
}
public class PerkSave
{
    public int id;
    public int level;

    public PerkSave()
    {
        id = 0;
        level = 0;
    }
}