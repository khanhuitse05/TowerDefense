using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {
	
	public class UI : MonoBehaviour {
		
		public float scaleFactor=1;
		public static float GetScaleFactor(){ return instance.scaleFactor; }
		
		public enum _BuildMode{PointNBuild, DragNDrop};
		public _BuildMode buildMode=_BuildMode.PointNBuild;
		public static bool UseDragNDrop(){ return instance.buildMode==_BuildMode.PointNBuild ? false : true; }
		
		private UnitTower selectedTower;
		
		public float fastForwardTimeScale=1.5f;
		public static float GetFFTime(){ return instance.fastForwardTimeScale; }
		
		
		public bool disableTextOverlay=false;
		public static bool DisableTextOverlay(){ return instance.disableTextOverlay; }

		public static UI instance;
		void Awake(){
			instance=this;
            scaleFactor = Screen.height/810f;
        }
		
		// Use this for initialization
		void Start () {
			
		}
		
		void OnEnable(){
			GameControl.onGameOverE += OnGameOver;
			
			Unit.onDestroyedE += OnUnitDestroyed;
			
			AbilityManager.onTargetSelectModeE += OnAbilityTargetSelectMode;
			
			UnitTower.onUpgradedE += SelectTower;	//called when tower is upgraded, require for upgrade which the current towerObj is destroyed so select UI can be cleared properly 
		}
		void OnDisable(){
			GameControl.onGameOverE -= OnGameOver;
			
			Unit.onDestroyedE -= OnUnitDestroyed;
			
			AbilityManager.onTargetSelectModeE -= OnAbilityTargetSelectMode;
			
			UnitTower.onUpgradedE -= SelectTower;
		}
		
		void OnGameOver(int _star){ StartCoroutine(_OnGameOver(_star)); }
		IEnumerator _OnGameOver(int _star){
			UIBuildButton.Hide();
			
			yield return new WaitForSeconds(1.0f);
			UIGameOverMenu.Show(_star);
		}
		
		void OnUnitDestroyed(Unit unit){
			if(!unit.IsTower()) return;
			
			if(selectedTower==unit.GetUnitTower()) ClearSelectedTower();
		}
		
		private bool abilityTargetSelecting=false;
		void OnAbilityTargetSelectMode(bool flag){ StartCoroutine(_OnAbilityTargetSelectMode(flag)); }
		IEnumerator _OnAbilityTargetSelectMode(bool flag){ 
			yield return null;
			abilityTargetSelecting=flag;
		}
		
		
		// Update is called once per frame
		void Update () {
			if(GameControl.GetGameState()==_GameState.Over) return;
			
			if(abilityTargetSelecting) return;
			
			#if (UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8) && (!UNITY_EDITOR)
				if(Input.touchCount==1){
					Touch touch=Input.touches[0];
					
					if(UIUtilities.IsCursorOnUI(touch.fingerId)) return;
					
					if(!UseDragNDrop() && !UIBuildButton.isOn) BuildManager.SetIndicator(touch.position);
					
					if(touch.phase==TouchPhase.Began) OnTouchCursorDown(touch.position);
				}
				else UpdateMouse();
			#else
				UpdateMouse();
			#endif
		}
		
		void UpdateMouse(){
			if(UIUtilities.IsCursorOnUI()) return;
				
			if(!UseDragNDrop() && !UIBuildButton.isOn) BuildManager.SetIndicator(Input.mousePosition);
			
			if(Input.GetMouseButtonDown(0)) OnTouchCursorDown(Input.mousePosition);
		}
		
		void OnTouchCursorDown(Vector3 cursorPos){
			UnitTower tower=GameControl.Select(cursorPos);
					
			if(tower!=null){
				SelectTower(tower);
				UIBuildButton.Hide();
			}
			else{
				if(selectedTower!=null){
					ClearSelectedTower();
					return;
				}
				
				if(!UseDragNDrop()){
					if(BuildManager.CheckBuildPoint(cursorPos)==_TileStatus.Available){
						UIBuildButton.Show();
					}
					else{
						UIBuildButton.Hide();
					}
				}
			}
		}
		
		
		
		void SelectTower(UnitTower tower){
			selectedTower=tower;
			
			Vector3 screenPos=Camera.main.WorldToScreenPoint(selectedTower.thisT.position);
			UITowerInfo.SetScreenPos(screenPos);
			
			UITowerInfo.Show(selectedTower, true);
		}
		public static void ClearSelectedTower(){
			if(instance.selectedTower==null) return;
			instance.selectedTower=null;
			UITowerInfo.Hide();
			GameControl.ClearSelectedTower();
		}
		
		public static UnitTower GetSelectedTower(){ return instance.selectedTower; }
		
	}

}