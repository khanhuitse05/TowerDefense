﻿using UnityEngine;
using System.Collections;

public class SettingButton : MonoBehaviour {

    public GameObject audioOn;
    public GameObject audioOff;
    void OnEnable()
    {
        Utils.setActive(audioOn, GamePreferences.setting.soundVolume > 0);
        Utils.setActive(audioOff, GamePreferences.setting.soundVolume == 0);
    }
    public void onBtnSettingsClick()
    {
        float value = 0;
        if (GamePreferences.setting.soundVolume != 0)
        {
            value = 0f;
            Utils.setActive(audioOn, false);
            Utils.setActive(audioOff, true);
        }
        else
        {
            value = 1.0f;
            Utils.setActive(audioOn, true);
            Utils.setActive(audioOff, false);
        }
        GamePreferences.setting.soundVolume = value;
        AudioManager.SetSFXVolume(value);
        GamePreferences.SaveSetting();
    }
}
