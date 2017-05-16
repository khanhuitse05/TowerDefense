using UnityEngine;
public class GSHome : GSTemplate
{
    static GSHome _instance;
    public static GSHome Instance { get { return _instance; } }
    protected override void Awake()
    {
        base.Awake();
        _instance = this;
    }
    protected override void init()
    {
    }
    public override void onEnter()
    {
        base.onEnter();
        HudUI.Instance.OnHide();
    }
    public override void onResume()
    {
        base.onResume();
    }
    public override void onSuspend()
    {
        base.onSuspend();
    }
    public override void onExit()
    {
        base.onExit();
    }
    public override void onBackKey()
    {
    }
    public void OnClickStart()
    {
        GameStatesManager.Instance.stateMachine.SwitchState(GSSelectLevel.Instance);
    }
    public void OnClickSetting()
    {
        UISettingMenu.Show();
    }
}