using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK
{

    public class EditorDBManager : EditorWindow
    {

        private static bool init = false;
        public static Sprite iconRsc;
        public static Sprite iconMana;
        public static void Init()
        {
            if (init) return;
            iconRsc = Resources.Load<Sprite>("Image/ic_gold");
            iconMana = Resources.Load<Sprite>("Image/ic_mana");
            LoadDamageArmorTable();
            LoadTower();
            LoadCreep();
            LoadAbility();
            LoadPerk();
        }

        private static DamageArmorDB DAPrefab;
        private static List<ArmorType> armorTypeList;
        private static List<DamageType> damageTypeList;
        private static string[] damageTypeLabel;
        private static string[] armorTypeLabel;
        private static void LoadDamageArmorTable()
        {
            DAPrefab = DamageArmorDB.LoadDB();
            armorTypeList = DAPrefab.armorTypeList;
            damageTypeList = DAPrefab.damageTypeList;
            UpdateDamageArmorLabel();
        }
        private static void UpdateDamageArmorLabel()
        {
            damageTypeLabel = new string[DAPrefab.damageTypeList.Count];
            for (int i = 0; i < DAPrefab.damageTypeList.Count; i++) damageTypeLabel[i] = DAPrefab.damageTypeList[i].name;
            armorTypeLabel = new string[DAPrefab.armorTypeList.Count];
            for (int i = 0; i < DAPrefab.armorTypeList.Count; i++) armorTypeLabel[i] = DAPrefab.armorTypeList[i].name;
        }

        public static List<ArmorType> GetArmorTypeList() { return armorTypeList; }
        public static List<DamageType> GetDamageTypeList() { return damageTypeList; }
        public static string[] GetDamageTypeLabel() { return damageTypeLabel; }
        public static string[] GetArmorTypeLabel() { return armorTypeLabel; }
        public static void SetDirtyDamageArmor()
        {
            UpdateDamageArmorLabel();
            EditorUtility.SetDirty(DAPrefab);
        }

        public static void AddNewArmorType()
        {
            ArmorType armorType = new ArmorType();
            armorType.name = "Armor" + DAPrefab.armorTypeList.Count;
            DAPrefab.armorTypeList.Add(armorType);
            UpdateDamageArmorLabel();
        }
        public static void AddNewDamageType()
        {
            DamageType damageType = new DamageType();
            damageType.name = "Damage" + DAPrefab.damageTypeList.Count;
            DAPrefab.damageTypeList.Add(damageType);
            UpdateDamageArmorLabel();
        }
        public static void RemoveArmorType(int listID)
        {
            DAPrefab.armorTypeList.RemoveAt(listID);
            UpdateDamageArmorLabel();
        }
        public static void RemoveDamageType(int listID)
        {
            DAPrefab.damageTypeList.RemoveAt(listID);
            UpdateDamageArmorLabel();
        }


        private static TowerDB towerDBPrefab;
        private static List<UnitTower> towerList = new List<UnitTower>();
        private static List<int> towerIDList = new List<int>();
        private static string[] towerNameList = new string[0];
        private static void LoadTower()
        {
            towerDBPrefab = TowerDB.LoadDB();
            towerList = towerDBPrefab.towerList;

            for (int i = 0; i < towerList.Count; i++)
            {
                //towerList[i].prefabID=i;
                if (towerList[i] != null)
                {
                    towerIDList.Add(towerList[i].prefabID);
                    if (towerList[i].stats.Count == 0) towerList[i].stats.Add(new UnitStat());
                }
                else
                {
                    towerList.RemoveAt(i);
                    i -= 1;
                }
            }

            UpdateTowerNameList();
        }
        private static void UpdateTowerNameList()
        {
            List<string> tempList = new List<string>();
            tempList.Add(" - ");
            for (int i = 0; i < towerList.Count; i++)
            {
                string name = towerList[i].unitName;
                while (tempList.Contains(name)) name += ".";
                tempList.Add(name);
            }

            towerNameList = new string[tempList.Count];
            for (int i = 0; i < tempList.Count; i++) towerNameList[i] = tempList[i];
        }

        public static List<UnitTower> GetTowerList() { return towerList; }
        public static string[] GetTowerNameList() { return towerNameList; }
        public static List<int> GetTowerIDList() { return towerIDList; }
        public static void SetDirtyTower()
        {
            EditorUtility.SetDirty(towerDBPrefab);
            for (int i = 0; i < towerList.Count; i++) EditorUtility.SetDirty(towerList[i]);
        }

        public static int AddNewTower(UnitTower newTower)
        {
            if (towerList.Contains(newTower)) return -1;

            int ID = GenerateNewID(towerIDList);
            newTower.prefabID = ID;
            towerIDList.Add(ID);
            towerList.Add(newTower);

            UpdateTowerNameList();

            if (newTower.stats.Count == 0) newTower.stats.Add(new UnitStat());

            SetDirtyTower();

            return towerList.Count - 1;
        }
        public static void RemoveTower(int listID)
        {
            towerIDList.Remove(towerList[listID].prefabID);
            towerList.RemoveAt(listID);
            UpdateTowerNameList();
            SetDirtyTower();
        }




        private static CreepDB creepDBPrefab;
        private static List<UnitCreep> creepList = new List<UnitCreep>();
        private static List<int> creepIDList = new List<int>();
        private static string[] creepNameList = new string[0];
        private static void LoadCreep()
        {
            creepDBPrefab = CreepDB.LoadDB();
            creepList = creepDBPrefab.creepList;

            for (int i = 0; i < creepList.Count; i++)
            {
                //creepList[i].prefabID=i;
                if (creepList[i] != null)
                {
                    creepIDList.Add(creepList[i].prefabID);
                    if (creepList[i].stats.Count == 0) creepList[i].stats.Add(new UnitStat());
                }
                else
                {
                    creepList.RemoveAt(i);
                    i -= 1;
                }
            }

            UpdateCreepNameList();
        }
        private static void UpdateCreepNameList()
        {
            List<string> tempList = new List<string>();
            tempList.Add(" - ");
            for (int i = 0; i < creepList.Count; i++)
            {
                string name = creepList[i].unitName;
                while (tempList.Contains(name)) name += ".";
                tempList.Add(name);
            }

            creepNameList = new string[tempList.Count];
            for (int i = 0; i < tempList.Count; i++) creepNameList[i] = tempList[i];
        }

        public static List<UnitCreep> GetCreepList() { return creepList; }
        public static string[] GetCreepNameList() { return creepNameList; }
        public static List<int> GetCreepIDList() { return creepIDList; }
        public static void SetDirtyCreep()
        {
            EditorUtility.SetDirty(creepDBPrefab);
            for (int i = 0; i < creepList.Count; i++) EditorUtility.SetDirty(creepList[i]);
        }

        public static int AddNewCreep(UnitCreep newCreep)
        {
            if (creepList.Contains(newCreep)) return -1;

            int ID = GenerateNewID(creepIDList);
            newCreep.prefabID = ID;
            creepIDList.Add(ID);
            creepList.Add(newCreep);

            UpdateCreepNameList();

            if (newCreep.stats.Count == 0) newCreep.stats.Add(new UnitStat());

            SetDirtyCreep();
            return creepList.Count - 1;
        }
        public static void RemoveCreep(int listID)
        {
            UnitCreep removedCreep = creepList[listID];
            creepIDList.Remove(creepList[listID].prefabID);
            creepList.RemoveAt(listID);
            UpdateCreepNameList();
            SetDirtyCreep();
            RefreshCreep_Remove(removedCreep);
        }


        private static AbilityDB abilityPrefabDB;
        private static List<Ability> abilityList = new List<Ability>();
        private static List<int> abilityIDList = new List<int>();
        private static string[] abilityNameList = new string[0];
        private static void LoadAbility()
        {
            abilityPrefabDB = AbilityDB.LoadDB();
            abilityList = abilityPrefabDB.abilityList;

            for (int i = 0; i < abilityList.Count; i++)
            {
                //abilityList[i].ID=i;
                if (abilityList[i] != null)
                {
                    abilityIDList.Add(abilityList[i].ID);
                }
                else
                {
                    abilityList.RemoveAt(i);
                    i -= 1;
                }
            }

            UpdateAbilityNameList();
        }
        private static void UpdateAbilityNameList()
        {
            List<string> tempList = new List<string>();
            tempList.Add(" - ");
            for (int i = 0; i < abilityList.Count; i++)
            {
                string name = abilityList[i].name;
                while (tempList.Contains(name)) name += ".";
                tempList.Add(name);
            }

            abilityNameList = new string[tempList.Count];
            for (int i = 0; i < tempList.Count; i++) abilityNameList[i] = tempList[i];
        }

        public static List<Ability> GetAbilityList() { return abilityList; }
        public static string[] GetAbilityNameList() { return abilityNameList; }
        public static List<int> GetAbilityIDList() { return abilityIDList; }
        public static void SetDirtyAbility()
        {
            EditorUtility.SetDirty(abilityPrefabDB);
        }

        private static PerkDB perkDBPrefab;
        private static List<Perk> perkList = new List<Perk>();
        private static List<int> perkIDList = new List<int>();
        private static string[] perkNameList = new string[0];
        private static void LoadPerk()
        {
            perkDBPrefab = PerkDB.LoadDB();
            perkList = perkDBPrefab.perkList;

            for (int i = 0; i < perkList.Count; i++)
            {
                if (perkList[i] != null)
                {
                    perkIDList.Add(perkList[i].ID);
                    if (perkList[i].perkLevel.Count == 0) perkList[i].perkLevel.Add(new PerkLevel());
                }
                else
                {
                    perkList.RemoveAt(i);
                    i -= 1;
                }
            }
            UpdatePerkNameList();
        }
        private static void UpdatePerkNameList()
        {
            List<string> tempList = new List<string>();
            tempList.Add(" - ");
            for (int i = 0; i < perkList.Count; i++)
            {
                string name = perkList[i].name;
                while (tempList.Contains(name)) name += ".";
                tempList.Add(name);
            }

            perkNameList = new string[tempList.Count];
            for (int i = 0; i < tempList.Count; i++) perkNameList[i] = tempList[i];
        }

        public static List<Perk> GetPerkList() { return perkList; }
        public static string[] GetPerkNameList() { return perkNameList; }
        public static List<int> GetPerkIDList() { return perkIDList; }
        public static void SetDirtyPerk()
        {
            EditorUtility.SetDirty(perkDBPrefab);
        }

        public static int AddNewPerk()
        {
            Perk perk = new Perk();
            perk.ID = GenerateNewID(perkIDList);
            perk.name = "Perk " + perk.ID;
            perkIDList.Add(perk.ID);
            perkList.Add(perk);
            UpdatePerkNameList();
            SetDirtyPerk();
            return perkList.Count - 1;
        }
        public static int ClonePerk(int listID)
        {
            Perk perk = perkList[listID].Clone();
            perk.ID = GenerateNewID(perkIDList);
            perkIDList.Add(perk.ID);
            perk.name += " (Clone)";
            perkList.Insert(listID + 1, perk);
            UpdatePerkNameList();
            SetDirtyPerk();
            return listID + 1;
        }
        public static void RemovePerk(int listID)
        {
            perkIDList.Remove(perkList[listID].ID);
            perkList.RemoveAt(listID);
            UpdatePerkNameList();
        }

        private static int GenerateNewID(List<int> list)
        {
            int ID = 0;
            while (list.Contains(ID)) ID += 1;
            return ID;
        }




        private static void RefreshRsc()
        {
            for (int i = 0; i < towerList.Count; i++)
            {
                UnitTower tower = towerList[i];
                for (int n = 0; n < tower.stats.Count; n++)
                {
                    UnitStat stat = tower.stats[n];
                }
            }

            for (int i = 0; i < creepList.Count; i++)
            {
                UnitCreep creep = creepList[i];
            }
        }

        private static void RefreshCreep_Remove(UnitCreep creep)
        {
            for (int i = 0; i < creepList.Count; i++)
            {
                if (creepList[i].spawnUponDestroyed == creep.gameObject)
                {
                    creepList[i].spawnUponDestroyed = null;
                }
            }
        }
    }

}