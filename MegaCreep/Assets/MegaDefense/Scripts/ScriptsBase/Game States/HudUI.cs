using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudUI : MonoBehaviour {

    static HudUI _instance;
    public static HudUI Instance { get { return _instance; } }
    public Text txtCash;
    public Text txtGS;
    Action onBack;
    protected void Awake()
    {
        _instance = this;
    }
    public void OnShow(string _txtGS, Action _onBack)
    {
        txtGS.text = _txtGS;
        onBack = _onBack;
        gameObject.SetActive(true);
        UpdateData();
    }
    public void OnHide()
    {
        onBack = null;
        gameObject.SetActive(false);
    }
    public void UpdateData()
    {
        txtCash.text = "" + GamePreferences.userInfo.coin;
    }
    public void OnClickBack()
    {
        if (onBack!=null)
        {
            onBack();
        }
    }
}