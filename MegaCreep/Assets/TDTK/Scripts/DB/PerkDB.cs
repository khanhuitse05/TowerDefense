using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	public class PerkDB : MonoBehaviour {

		public List<Perk> perkList=new List<Perk>();
		
		public static PerkDB LoadDB(){
			GameObject obj=Resources.Load("DB_TDTK/PerkDB", typeof(GameObject)) as GameObject;
			
			#if UNITY_EDITOR
				if(obj==null) obj=CreatePrefab();
			#endif
			
			return obj.GetComponent<PerkDB>();
		}
		
		public static List<Perk> Load(){
			GameObject obj=Resources.Load("DB_TDTK/PerkDB", typeof(GameObject)) as GameObject;
			
			#if UNITY_EDITOR
				if(obj==null) obj=CreatePrefab();
			#endif
			
			PerkDB instance=obj.GetComponent<PerkDB>();
			return instance.perkList;
		}

#if UNITY_EDITOR
        private static GameObject CreatePrefab()
        {
            GameObject obj = new GameObject();
            obj.AddComponent<PerkDB>();
            GameObject prefab = PrefabUtility.CreatePrefab("Assets/TDTK/Resources/DB_TDTK/PerkDB.prefab", obj, ReplacePrefabOptions.ConnectToPrefab);
            DestroyImmediate(obj);
            AssetDatabase.Refresh();
            return prefab;
        }
		#endif
		
	}

}
