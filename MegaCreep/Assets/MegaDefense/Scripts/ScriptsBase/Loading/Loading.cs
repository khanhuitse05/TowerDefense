using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Loading : MonoBehaviour {

    public Text txt;
	void Start () {
        LoadSceneFromOther.isLoad = true;
        GamePreferences.Load();
        PlayerPrefsLevel.LoadLevelData();
        Invoke("Finish", 2);
    }
    private void Update()
    {
        txt.text += ". ";
    }
    void Finish () {
        SceneManager.LoadScene(1);
	}
}
