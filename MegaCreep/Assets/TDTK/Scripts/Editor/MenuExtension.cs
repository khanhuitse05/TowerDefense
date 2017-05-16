using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace TDTK {

	public class MenuExtension : EditorWindow {

        [MenuItem ("ToolsTDTK/New Scene - TD", false, -100)]
		static void New2 () {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
			GameObject camObj=Camera.main.gameObject; 	DestroyImmediate(camObj);
			
			GameObject obj=(GameObject)Instantiate(Resources.Load("ScenePrefab/Template_OpenPath", typeof(GameObject)));
			obj.name="TDTK_OpenPath";

            SpawnManager spawnManager=(SpawnManager)FindObjectOfType(typeof(SpawnManager));
			if(spawnManager.waveList[0].subWaveList[0].unit==null)
				spawnManager.waveList[0].subWaveList[0].unit=CreepDB.GetFirstPrefab().gameObject;
		}
		
		[MenuItem ("ToolsTDTK/CreepEditor", false, 10)]
		static void OpenCreepEditor () {
			UnitCreepEditorWindow.Init();
		}
		
		[MenuItem ("ToolsTDTK/TowerEditor", false, 10)]
		static void OpenTowerEditor () {
			UnitTowerEditorWindow.Init();
		}
		
		[MenuItem ("ToolsTDTK/SpawnEditor", false, 10)]
		static void OpenSpawnEditor () {
			SpawnEditorWindow.Init();
		}
		
		[MenuItem ("ToolsTDTK/AbilityEditor", false, 10)]
		public static void OpenFPSWeaponEditor () {
			AbilityEditorWindow.Init();
		}
		
		[MenuItem ("ToolsTDTK/PerkEditor", false, 10)]
		public static void OpenPerkEditor () {
			PerkEditorWindow.Init();
		}

        [MenuItem ("ToolsTDTK/DamageArmorTable", false, 10)]
		public static void OpenDamageTable () {
			DamageArmorDBEditor.Init();
		}		
	}
}