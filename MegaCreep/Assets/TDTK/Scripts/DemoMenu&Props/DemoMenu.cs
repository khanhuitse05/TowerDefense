using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TDTK;
using UnityEngine.SceneManagement;

namespace TDTK
{
    [System.Serializable]
    public class DemoMenuItem
    {
        public string displayedName;
        public string levelName;
    }
	public class DemoMenu : MonoBehaviour {
		
		public RectTransform frame;
		
		public List<DemoMenuItem> level =new List<DemoMenuItem>();
		public List<UnityButton> buttonList=new List<UnityButton>();

        // Use this for initialization
        void Start()
        {
            for (int i = 0; i < level.Count; i++)
            {
                if (i == 0) buttonList[0].Init();
                else if (i > 0)
                {
                    buttonList.Add(buttonList[0].Clone("ButtonStart" + (i + 1), new Vector3(0, -i * 40, 0)));
                }
                buttonList[i].label.text = level[i].displayedName;

                frame.sizeDelta = new Vector2(200, 30 + level.Count * 40);
            }
        }
		public void OnStartButton(GameObject butObj){
			for(int i=0; i<buttonList.Count; i++){
				if(buttonList[i].rootObj==butObj){
					SceneManager.LoadScene(level[i].levelName);
				}
			}
		}
		
	}

}