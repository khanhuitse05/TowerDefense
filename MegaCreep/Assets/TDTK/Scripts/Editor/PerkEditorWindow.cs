using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK
{

    public class PerkEditorWindow : EditorWindow
    {

        private static PerkEditorWindow window;

        public static void Init()
        {
            window = (PerkEditorWindow)EditorWindow.GetWindow(typeof(PerkEditorWindow));
            EditorDBManager.Init();

            InitLabel();
        }

        private static string[] perkTypeLabel;
        private static string[] perkTypeTooltip;

        private static void InitLabel()
        {
            int enumLength = Enum.GetValues(typeof(_PerkType)).Length;
            perkTypeLabel = new string[enumLength];
            perkTypeTooltip = new string[enumLength];
            for (int i = 0; i < enumLength; i++)
            {
                perkTypeLabel[i] = ((_PerkType)i).ToString();

                if ((_PerkType)i == _PerkType.GainLife) perkTypeTooltip[i] = "";
                if ((_PerkType)i == _PerkType.LifeCap) perkTypeTooltip[i] = "";
                if ((_PerkType)i == _PerkType.LifeRegen) perkTypeTooltip[i] = "";
                if ((_PerkType)i == _PerkType.LifeWaveClearedBonus) perkTypeTooltip[i] = "";
            }
        }



        void SelectPerk(int ID)
        {
            selectID = ID;
            GUI.FocusControl("");

            if (selectID * 35 < scrollPos1.y) scrollPos1.y = selectID * 35;
            if (selectID * 35 > scrollPos1.y + listVisibleRect.height - 40) scrollPos1.y = selectID * 35 - listVisibleRect.height + 40;
        }

        private int selectID = 0;

        private Vector2 scrollPos1;
        private Vector2 scrollPos2;

        private GUIContent cont;
        private GUIContent[] contL;

        private float contentHeight = 0;
        private float contentWidth = 0;

        private float spaceX = 120;
        private float spaceY = 20;
        private float width = 150;
        private float height = 18;

        void OnGUI()
        {
            if (window == null) Init();

            List<Perk> perkList = EditorDBManager.GetPerkList();

            if (GUI.Button(new Rect(window.position.width - 120, 5, 100, 25), "Save")) EditorDBManager.SetDirtyPerk();


            if (GUI.Button(new Rect(5, 5, 120, 25), "Create New"))
            {
                int newSelectID = EditorDBManager.AddNewPerk();
                if (newSelectID != -1) SelectPerk(newSelectID);
            }
            if (perkList.Count > 0 && GUI.Button(new Rect(130, 5, 100, 25), "Clone Selected"))
            {
                int newSelectID = EditorDBManager.ClonePerk(selectID);
                if (newSelectID != -1) SelectPerk(newSelectID);
            }


            float startX = 5;
            float startY = 55;


            if (minimiseList)
            {
                if (GUI.Button(new Rect(startX, startY - 20, 30, 18), ">>")) minimiseList = false;
            }
            else
            {
                if (GUI.Button(new Rect(startX, startY - 20, 30, 18), "<<")) minimiseList = true;
            }
            Vector2 v2 = DrawPerkList(startX, startY, perkList);

            startX = v2.x + 25;

            if (perkList.Count == 0) return;


            Rect visibleRect = new Rect(startX, startY, window.position.width - startX - 10, window.position.height - startY - 5);
            Rect contentRect = new Rect(startX, startY, contentWidth - startY, contentHeight);

            scrollPos2 = GUI.BeginScrollView(visibleRect, scrollPos2, contentRect);

            //float cachedX=startX;
            v2 = DrawPerkConfigurator(startX, startY, perkList[selectID]);
            contentWidth = v2.x + 50;
            contentHeight = v2.y - 55;

            GUI.EndScrollView();


            if (GUI.changed) EditorDBManager.SetDirtyPerk();
        }

        int GetListIDFromPerkID(int ID)
        {
            List<Perk> perkList = EditorDBManager.GetPerkList();
            for (int i = 0; i < perkList.Count; i++) { if (perkList[i].ID == ID) return i; }
            return 0;
        }
        int GetPerkIDFromListID(int ID) { return EditorDBManager.GetPerkList()[ID].ID; }


        Vector2 DrawPerkConfigurator(float startX, float startY, Perk perk)
        {

            float cachedX = startX;
            float cachedY = startY;

            EditorUtilities.DrawSprite(new Rect(startX, startY, 60, 60), perk.icon);
            startX += 65;

            cont = new GUIContent("Name:", "The ability name to be displayed in game");
            EditorGUI.LabelField(new Rect(startX, startY += spaceY / 2, width, height), cont);
            perk.name = EditorGUI.TextField(new Rect(startX + spaceX - 65, startY, width - 5, height), perk.name);

            cont = new GUIContent("Icon:", "The ability icon to be displayed in game, must be a sprite");
            EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
            perk.icon = (Sprite)EditorGUI.ObjectField(new Rect(startX + spaceX - 65, startY, width - 5, height), perk.icon, typeof(Sprite), false);

            cont = new GUIContent("Disable Perk:", "Disable Perk");
            EditorGUI.LabelField(new Rect(startX + 295, startY, width, height), cont);
            perk.disablePerk = EditorGUI.Toggle(new Rect(startX + 295 + spaceX, startY, 185, height), perk.disablePerk);

            startX -= 65;
            startY += 30 + spaceY / 2; cachedY = startY;

            cont = new GUIContent("Min level required:", "Minimum level to reach before the perk becoming available. (level are specified in GameControl of each scene)");
            EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
            perk.levelUnlock = EditorGUI.IntField(new Rect(startX + spaceX, startY, 40, height), perk.levelUnlock);

            float temp = cachedY;
            cachedY = startY + 15; startY = temp;
            startX = cachedX + 310;

            GUIStyle style = new GUIStyle("TextArea");
            style.wordWrap = true;
            cont = new GUIContent("Perk description (to be used in runtime): ", "");
            EditorGUI.LabelField(new Rect(startX, startY += spaceY, 400, 20), cont);
            perk.desp = EditorGUI.TextArea(new Rect(startX, startY + spaceY - 3, 270, 50), perk.desp, style);


            startX = cachedX; startY = cachedY;

            Vector2 v2 = DrawPerkType(startX, startY, perk);
            float maxHeight = v2.y + 40;

            return new Vector2(Mathf.Max(startX + 280, v2.x), Mathf.Max(maxHeight, startY + 170));
        }


        Vector2 DrawItemIDTower(float startX, float startY, Perk perk, int limit = 1)
        {
            string[] towerNameList = EditorDBManager.GetTowerNameList();
            List<UnitTower> towerList = EditorDBManager.GetTowerList();

            if (perk.itemIDList.Count == 0) perk.itemIDList.Add(-1);
            while (perk.itemIDList.Count > limit) perk.itemIDList.RemoveAt(perk.itemIDList.Count - 1);

            for (int i = 0; i < perk.itemIDList.Count; i++)
            {
                int ID = perk.itemIDList[i];

                if (ID >= 0)
                {
                    for (int n = 0; n < towerList.Count; n++)
                    {
                        if (towerList[n].prefabID == ID) { ID = n + 1; break; }
                    }
                }

                cont = new GUIContent(" - Tower:", "The tower to add to game when the perk is unlocked");
                EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
                ID = EditorGUI.Popup(new Rect(startX + spaceX - 20, startY, width, 15), ID, towerNameList);
                if (ID > 0 && !perk.itemIDList.Contains(towerList[ID - 1].prefabID)) perk.itemIDList[i] = towerList[ID - 1].prefabID;
                else if (ID == 0) perk.itemIDList[i] = -1;

                //if the list is full, extend it
                if (i == perk.itemIDList.Count - 1 && ID >= 0 && perk.itemIDList.Count < limit) perk.itemIDList.Add(-1);

                //if one of the element in the list is empty, shrink it
                if (i < perk.itemIDList.Count - 1 && perk.itemIDList[i] == -1) { perk.itemIDList.RemoveAt(i); i -= 1; }
            }

            return new Vector2(startX, startY);
        }

        Vector2 DrawItemIDAbility(float startX, float startY, Perk perk, int limit = 1)
        {
            string[] abilityNameList = EditorDBManager.GetAbilityNameList();
            List<Ability> abilityList = EditorDBManager.GetAbilityList();

            if (perk.itemIDList.Count == 0) perk.itemIDList.Add(-1);
            while (perk.itemIDList.Count > limit) perk.itemIDList.RemoveAt(perk.itemIDList.Count - 1);

            for (int i = 0; i < perk.itemIDList.Count; i++)
            {
                int ID = perk.itemIDList[i];

                if (ID >= 0)
                {
                    for (int n = 0; n < abilityList.Count; n++)
                    {
                        if (abilityList[n].ID == ID) { ID = n + 1; break; }
                    }
                }

                cont = new GUIContent(" - Ability:", "The ability to add to game when the perk is unlocked");
                EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
                ID = EditorGUI.Popup(new Rect(startX + spaceX - 20, startY, width, 15), ID, abilityNameList);
                if (ID > 0 && !perk.itemIDList.Contains(abilityList[ID - 1].ID)) perk.itemIDList[i] = abilityList[ID - 1].ID;
                else if (ID == 0) perk.itemIDList[i] = -1;

                //if the list is full, extend it
                if (i == perk.itemIDList.Count - 1 && ID >= 0 && perk.itemIDList.Count < limit) perk.itemIDList.Add(-1);

                //if one of the element in the list is empty, shrink it
                if (i < perk.itemIDList.Count - 1 && perk.itemIDList[i] == -1) { perk.itemIDList.RemoveAt(i); i -= 1; }
            }

            return new Vector2(startX, startY);
        }

        void DrawValue(float startX, float startY, Perk perk, int level, GUIContent cont = null)
        {
            EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
            perk.perkLevel[level].value = EditorGUI.FloatField(new Rect(startX + spaceX, startY, 40, height), perk.perkLevel[level].value);
        }

        void DrawValueMinMax(float startX, float startY, Perk perk, int level, GUIContent cont1 = null, GUIContent cont2 = null)
        {
            EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont1);
            perk.perkLevel[level].value = EditorGUI.FloatField(new Rect(startX + spaceX, startY, 40, height), perk.perkLevel[level].value);

            EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont2);
            perk.perkLevel[level].valueAlt = EditorGUI.FloatField(new Rect(startX + spaceX, startY, 40, height), perk.perkLevel[level].valueAlt);
        }



        private bool minimiseStat = false;

        Vector2 DrawPerkType(float startX, float startY, Perk perk)
        {
            // type
            int type = (int)perk.type;
            cont = new GUIContent("Perk Type:", "What the perk does");
            EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
            contL = new GUIContent[perkTypeLabel.Length];
            for (int i = 0; i < contL.Length; i++) contL[i] = new GUIContent(perkTypeLabel[i], perkTypeTooltip[i]);
            type = EditorGUI.Popup(new Rect(startX + spaceX - 20, startY, width, 15), new GUIContent(""), type, contL);
            perk.type = (_PerkType)type;
            _PerkType perkType = perk.type;
            Vector2 v2;
            if (perkType == _PerkType.TowerSpecific)
            {
                v2 = DrawItemIDTower(startX, startY, perk, 3); startY = v2.y;
            }
            if (perkType == _PerkType.AbilitySpecific)
            {
                DrawItemIDAbility(startX, startY, perk); startY += spaceY;
            }
            // level
            perk.level = Mathf.Clamp(perk.level, 0, perk.perkLevel.Count - 1);
            int level = perk.level;
            cont = new GUIContent("Perk Level:", "Level of perk");
            EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
            contL = new GUIContent[perk.perkLevel.Count];
            for (int i = 0; i < contL.Length; i++) contL[i] = new GUIContent((i + 1).ToString(), "");
            level = EditorGUI.Popup(new Rect(startX + spaceX - 20, startY, 50, 15), new GUIContent(""), level, contL);
            perk.level = level;
            if (GUI.Button(new Rect(startX + spaceX + 40, startY, 40, 15), "-1"))
            {
                if (perk.perkLevel.Count > 1) perk.perkLevel.RemoveAt(perk.perkLevel.Count - 1);
            }
            if (GUI.Button(new Rect(startX + spaceX + 90, startY, 40, 15), "+1"))
            {
                perk.perkLevel.Add(perk.perkLevel[perk.perkLevel.Count - 1].Clone());
            }

            // Detail level

            startY += spaceY * 2;
            minimiseStat = EditorGUI.Foldout(new Rect(startX, startY, width, height), minimiseStat, "Show Stats");

            float offsetDetail = 220;
            float maxHeight = startY;
            float maxWidth = startX + offsetDetail * (perk.perkLevel.Count - 1);
            if (!minimiseStat)
            {
                startY += spaceY;
                startX += 15;
                for (int i = 0; i < perk.perkLevel.Count; i++)
                {
                    EditorGUI.LabelField(new Rect(startX + offsetDetail * i, startY, width, height), "Level " + (i + 1) + " Stats");
                    float _height = DrawLevelDetail(startX + offsetDetail * i, startY, perk, i).y;
                    if (_height > maxHeight)
                    {
                        maxHeight = _height;
                    }
                }
            }

            return new Vector2(maxWidth, maxHeight);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="perk"></param>
        /// <param name="level"></param>
        /// <returns></returns>
		Vector2 DrawLevelDetail(float startX, float startY, Perk perk, int level)
        {
            _PerkType perkType = perk.type;
            // cost
            cont = new GUIContent("Upgrage Cost:", "The resource required to build/upgrade to this level");
            EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
            perk.perkLevel[level].cost = EditorGUI.IntField(new Rect(startX + 100, startY, 85, height), perk.perkLevel[level].cost);
            // stats level
            startX += 10;

            if (perkType == _PerkType.GainLife)
            {
                GUIContent cont1 = new GUIContent(" - Min value:", "Minimum value");
                GUIContent cont2 = new GUIContent(" - Max value:", "Maximum value");
                DrawValueMinMax(startX, startY, perk, level, cont1, cont2);
                startY += spaceY;
            }
            else if (perkType == _PerkType.LifeCap)
            {
                cont = new GUIContent(" - Increase Value:", "value used to modify the existing maximum life capacity");
                DrawValue(startX, startY, perk, level, cont);
            }
            else if (perkType == _PerkType.LifeRegen)
            {
                cont = new GUIContent(" - Increase Value:", "value used to modify the existing life regeneration rate");
                DrawValue(startX, startY, perk, level, cont);
            }
            else if (perkType == _PerkType.LifeWaveClearedBonus)
            {
                GUIContent cont1 = new GUIContent(" - Min value:", "Minimum value");
                GUIContent cont2 = new GUIContent(" - Max value:", "Maximum value");
                DrawValueMinMax(startX, startY, perk, level, cont1, cont2);
                startY += spaceY;
            }
            else if (IsPerkTypeUsesRsc(perkType))
            {
                if (perkType == _PerkType.RscCap) cont = new GUIContent(" - Gain:", "The resource to be gain upon purchasing this perk");
                else if (perkType == _PerkType.RscRegen) cont = new GUIContent(" - Rate modifier:", "The resource to be gain upon purchasing this perk");
                else if (perkType == _PerkType.RscGain) cont = new GUIContent(" - Gain multiplier:", "The resource to be gain upon purchasing this perk");
                else if (perkType == _PerkType.RscCreepKilledGain) cont = new GUIContent(" - Gain multiplier:", "The resource to be gain upon purchasing this perk");
                else if (perkType == _PerkType.RscWaveClearedGain) cont = new GUIContent(" - Gain multiplier:", "The resource to be gain upon purchasing this perk");
                else if (perkType == _PerkType.RscResourceTowerGain) cont = new GUIContent(" - Gain multiplier:", "The resource to be gain upon purchasing this perk");

                //cont=new GUIContent("Gain:", "The resource to be gain upon purchasing this perk");
                EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
                startY += spaceY; float cachedX = startX;

                EditorUtilities.DrawSprite(new Rect(startX + 15, startY - 1, 20, 20), EditorDBManager.iconRsc);
                perk.perkLevel[level].valueRsc = EditorGUI.FloatField(new Rect(startX + 35, startY, 40, height), perk.perkLevel[level].valueRsc);

                startX = cachedX; startY += 5;
            }

            //~ if(IsPerkTypeUsesUnitStats(perkType)){
            if (perkType == _PerkType.Tower || perkType == _PerkType.TowerSpecific)
            {
                Vector2 v2;
                v2 = DrawTowerStat(startX, startY, perk, level); startY = v2.y;
            }

            if (perkType == _PerkType.Ability || perkType == _PerkType.AbilitySpecific)
            {
                Vector2 v2 = DrawAbilityStat(startX, startY, perk, level); startY = v2.y;
            }

            else if (perkType == _PerkType.EnergyRegen)
            {
                cont = new GUIContent(" - Increase Value:", "value used to modify the existing energy regeneration rate");
                DrawValue(startX, startY, perk, level, cont);
            }
            else if (perkType == _PerkType.EnergyIncreaseCap)
            {
                cont = new GUIContent(" - Increase Value:", "value used to modify the existing maximum energy capacity");
                DrawValue(startX, startY, perk, level, cont);
            }
            else if (perkType == _PerkType.EnergyCreepKilledBonus)
            {
                GUIContent cont1 = new GUIContent(" - Min value:", "Minimum value");
                GUIContent cont2 = new GUIContent(" - Max value:", "Maximum value");
                DrawValueMinMax(startX, startY, perk, level, cont1, cont2);
                startY += spaceY;
            }
            else if (perkType == _PerkType.EnergyWaveClearedBonus)
            {
                GUIContent cont1 = new GUIContent(" - Min value:", "Minimum value");
                GUIContent cont2 = new GUIContent(" - Max value:", "Maximum value");
                DrawValueMinMax(startX, startY, perk, level, cont1, cont2);
                startY += spaceY;
            }
            // des level

            startY += spaceY;
            GUIStyle style = new GUIStyle("TextArea");
            style.wordWrap = true;
            cont = new GUIContent("Level Description:", "Level Description");
            EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
            perk.perkLevel[level].desp = EditorGUI.TextArea(new Rect(startX, startY + spaceY - 3, 175, 75), perk.perkLevel[level].desp, style);
            startY += 75;
            //
            return new Vector2(startX, startY);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="perk"></param>
        /// <returns></returns>
        /// 
        private bool foldGeneralParameter = true;
        Vector2 DrawTowerStat(float startX, float startY, Perk perk, int level)
        {
            startY += 5;

            foldGeneralParameter = EditorGUI.Foldout(new Rect(startX, startY += spaceY, width, height), foldGeneralParameter, "Show General Stats");

            if (foldGeneralParameter)
            {
                startX += 15;

                cont = new GUIContent("HP:", "HP multiplier of the tower. Takes value from 0 and above with 0.2 being 20% increment");
                EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
                perk.perkLevel[level].HP = EditorGUI.FloatField(new Rect(startX + spaceX, startY, 40, height), perk.perkLevel[level].HP);

                cont = new GUIContent("Build Cost:", "Build cost multiplier of the tower. Takes value from 0-1 with 0.2 being 20% decrease in cost");
                EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
                perk.perkLevel[level].buildCost = EditorGUI.FloatField(new Rect(startX + spaceX, startY, 40, height), perk.perkLevel[level].buildCost);
                cont = new GUIContent("Upgrade Cost:", "Upgrade cost multiplier of the tower. Takes value from 0-1 with 0.2 being 20% decrease in cost");
                EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
                perk.perkLevel[level].upgradeCost = EditorGUI.FloatField(new Rect(startX + spaceX, startY, 40, height), perk.perkLevel[level].upgradeCost);

                startX -= 15;
            }

            Vector2 v2 = DrawUnitStat(startX, startY + 5, perk.perkLevel[level].stats, false);
            startY = v2.y;

            return new Vector2(startX, startY);
        }

        Vector2 DrawAbilityStat(float startX, float startY, Perk perk, int level)
        {
            startY += 5;

            //~ if(perk.itemIDList[0]==-1) return;

            //~ Ability ability=null; 
            //~ List<Ability> abilityList=EditorDBManager.GetAbilityList();
            //~ for(int i=0; i<abilityList.Count; i++){ if(abilityList[i].ID==perk.itemIDList[0]) ability=abilityList[i]; }

            cont = new GUIContent("Cost:", "Multiplier to the ability energy cost. Takes value from 0-1 with 0.3 being decrease energy cost by 30%");
            EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
            perk.perkLevel[level].abCost = EditorGUI.FloatField(new Rect(startX + spaceX, startY, 40, height), perk.perkLevel[level].abCost);
            cont = new GUIContent("Cooldown:", "Multiplier to the ability cooldown duration. Takes value from 0-1 with 0.3 being decrease cooldown duration by 30%");
            EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
            perk.perkLevel[level].abCooldown = EditorGUI.FloatField(new Rect(startX + spaceX, startY, 40, height), perk.perkLevel[level].abCooldown);
            cont = new GUIContent("AOE Radius:", "Multiplier to the ability AOE radius. Takes value from 0 and above with 0.3 being increment of 30%");
            EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
            perk.perkLevel[level].abAOERadius = EditorGUI.FloatField(new Rect(startX + spaceX, startY, 40, height), perk.perkLevel[level].abAOERadius);


            startY += 5;

            cont = new GUIContent("Duration:", "Duration multiplier. Takes value from 0 and above with 0.3 being increase duration by 30%");
            EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
            perk.perkLevel[level].effects.duration = EditorGUI.FloatField(new Rect(startX + spaceX, startY, 40, height), perk.perkLevel[level].effects.duration);
            perk.perkLevel[level].effects.dot.duration = perk.perkLevel[level].effects.duration;
            perk.perkLevel[level].effects.slow.duration = perk.perkLevel[level].effects.duration;

            startY += 5;

            cont = new GUIContent("Damage:", "Damage multiplier. Takes value from 0 and above with 0.3 being increase existing effect damage by 30%");
            EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
            perk.perkLevel[level].effects.damageMin = EditorGUI.FloatField(new Rect(startX + spaceX, startY, 40, height), perk.perkLevel[level].effects.damageMin);

            cont = new GUIContent("Stun Chance:", "Duration modifier. Takes value from 0 and above with 0.3 being increase stun chance by 30%");
            EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
            perk.perkLevel[level].effects.stunChance = EditorGUI.FloatField(new Rect(startX + spaceX, startY, 40, height), perk.perkLevel[level].effects.stunChance);

            startY += 5;

            cont = new GUIContent("Slow", "");
            EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont); startY -= spaceY;

            cont = new GUIContent("         - Multiplier:", "Move speed multiplier. Takes value from 0-1 with with 0.3 being decrese default speed by 30%");
            EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
            perk.perkLevel[level].effects.slow.slowMultiplier = EditorGUI.FloatField(new Rect(startX + spaceX, startY, 40, height), perk.perkLevel[level].effects.slow.slowMultiplier);


            startY += 5;

            cont = new GUIContent("Dot", "Damage over time");
            EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont); startY -= spaceY;

            cont = new GUIContent("        - Damage:", "Damage multiplier to DOT. Takes value from 0 and above with with 0.3 being increase the tick damage by 30%");
            EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
            perk.perkLevel[level].effects.dot.value = EditorGUI.FloatField(new Rect(startX + spaceX, startY, 40, height), perk.perkLevel[level].effects.dot.value);

            startY += 5;

            cont = new GUIContent("DamageBuff:", "Damage buff modifer. Takes value from 0 and above with 0.3 being increase existing damage by 30%");
            EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
            perk.perkLevel[level].effects.damageBuff = EditorGUI.FloatField(new Rect(startX + spaceX, startY, 40, height), perk.perkLevel[level].effects.damageBuff);

            cont = new GUIContent("RangeBuff:", "Range buff modifer. Takes value from 0 and above with 0.3 being increase existing range by 30%");
            EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
            perk.perkLevel[level].effects.rangeBuff = EditorGUI.FloatField(new Rect(startX + spaceX, startY, 40, height), perk.perkLevel[level].effects.rangeBuff);

            cont = new GUIContent("CDBuff:", "Cooldown buff modifer. Takes value from 0 and above with 0.3 being reduce existing cooldown by 30%");
            EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
            perk.perkLevel[level].effects.cooldownBuff = EditorGUI.FloatField(new Rect(startX + spaceX, startY, 40, height), perk.perkLevel[level].effects.cooldownBuff);

            cont = new GUIContent("HPGain:", "HP Gain multiplier. Takes value from 0 and above with 0.3 being increase existing effect HP gain value by 30%.");
            EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
            perk.perkLevel[level].effects.HPGainMin = EditorGUI.FloatField(new Rect(startX + spaceX, startY, 40, height), perk.perkLevel[level].effects.HPGainMin);


            return new Vector2(startX, startY);
        }


        private bool foldOffenseParameter = true;
        private bool foldSupportParameter = true;
        private bool foldRscParameter = true;
        Vector2 DrawUnitStat(float startX, float startY, UnitStat stats, bool isWeapon)
        {
            float fWidth = 40;

            if (!isWeapon) foldOffenseParameter = EditorGUI.Foldout(new Rect(startX, startY += spaceY, width, height), foldOffenseParameter, "Show Offensive Stats");
            if (isWeapon || foldOffenseParameter)
            {
                startX += 15;

                cont = new GUIContent("Damage:", "");
                EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
                stats.damageMin = EditorGUI.FloatField(new Rect(startX + spaceX, startY, fWidth, height), stats.damageMin);

                cont = new GUIContent("Cooldown:", "");
                EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
                stats.cooldown = EditorGUI.FloatField(new Rect(startX + spaceX, startY, fWidth, height), stats.cooldown);

                if (!isWeapon)
                {
                    cont = new GUIContent("Range:", "");
                    EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
                    stats.range = EditorGUI.FloatField(new Rect(startX + spaceX, startY, fWidth, height), stats.range);
                }

                cont = new GUIContent("AOE Radius:", "");
                EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
                stats.aoeRadius = EditorGUI.FloatField(new Rect(startX + spaceX, startY, fWidth, height), stats.aoeRadius);

                cont = new GUIContent("Stun", "");
                EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont); startY -= spaceY;

                cont = new GUIContent("        - Chance:", "Chance to stun the target in each successful attack. Takes value from 0-1 with 0 being 0% and 1 being 100%");
                EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
                stats.stun.chance = EditorGUI.FloatField(new Rect(startX + spaceX, startY, fWidth, height), stats.stun.chance);

                cont = new GUIContent("        - Duration:", "The stun duration in second");
                EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
                stats.stun.duration = EditorGUI.FloatField(new Rect(startX + spaceX, startY, fWidth, height), stats.stun.duration);

                cont = new GUIContent("Critical", "");
                EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont); startY -= spaceY;

                cont = new GUIContent("            - Chance:", "Chance to score critical hit in attack. Takes value from 0-1 with 0 being 0% and 1 being 100%");
                EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
                stats.crit.chance = EditorGUI.FloatField(new Rect(startX + spaceX, startY, fWidth, height), stats.crit.chance);

                cont = new GUIContent("            - Multiplier:", "Damage multiplier for successful critical hit. Takes value from 0 and above with with 0.5 being 50% of normal damage as bonus");
                EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
                stats.crit.dmgMultiplier = EditorGUI.FloatField(new Rect(startX + spaceX, startY, fWidth, height), stats.crit.dmgMultiplier);


                cont = new GUIContent("Slow", "");
                EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont); startY -= spaceY;

                cont = new GUIContent("         - Duration:", "The effect duration in second");
                EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
                stats.slow.duration = EditorGUI.FloatField(new Rect(startX + spaceX, startY, fWidth, height), stats.slow.duration);

                cont = new GUIContent("         - Multiplier:", "Move speed multiplier. Takes value from 0-1 with with 0.7 being decrese default speed by 30%");
                EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
                stats.slow.slowMultiplier = EditorGUI.FloatField(new Rect(startX + spaceX, startY, fWidth, height), stats.slow.slowMultiplier);



                cont = new GUIContent("Dot", "Damage over time");
                EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont); startY -= spaceY;

                cont = new GUIContent("        - Duration:", "The effect duration in second");
                EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
                stats.dot.duration = EditorGUI.FloatField(new Rect(startX + spaceX, startY, fWidth, height), stats.dot.duration);

                cont = new GUIContent("        - Interval:", "Duration between each tick. Damage is applied at each tick.");
                EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
                stats.dot.interval = EditorGUI.FloatField(new Rect(startX + spaceX, startY, fWidth, height), stats.dot.interval);

                cont = new GUIContent("        - Damage:", "Damage applied at each tick");
                EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
                stats.dot.value = EditorGUI.FloatField(new Rect(startX + spaceX, startY, fWidth, height), stats.dot.value);

                startX -= 15;
            }

            if (!isWeapon) foldSupportParameter = EditorGUI.Foldout(new Rect(startX, startY += spaceY, width, height), foldSupportParameter, "Show Support Stats");
            if (!isWeapon && foldSupportParameter)
            {
                startX += 15;

                cont = new GUIContent("Buff:", "Note: Buffs from multple tower doesnt stack, however when there's difference in the buff strength, the stronger buff applies. A tower can gain maximum dmage buff from one source and maximum range buff from another");
                EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont); startY -= spaceY;

                cont = new GUIContent("        - Damage:", "Damage buff multiplier. Takes value from 0 and above with 0.5 being 50% increase in damage");
                EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
                stats.buff.damageBuff = EditorGUI.FloatField(new Rect(startX + spaceX, startY, fWidth, height), stats.buff.damageBuff);

                cont = new GUIContent("        - Cooldown:", "Dooldown buff multiplier. Takes value from 0-1 with 0.2 being reduce cooldown by 20%");
                EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
                stats.buff.cooldownBuff = EditorGUI.FloatField(new Rect(startX + spaceX, startY, fWidth, height), stats.buff.cooldownBuff);

                cont = new GUIContent("        - Range:", "Range buff multiplier. Takes value from 0 and above with 0.5 being 50% increase in range");
                EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
                stats.buff.rangeBuff = EditorGUI.FloatField(new Rect(startX + spaceX, startY, fWidth, height), stats.buff.rangeBuff);

                cont = new GUIContent("        - Critical:", "Critical hit chance buff modifier. Takes value from 0 and above with 0.25 being 25% increase in critical hit chance");
                EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
                stats.buff.criticalBuff = EditorGUI.FloatField(new Rect(startX + spaceX, startY, fWidth, height), stats.buff.criticalBuff);

                cont = new GUIContent("        - HP Regen:", "HP Regeneration Buff. Takes value from 0 and above with 2 being gain 2HP second ");
                EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
                stats.buff.regenHP = EditorGUI.FloatField(new Rect(startX + spaceX, startY, fWidth, height), stats.buff.regenHP);

                startX -= 15;
            }

            if (!isWeapon) foldRscParameter = EditorGUI.Foldout(new Rect(startX, startY += spaceY, width, height), foldRscParameter, "Show RscGain");
            if (!isWeapon && foldRscParameter)
            {
                startX += 15;

                cont = new GUIContent("Resource Gain:", "The resource gain by unit at each cooldown interval\nOnly applicable to ResourceTower");
                EditorGUI.LabelField(new Rect(startX, startY += spaceY, width, height), cont);
                startY += spaceY;

                EditorUtilities.DrawSprite(new Rect(startX + 10, startY - 1, 20, 20), EditorDBManager.iconRsc);
                stats.rscGain = EditorGUI.IntField(new Rect(startX + 30, startY, fWidth, height), stats.rscGain);

                startX -= 15;
            }

            return new Vector2(startX, startY);
        }


        bool IsPerkTypeUsesRsc(_PerkType type)
        {
            if (type == _PerkType.RscCap) return true;
            else if (type == _PerkType.RscRegen) return true;
            else if (type == _PerkType.RscGain) return true;
            else if (type == _PerkType.RscCreepKilledGain) return true;
            else if (type == _PerkType.RscWaveClearedGain) return true;
            else if (type == _PerkType.RscResourceTowerGain) return true;
            return false;
        }

        bool IsPerkTypeUsesUnitStats(_PerkType type)
        {
            if (type == _PerkType.Tower) return true;
            else if (type == _PerkType.TowerSpecific) return true;
            else if (type == _PerkType.Ability) return true;
            else if (type == _PerkType.AbilitySpecific) return true;
            return false;
        }








        private Rect listVisibleRect;
        private Rect listContentRect;

        private int deleteID = -1;
        private bool minimiseList = false;
        Vector2 DrawPerkList(float startX, float startY, List<Perk> perkList)
        {

            float width = 260;
            if (minimiseList) width = 60;


            if (!minimiseList)
            {
                if (GUI.Button(new Rect(startX + 180, startY - 20, 40, 18), "up"))
                {
                    if (selectID > 0)
                    {
                        Perk perk = perkList[selectID];
                        perkList[selectID] = perkList[selectID - 1];
                        perkList[selectID - 1] = perk;
                        selectID -= 1;

                        if (selectID * 35 < scrollPos1.y) scrollPos1.y = selectID * 35;
                    }
                }
                if (GUI.Button(new Rect(startX + 222, startY - 20, 40, 18), "down"))
                {
                    if (selectID < perkList.Count - 1)
                    {
                        Perk perk = perkList[selectID];
                        perkList[selectID] = perkList[selectID + 1];
                        perkList[selectID + 1] = perk;
                        selectID += 1;

                        if (listVisibleRect.height - 35 < selectID * 35) scrollPos1.y = (selectID + 1) * 35 - listVisibleRect.height + 5;
                    }
                }
            }


            listVisibleRect = new Rect(startX, startY, width + 15, window.position.height - startY - 5);
            listContentRect = new Rect(startX, startY, width, perkList.Count * 35 + 5);

            GUI.color = new Color(.8f, .8f, .8f, 1f);
            GUI.Box(listVisibleRect, "");
            GUI.color = Color.white;

            scrollPos1 = GUI.BeginScrollView(listVisibleRect, scrollPos1, listContentRect);


            startY += 5; startX += 5;

            for (int i = 0; i < perkList.Count; i++)
            {

                EditorUtilities.DrawSprite(new Rect(startX, startY + (i * 35), 30, 30), perkList[i].icon);

                if (minimiseList)
                {
                    if (selectID == i) GUI.color = new Color(0, 1f, 1f, 1f);
                    if (GUI.Button(new Rect(startX + 35, startY + (i * 35), 30, 30), "")) SelectPerk(i);
                    GUI.color = Color.white;

                    continue;
                }



                if (selectID == i) GUI.color = new Color(0, 1f, 1f, 1f);
                if (GUI.Button(new Rect(startX + 35, startY + (i * 35), 150, 30), perkList[i].name)) SelectPerk(i);
                GUI.color = Color.white;

                if (deleteID == i)
                {

                    if (GUI.Button(new Rect(startX + 190, startY + (i * 35), 60, 15), "cancel")) deleteID = -1;

                    GUI.color = Color.red;
                    if (GUI.Button(new Rect(startX + 190, startY + (i * 35) + 15, 60, 15), "confirm"))
                    {
                        if (selectID >= deleteID) SelectPerk(Mathf.Max(0, selectID - 1));
                        perkList.RemoveAt(deleteID);
                        deleteID = -1;
                    }
                    GUI.color = Color.white;
                }
                else
                {
                    if (GUI.Button(new Rect(startX + 190, startY + (i * 35), 60, 15), "remove")) deleteID = i;
                }
            }

            GUI.EndScrollView();

            return new Vector2(startX + width, startY);
        }

    }
}