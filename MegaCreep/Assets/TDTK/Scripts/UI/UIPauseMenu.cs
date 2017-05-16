using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	public class UIPauseMenu : MonoBehaviour {

		private GameObject thisObj;
		private static UIPauseMenu instance;
		
		public Slider sliderMusicVolume;
		public Slider sliderSFXVolume;
		void Awake(){
			instance=this;
			thisObj=gameObject;
			transform.localPosition=Vector3.zero;
			sliderMusicVolume.value=AudioManager.GetMusicVolume()*100;
			sliderSFXVolume.value=AudioManager.GetSFXVolume()*100;
		}
		
		void Start () {
			Hide();
		}

        public void OnResumeButton(){
			Hide();
			GameControl.ResumeGame();
		}
		
		public void OnResetButton()
        {
            Hide();
            GameControl.ResetGame();
        }
		
		public void OnMainMenuButton(){
            SceneControl.LoadMenu();
		}
		
		public static bool isOn=true;
		public static void Show(){ instance._Show(); }
		public void _Show(){
			isOn=true;
			thisObj.SetActive(isOn);
		}
		public static void Hide(){ instance._Hide(); }
		public void _Hide(){
			isOn=false;
			thisObj.SetActive(isOn);
		}
		
		public void OnMusicVolumeSlider(){
			if(Time.timeSinceLevelLoad>0.5f)
				AudioManager.SetMusicVolume(sliderMusicVolume.value/100);
		}
		public void OnSFXVolumeSlider(){
			if(Time.timeSinceLevelLoad>0.5f)
				AudioManager.SetSFXVolume(sliderSFXVolume.value/100);
		}
	}
}