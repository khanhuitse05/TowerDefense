using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	public class UITowerInfo : MonoBehaviour {
		

		public UnitTower currentTower;
		
		public static UITowerInfo instance;
		private GameObject thisObj;
	
		public Transform anchorLeft;
		public Transform anchorRight;
		public Transform frameT;
		
		public Text txtName;
		public Text txtDesp1;
		public Text txtDesp2;
		
		public GameObject rscObj;
        public Text txtRsc;
		
		public GameObject floatingButtons;
		public GameObject butUpgrade;
		public Text txtUpgrade;
        public GameObject butSell;
        public Text txtSell;

        public GameObject butUpgradeAlt1;
        public Text txtUpgradeAlt1;

        //a number recording the upgrade option available for the tower, update everytime tower.ReadyToBeUpgrade() is called
        //0 - not upgradable
        //1 - can be upgraded
        //2 - 2 upgrade path, shows 2 upgrade buttons
        private int upgradeOption=1;	
		
		void Start(){
			instance=this;
			thisObj=gameObject;
			Hide();
		}
		
		void OnEnable(){
			GameControl.onGameOverE += OnGameOver;
			UnitTower.onConstructionCompleteE += OnConstructionComplete;
			
			Unit.onDestroyedE += OnUnitDestroyed;
		}
		void OnDisable(){
			GameControl.onGameOverE += OnGameOver;
			UnitTower.onConstructionCompleteE -= OnConstructionComplete;
			
			Unit.onDestroyedE -= OnUnitDestroyed;
		}
		
		void OnGameOver(int _star){ Hide(); }
		
		void OnTowerUpgraded(UnitTower tower){
			Show(tower, true);
		}
		
		void OnConstructionComplete(UnitTower tower){
			if(tower!=currentTower) return;
			
			upgradeOption=currentTower.ReadyToBeUpgrade();
            butSell.SetActive(true);
            txtSell.text = "" + currentTower.GetValueSell();
            if (upgradeOption>0){
				butUpgrade.SetActive(true);
                txtUpgrade.text = "" + currentTower.GetCost();
                if (upgradeOption > 1)
                {
                    butUpgradeAlt1.SetActive(true);
                    txtUpgradeAlt1.text = "" + currentTower.GetCost(1);
                }
                else butUpgradeAlt1.SetActive(false);
			}
			else{
				butUpgrade.SetActive(false);
				butUpgradeAlt1.SetActive(false);
			}
		}
		
		void OnUnitDestroyed(Unit unit){
			if(currentTower==null) return;
			if(!unit.IsTower() || unit.GetUnitTower()!=currentTower) return;
			Hide();
		}
		
		// Update is called once per frame
		void Update () {
			if(!isOn) return;
			
			Vector3 screenPos = Camera.main.WorldToScreenPoint(currentTower.thisT.position);
			floatingButtons.transform.localPosition=screenPos/UI.GetScaleFactor();
			
			//force the frame to go left when the tower is off screen (specifically for dragNdrop button hover)
			if(currentTower.IsSampleTower()){
				if(screenPos.x<0 || screenPos.x>Screen.width || screenPos.y<0 || screenPos.y>Screen.height){
					screenPos.x=Screen.width;
					currentX=0;
				}
			}
			
			if(currentX<Screen.width/2 && screenPos.x>Screen.width/2)
                _SetScreenPos(screenPos);
			else if(currentX>Screen.width/2 && screenPos.x<Screen.width/2)
                _SetScreenPos(screenPos);
			
		}
		
		
		
		
		public void OnUpgradeButton(){ UpgradeTower(); }
		public void OnUpgradeButtonAlt(){ UpgradeTower(1); }
		public void UpgradeTower(int upgradePath=0){
			bool exception=currentTower.Upgrade(upgradePath);
			if(exception==true){
				upgradeOption=0;
				floatingButtons.SetActive(false);
			}
			else UIGameMessage.DisplayMessage("Insufficient Resource");
		}
		
		public void OnSellButton(){
			currentTower.Sell();
			UI.ClearSelectedTower();
		}

		private float currentX=0;
		public static void SetScreenPos(Vector3 pos){ instance._SetScreenPos(pos); }
		public void _SetScreenPos(Vector3 pos){
			if(pos.x<Screen.width/2){
				frameT.SetParent(anchorRight);
				frameT.localPosition=new Vector3(-160, 0, 0);
			}
			else{
				frameT.SetParent(anchorLeft);
				frameT.localPosition=new Vector3(160, 0, 0);
			}
			
			currentX=pos.x;
		}
		
		
		IEnumerator WaitForConstruction(){
			while(currentTower!=null && currentTower.IsInConstruction()) yield return null;
			if(currentTower!=null){
				Update();
				floatingButtons.SetActive(true);
			}
		}

        public static bool isOn=true;
		public static void Show(UnitTower tower, bool showControl=false){ instance._Show(tower, showControl); }
		public void _Show(UnitTower tower, bool showControl=false){
			isOn=true;
			thisObj.SetActive(showControl);
			rscObj.SetActive(!showControl);
            currentTower =tower;

            if (showControl)
            {
                if (currentTower.IsInConstruction())
                {
                    StartCoroutine(WaitForConstruction());
                    floatingButtons.SetActive(false);
                    butSell.SetActive(false);
                    butUpgradeAlt1.SetActive(false);
                    butUpgrade.SetActive(false);
                }
                else
                {
                    floatingButtons.SetActive(true);
                    butSell.SetActive(true);
                    txtSell.text = "" + currentTower.GetValueSell();
                    upgradeOption = currentTower.ReadyToBeUpgrade();
                    if (upgradeOption > 0)
                    {
                        butUpgrade.SetActive(true);
                        txtUpgrade.text = "" + currentTower.GetCost();
                        if (upgradeOption > 1)
                        {
                            butUpgradeAlt1.SetActive(true);
                            txtUpgradeAlt1.text = "" + currentTower.GetCost(1);
                        }
                        else
                        {
                            butUpgradeAlt1.SetActive(false);
                        }
                    }
                    else
                    {
                        butUpgrade.SetActive(false);
                        butUpgradeAlt1.SetActive(false);
                    }
                }
            }
            else
            {
                int cost = currentTower.GetCost();
                txtRsc.text = cost.ToString();
            }
			
			Update();
			
			txtName.text=tower.unitName;
			txtDesp1.text=tower.GetDespStats();
			txtDesp2.text=tower.GetDespGeneral();
			
			thisObj.SetActive(isOn);
		}
		public static void Hide(){ instance._Hide(); }
		public void _Hide(){
			//currentTower=null;
			isOn=false;
			thisObj.SetActive(isOn);
		}
		
	}

}