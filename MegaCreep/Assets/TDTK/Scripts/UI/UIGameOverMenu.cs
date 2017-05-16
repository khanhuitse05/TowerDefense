using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using TDTK;
using UnityEngine.SceneManagement;

namespace TDTK
{
    public class UIGameOverMenu : MonoBehaviour
    {
        private static UIGameOverMenu instance;
        public GameObject guiDefeat;
        public GameObject guiVictory;
        public GameObject[] starObj;

        void Awake()
        {
            instance = this;
        }
        private void Start()
        {
            Hide();
        }
        
        public void OnContinueButton()
        {
            SceneControl.LoadNextLevel();
        }

        public void OnRestartButton()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void OnMainMenuButton()
        {
            SceneControl.LoadMenu();
        }

        public static bool isOn = true;
        public static void Show(int _star) { instance._Show(_star); }
        public void _Show(int _star)
        {
            Time.timeScale = 1;
            isOn = true;
            gameObject.SetActive(isOn);
            if (_star <= 0)
            {
                guiDefeat.SetActive(true);
            }
            else
            {
                guiVictory.SetActive(true);
                PlayerPrefsLevel.levelData.FinishLevel(_star);
                StartCoroutine(ShowStartRoutine(_star));
            }
        }
        public static void Hide() { instance._Hide(); }
        public void _Hide()
        {
            isOn = false;
            guiDefeat.SetActive(false);
            guiVictory.SetActive(false);
            gameObject.SetActive(false);
        }
        IEnumerator ShowStartRoutine(int _star)
        {
            yield return new WaitForSeconds(0.6f);
            int _index = 0;
            while (_index < _star)
            {
                starObj[_index].SetActive(true);
                starObj[_index].transform.localScale = Vector3.zero;
                iTween.ScaleTo(starObj[_index], Vector3.one, 0.2f);
                _index++;
                yield return new WaitForSeconds(0.2f);
            }
        }

    }
}