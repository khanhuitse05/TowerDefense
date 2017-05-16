using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using TDTK;

public class GSPerkMenu : GSTemplate
{
    public GameObject perkButtonObj;
    [HideInInspector] public List<PerkItemUI> itemList = new List<PerkItemUI>();
    public GameObject pfItemObj;
    public RectTransform scrollViewContent;
    public Transform selectHighlightT;

    PerkItemUI perkSelected;

    public Text lbLevel;
    public Text lbCash;
    public Text lbName;
    public Text lbDesp;
    public Text lbReq;
    public Text lbCurrent;
    public Text lbCurrentDetail;
    public Text lbNext;
    public Text lbNextDetail;
    public Text lbCost;
    public GameObject butPurchaseObj;

    static GSPerkMenu _instance;
    public static GSPerkMenu Instance { get { return _instance; } }
    protected override void Awake()
    {
        base.Awake();
        _instance = this;
    }
    protected override void init()
    {
        // init perkList
        itemList = new List<PerkItemUI>();
        for (int i = 0; i < GamePreferences.perkList.Count; i++)
        {
            GameObject _obj = Utils.Spawn(pfItemObj, scrollViewContent);
            PerkItemUI _item = _obj.GetComponent<PerkItemUI>();
            itemList.Add(_item);
            _item.Init(GamePreferences.perkList[i]);
            _item.SetToNormal();

        }
        //select the first item
        StartCoroutine(SelectFirst());
    }
    IEnumerator SelectFirst()
    {
        yield return new WaitForEndOfFrame();
        OnItemButton(itemList[0].gameObject);
    }
    public override void onEnter()
    {
        base.onEnter();
        UpdatePerkItem();
        lbLevel.text = GamePreferences.userInfo.level.ToString();
        perkButtonObj.SetActive(false);
    }
    public override void onResume()
    {
        base.onResume();
        HudUI.Instance.OnShow("Upgrage", onBackKey);
    }
    public override void onSuspend()
    {
        base.onSuspend();
    }
    public override void onExit()
    {
        base.onExit();
        perkButtonObj.SetActive(true);
    }
    public override void onBackKey()
    {
        GameStatesManager.Instance.stateMachine.PopState(GSSelectLevel.Instance);
    }
    
    public void OnItemButton(GameObject butObj)
    {

        PerkItemUI _item = butObj.GetComponent<PerkItemUI>();
        if (_item == perkSelected)
        {
            return;
        }
        ClearSelection();
        perkSelected = _item;
        SetToSelected(_item);
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        lbCash.text = "Cash: " + GamePreferences.userInfo.coin.ToString();
        lbLevel.text = "Level: " + GamePreferences.userInfo.level.ToString();
        if (perkSelected == null)
        {
            lbName.text = "";
            lbDesp.text = "";
            lbReq.text = "";
            butPurchaseObj.SetActive(false);
            lbCurrent.gameObject.SetActive(false);
            return;
        }

        Perk perk = perkSelected.perk;
        lbName.text = perk.name;
        lbDesp.text = perk.desp;
        if (perk.IsAvailable() == false)
        {
            lbReq.text = "Perk unlock at level " + perk.levelUnlock;
            butPurchaseObj.SetActive(false);
            lbCurrent.gameObject.SetActive(false);
        }
        else
        {
            lbCurrent.gameObject.SetActive(true);
            lbReq.text = "";
            lbCurrent.text = "Level " + perk.level;
            lbCurrentDetail.text = perk.perkLevel[perk.level].desp;
            if (perk.IsMaxLevel())
            {
                lbNext.gameObject.SetActive(false);
                butPurchaseObj.SetActive(false);
            }
            else
            {
                lbNext.gameObject.SetActive(true);
                butPurchaseObj.SetActive(true);
                lbNext.text = "Level " + (perk.level + 1);
                lbNextDetail.text = perk.perkLevel[perk.level + 1].desp;
                lbCost.text = perk.GetCost().ToString(); ;
            }
        }
    }

    void ClearSelection()
    {
        if (perkSelected == null) return;
        SetToNormal(perkSelected);
        perkSelected = null;
    }

    public void OnUpgrageButton()
    {
        if (perkSelected == null)
        {
            UpdateDisplay();
            return;
        }
        if (perkSelected.perk.IsMaxLevel())
        {
            UpdateDisplay();
            return;
        }
        Perk perk = perkSelected.perk;
        int _cost = perk.GetCost();
        if (_cost <= GamePreferences.userInfo.coin)
        {
            GamePreferences.userInfo.UpgrageCoin(0 - _cost);
            GamePreferences.UpdateLevelPerk(perk.ID, perk.level + 1);
            perkSelected.UpgragePerk();
            HudUI.Instance.UpdateData();
            UpdateDisplay();
        }
    }
    
    void UpdatePerkItem()
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            SetToNormal(itemList[i]);
        }
        if (perkSelected != null)
        {
            SetToSelected(perkSelected);
        }
    }

    public void SetToSelected(PerkItemUI perkItem)
    {
        perkItem.SetToSelected();
        selectHighlightT.position = perkItem.transform.position;
    }
    public void SetToNormal(PerkItemUI perkItem)
    {
        perkItem.SetToNormal();
    }
}