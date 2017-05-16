using UnityEngine;

public class CheatManager : MonoBehaviour {
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    void Update () {
#if !LIVE
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F1))
        {
            OnCheat();
        }
#else
        if (Input.touches.Length == 5)
        {
            OnCheat();
        }
#endif
#endif
    }
    public void OnCheat()
    {
        PopupManager.Instance.InitMessage("OnCheat");
    }
}
