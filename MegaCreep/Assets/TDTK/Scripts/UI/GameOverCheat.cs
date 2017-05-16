using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TDTK;

public class GameOverCheat : MonoBehaviour {

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F4) && UIGameOverMenu.isOn == false)
        {
            UIGameOverMenu.Show(0);
        }
        if (Input.GetKeyDown(KeyCode.F5) && UIGameOverMenu.isOn == false)
        {
            UIGameOverMenu.Show(1);
        }
        if (Input.GetKeyDown(KeyCode.F6) && UIGameOverMenu.isOn == false)
        {
            UIGameOverMenu.Show(2);
        }
        if (Input.GetKeyDown(KeyCode.F7) && UIGameOverMenu.isOn == false)
        {
            UIGameOverMenu.Show(3);
        }
#endif
    }
}
