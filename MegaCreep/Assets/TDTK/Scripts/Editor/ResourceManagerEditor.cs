using UnityEngine;
using UnityEditor;

namespace TDTK {

	[CustomEditor(typeof(ResourceManager))]
	public class ResourceManagerEditor : Editor {

		private static ResourceManager instance;

		void Awake(){
			instance = (ResourceManager)target;
		}
		
		private static bool showDefaultFlag=false;
		private GUIContent cont;
		
		
		public override void OnInspectorGUI(){
			
			GUI.changed = false;
			EditorGUILayout.Space();
			
			EditorGUILayout.Space();
			
			EditorGUILayout.BeginHorizontal();
            cont =new GUIContent("Enable Rsc Regen:", "Check to have the player Rsc regenerate overtime");
			instance.enableRscGen=EditorGUILayout.Toggle(cont, instance.enableRscGen);
			
			if(instance.enableRscGen)
            {
				cont=new GUIContent("  Rate:", "The rate at which the player rsc regenerate (per second)");
				EditorGUILayout.LabelField(cont, GUILayout.MaxWidth(45));
				instance.rscGenRate = EditorGUILayout.FloatField(instance.rscGenRate);
			}
            EditorGUILayout.EndHorizontal();

            cont = new GUIContent("Resource Start:", "The Resource start of map");
            instance.rsc = EditorGUILayout.IntField(cont, instance.rsc);

			EditorGUILayout.Space();
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("", GUILayout.MaxWidth(10));
			showDefaultFlag=EditorGUILayout.Foldout(showDefaultFlag, "Show default editor");
			EditorGUILayout.EndHorizontal();
			if(showDefaultFlag) DrawDefaultInspector();
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
	}

}