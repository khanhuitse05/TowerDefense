using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK
{

    [CustomEditor(typeof(PerkManager))]
    public class PerkManagerEditor : Editor
    {

        private static PerkManager instance;

        private bool showDefaultFlag = false;
        private bool showPerkList = true;

        private static List<Perk> perkList = new List<Perk>();

        private GUIContent cont;

        void Awake()
        {
            instance = (PerkManager)target;

            GetPerk();

            EditorUtility.SetDirty(instance);
        }

        private static void GetPerk()
        {
            EditorDBManager.Init();
            perkList = EditorDBManager.GetPerkList();
        }

        public override void OnInspectorGUI()
        {
            GUI.changed = false;

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.MaxWidth(10));
            showPerkList = EditorGUILayout.Foldout(showPerkList, "Show Perk List");
            EditorGUILayout.EndHorizontal();
            if (showPerkList)
            {

                for (int i = 0; i < perkList.Count; i++)
                {

                    Perk perk = perkList[i];
                    if (perk.disablePerk)
                    {
                        continue;
                    }
                    GUILayout.BeginHorizontal();

                    GUILayout.Box("", GUILayout.Width(40), GUILayout.Height(40));
                    Rect rect = GUILayoutUtility.GetLastRect();
                    EditorUtilities.DrawSprite(rect, perk.icon, false);

                    GUILayout.BeginVertical();
                    EditorGUILayout.Space();
                    GUILayout.Label(perk.name, GUILayout.ExpandWidth(false));
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Open PerkEditor"))
            {
                PerkEditorWindow.Init();
            }
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.MaxWidth(10));
            showDefaultFlag = EditorGUILayout.Foldout(showDefaultFlag, "Show default editor");
            EditorGUILayout.EndHorizontal();
            if (showDefaultFlag) DrawDefaultInspector();


            if (GUI.changed) EditorUtility.SetDirty(instance);

        }
    }
}