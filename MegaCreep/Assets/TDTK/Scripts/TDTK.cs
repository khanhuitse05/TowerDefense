using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	public class TDTK : MonoBehaviour {
		
		
		public delegate void NewWaveHandler(int waveID);
		public static event NewWaveHandler onNewWaveE;				//inidcate a new wave has start spawning
		
		public delegate void WaveSpawnedHandler(int waveID);
		public static event WaveSpawnedHandler onWaveSpawnedE;	//inidcate a wave has finished spawning
		
		public delegate void WaveClearedHandler(int waveID);
		public static event WaveClearedHandler onWaveClearedE;		//indicate a wave has been cleared
		
		public delegate void EnableSpawnHandler();
		public static event EnableSpawnHandler onEnableSpawnE;	//call to indicate SpawnManager is ready to spawn next wave (when no longer in the process of actively spawning a wave)
		
		public delegate void SpawnTimerHandler(float time);
		public static event SpawnTimerHandler onSpawnTimerE;		//call to indicate how long until next spawn
		
		
		
		public delegate void LifeHandler(int value);
		public static event LifeHandler onLifeE;								//call when player's life has changed
		
		public delegate void GameOverHandler(int _star);
		public static event GameOverHandler onGameOverE;			//call when game is over
		
		
		
		public delegate void ResourceHandler(int changedValueList);
		public static event ResourceHandler onResourceE;				//call when the values on resource list are changed
		
		
		
		//onAddNewTowerE
		public delegate void NewTowerHandler(UnitTower tower);
		public static event NewTowerHandler onNewTowerE;
		
		
		
		public delegate void CreepDestroyedHandler(UnitCreep creep);
		public static event CreepDestroyedHandler onCreepDestroyedE;		//indicate the creep has been destroyed
		
		public delegate void CreepDestinationHandler(UnitCreep creep);
		public static event CreepDestinationHandler onCreepDestinationE;	//indicate the creep has reach its destination
	
		public delegate void TowerConstructionStartHandler(UnitTower tower);
		public static event TowerConstructionStartHandler onTowerConstructingE;	//called when the tower start constructing 
		
		public delegate void TowerConstructionCompleteHandler(UnitTower tower);
		public static event TowerConstructionCompleteHandler onTowerConstructedE; //called when the tower finished constructing 
		
		public delegate void TowerUpgradedHandler(UnitTower tower);
		public static event TowerUpgradedHandler onTowerUpgradedE;			//called when tower has been upgraded
		
		public delegate void TowerSoldHandler(UnitTower tower);
		public static event TowerSoldHandler onTowerSoldE;						//called when tower has been sold
		
		public delegate void TowerDestroyedHandler(UnitTower tower);
		public static event TowerDestroyedHandler onTowerDestroyedE;		//called when tower has been destroyed


		
		public delegate void NewAbilityHandler(Ability ability);
		public static event NewAbilityHandler onNewAbilityE;					//called when new ability is added
		
		public delegate void AbilityActivatedHandler(Ability ability);
		public static event AbilityActivatedHandler onAbilityActivatedE;		//called when an ability is activated
		
		public delegate void AbilityTargetSelectedHandler(bool flag);
		public static event AbilityTargetSelectedHandler onAbilityTargetSelectedE;	//call when target select for ability is switch on/off (true/false)
		
		public static TDTK instance;
		
		public static void Init(){
			if(instance!=null) return;
			GameObject obj=new GameObject();
			obj.name="_TDTK";
			obj.AddComponent<TDTK>();
		}
		
		
		void Awake(){
			if(instance!=null){
				Destroy(gameObject);
				return;
			}
			
			instance=this;
		}
		
		
		void OnEnable(){
			SpawnManager.onNewWaveE += OnNewWave;
			SpawnManager.onWaveSpawnedE += OnWaveSpawned;
			SpawnManager.onWaveClearedE += OnWaveCleared;
			SpawnManager.onEnableSpawnE += OnEnableSpawn;
			SpawnManager.onSpawnTimerE += OnSpawnTimer;
			
			GameControl.onLifeE += OnLife;
			GameControl.onGameOverE += OnGameOver;
			
			ResourceManager.onRscChangedE += OnResource;
			
			Unit.onDestroyedE += OnUnitDestroyed;
			UnitCreep.onDestinationE += OnCreepDestination;
			
			UnitTower.onSoldE += OnTowerSold;
			UnitTower.onUpgradedE += OnTowerUpgraded;
			UnitTower.onConstructionStartE += OnTowerConstructing;
			UnitTower.onConstructionCompleteE += OnTowerConstructed;
			
			AbilityManager.onAbilityActivatedE += OnAbilityActivated;
			AbilityManager.onTargetSelectModeE += OnAbilityTargetSelectModeE;
		}
		void OnDisable(){
			SpawnManager.onNewWaveE -= OnNewWave;
			SpawnManager.onWaveSpawnedE -= OnWaveSpawned;
			SpawnManager.onWaveClearedE -= OnWaveCleared;
			SpawnManager.onEnableSpawnE -= OnEnableSpawn;
			SpawnManager.onSpawnTimerE -= OnSpawnTimer;
			
			GameControl.onLifeE -= OnLife;
			GameControl.onGameOverE -= OnGameOver;
			
			ResourceManager.onRscChangedE -= OnResource;
			
			Unit.onDestroyedE -= OnUnitDestroyed;
			UnitCreep.onDestinationE -= OnCreepDestination;
			
			UnitTower.onSoldE -= OnTowerSold;
			UnitTower.onUpgradedE -= OnTowerUpgraded;
			UnitTower.onConstructionStartE -= OnTowerConstructing;
			UnitTower.onConstructionCompleteE -= OnTowerConstructed;
			
			AbilityManager.onAbilityActivatedE -= OnAbilityActivated;
			AbilityManager.onTargetSelectModeE -= OnAbilityTargetSelectModeE;
		}
		
		
		
		public static void OnNewWave(int waveID){
			if(onNewWaveE!=null) onNewWaveE(waveID);
		}
		public static void OnWaveSpawned(int waveID){
			if(onWaveSpawnedE!=null) onWaveSpawnedE(waveID);
		}
		public static void OnWaveCleared(int waveID){
			if(onWaveClearedE!=null) onWaveClearedE(waveID);
		}
		public static void OnEnableSpawn(){
			if(onEnableSpawnE!=null) onEnableSpawnE();
		}
		public static void OnSpawnTimer(float timer){
			if(onSpawnTimerE!=null) onSpawnTimerE(timer);
		}
		
		public static void OnLife(int valueChanged){
			if(onLifeE!=null) onLifeE(valueChanged);
		}
		public static void OnGameOver(int _star){
			if(onGameOverE!=null) onGameOverE(_star);
		}
		
		public static void OnResource(int valueChangedList){
			if(onResourceE!=null)
                onResourceE(valueChangedList);
		}
		
		public static void OnUnitDestroyed(Unit unit){
			if(unit.IsCreep()){
				if(onCreepDestroyedE!=null) onCreepDestroyedE(unit.unitC);
			}
			else if(!unit.IsTower()){
				if(onTowerDestroyedE!=null) onTowerDestroyedE(unit.unitT);
			}
		}
		
		public static void OnCreepDestination(UnitCreep creep){
			if(onCreepDestinationE!=null) onCreepDestinationE(creep);
		}
		
		
		public static void OnTowerSold(UnitTower tower){
			if(onTowerSoldE!=null) onTowerSoldE(tower);
		}
		public static void OnTowerUpgraded(UnitTower tower){
			if(onTowerUpgradedE!=null) onTowerUpgradedE(tower);
		}
		public static void OnTowerConstructing(UnitTower tower){
			if(onTowerConstructingE!=null) onTowerConstructingE(tower);
		}
		public static void OnTowerConstructed(UnitTower tower){
			if(onTowerConstructedE!=null) onTowerConstructedE(tower);
		}
		
		public static void OnAbilityActivated(Ability ability){
			if(onAbilityActivatedE!=null) onAbilityActivatedE(ability);
		}
		public static void OnAbilityTargetSelectModeE(bool flag){
			if(onAbilityTargetSelectedE!=null) onAbilityTargetSelectedE(flag);
		}
	}

}
