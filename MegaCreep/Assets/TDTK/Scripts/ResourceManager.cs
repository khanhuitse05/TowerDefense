using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {
	
	
	public class ResourceManager : MonoBehaviour {
		
		public delegate void RscChangedHandler(int changedValue);
		public static event RscChangedHandler onRscChangedE;
		
		public bool enableRscGen=false;
        public float rscGenRate = 0;
        public int rsc = 0;
		public static ResourceManager instance;
		
		void Awake(){
			if(instance!=null) instance=this;
		}
		
		public void Init(){	//to match the rsc with the DB
			instance=this;
			if(enableRscGen) StartCoroutine(RscGenRoutine());
		}
		
		void OnEnable(){
			GameControl.onGameOverE += OnGameOver;
		}
		void OnDisable(){
			GameControl.onGameOverE -= OnGameOver;
		}
		
		void OnGameOver(int _star){
		}
		IEnumerator RscGenRoutine(){
            while (true) {
                yield return new WaitForSeconds(1);

                float perkRegenRate = PerkManager.GetRscRegen();
                int valueList = 0;
                bool increased = false;
                float temp = rscGenRate + perkRegenRate;
                if (temp >= 1) {
                    while (temp >= 1) {
                        valueList += 1;
                        temp -= 1;
                    }
                    increased = true;
                }
                if (increased) GainResource(valueList);
            }
		}
		public static int GetResource(){
			return instance.rsc;
		}
		
		public static bool HasSufficientResource(int rscL){ return instance._HasSufficientResource(rscL); }
		public bool _HasSufficientResource(int rscL)
        {
            if (rsc<rscL) return false;
            return true;
		}
		
		public static void SpendResource(int rscL){ instance._GainResource(rscL, 0, false, -1); }
		public static void GainResource(int rscL, float mulL=0, bool useMul=true){ instance._GainResource(rscL, mulL, useMul); }
        public void _GainResource(int rscL, float mulL = 0, bool useMul = true, float sign = 1f)
        {
            //if this is gain, apply perks multiplier
            if (sign == 1 && useMul)
            {
                float multiplierL = PerkManager.GetRscGain();
                multiplierL += mulL;
                rscL = (int)((float)(rscL * (1f + multiplierL)));
            }

            rsc = (int)Mathf.Max(0, rsc + rscL * sign);

            if (onRscChangedE != null) onRscChangedE(rscL);
        }
		//not in use at the moment
		public static void NewSceneNotification(){
			//resourcesA=resourceManager.resources;
		}
		public static void ResetCummulatedResource(){
			//for(int i=0; i<resourcesA.Length; i++){
			//	resourcesA[i].value=0;
			//}
		}
	}
}
