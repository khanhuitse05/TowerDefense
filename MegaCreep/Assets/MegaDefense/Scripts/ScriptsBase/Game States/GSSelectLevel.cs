using UnityEngine;
public class GSSelectLevel : GSTemplate
{
    static GSSelectLevel _instance;
    public static GSSelectLevel Instance { get { return _instance; } }

    public LevelSelectionLogic levelLogic;

    protected override void Awake()
    {
        base.Awake();
        _instance = this;
    }
    protected override void init()
    {
        levelLogic.init();
    }
    public override void onEnter()
    {
        base.onEnter();
        levelLogic.onEnter();
    }
    public override void onResume()
    {
        base.onResume();
        HudUI.Instance.OnShow("Select Level", onBackKey);
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
        GameStatesManager.Instance.stateMachine.SwitchState(GSHome.Instance);
    }

    public void OnClickPerk()
    {
        GameStatesManager.Instance.stateMachine.PushState(GSPerkMenu.Instance);
    }
    
}