using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TDTK
{

    public class PerkTowerModifier
    {
        public int prefabID = -1;

        public float HP = 0;

        public float buildCost = 0;
        public float upgradeCost = 0;

        public UnitStat stats = new UnitStat();

        public PerkTowerModifier()
        {
            stats.damageMin = 0;
            stats.cooldown = 0;
            stats.range = 0;
            stats.aoeRadius = 0;
            stats.crit.chance = 0;
            stats.crit.dmgMultiplier = 0;
        }
    }

    public class PerkAbilityModifier
    {
        public int abilityID = -1;

        public float cost = 0;
        public float cooldown = 0;
        public float aoeRadius = 0;

        public AbilityEffect effects = new AbilityEffect();
    }

    public enum _PerkType
    {

        GainLife,
        LifeCap,
        LifeRegen,
        LifeWaveClearedBonus,

        RscCap,
        RscRegen,                       //generate overtime
        RscGain,                        //modifier for any gain (global)
        RscCreepKilledGain,             //modifier for gain when a creep is killed
        RscWaveClearedGain,             //modifier for gain when a wave is cleared
        RscResourceTowerGain,           //modifier for gain from rscTower

        Tower,                          //global for all towers
        TowerSpecific,                  //only for specific tower (that uses the same prefabID)
        Ability,
        AbilitySpecific,

        EnergyRegen,
        EnergyIncreaseCap,
        EnergyCreepKilledBonus,
        EnergyWaveClearedBonus,
    }


    [System.Serializable]
    public class Perk : TDTKItem
    {
        public bool disablePerk = false;
        public _PerkType type;
        public int levelUnlock = 0;              //min level to reach before becoming available (check GameControl.levelID)
        public string desp = "";
        public int level = 0; //Level ò perk
        public List<int> itemIDList = new List<int>(); // List tower, List ability
        public List<PerkLevel> perkLevel = new List<PerkLevel>(); // List level

        public Perk()
        {
        }

        public Perk Clone()
        {
            Perk perk = new Perk();
            perk.ID = ID;
            perk.name = name;
            perk.icon = icon;
            perk.type = type;
            perk.levelUnlock = levelUnlock;

            perk.itemIDList = new List<int>(itemIDList);
            perk.desp = desp;
            perk.perkLevel = new List<PerkLevel>();
            for (int i = 0; i < perkLevel.Count; i++)
            {
                perk.perkLevel.Add(perkLevel[i].Clone());
            }
            return perk;
        }
        public int GetCost()
        {
            if (IsMaxLevel())
            {
                return 0;
            }
            else
            {
                return perkLevel[level + 1].cost;
            }
        }
        public bool IsAvailable()
        {
            if (GamePreferences.userInfo.level < levelUnlock)
                return false;
            return true;
        }
        public bool IsMaxLevel()
        {
            if (level >= (perkLevel.Count - 1))
            {
                level = perkLevel.Count - 1;
                return true;
            }
            return false;
        }
    }
    [System.Serializable]
    public class PerkLevel
    {
        public string desp = "";

        public int cost;
        public float value;
        public float valueAlt;  //act as min/max in some case
        public float valueRsc = 0;
        public UnitStat stats = new UnitStat();
        public AbilityEffect effects = new AbilityEffect();

        //for tower
        public float HP = 0;
        public float buildCost = 0;
        public float upgradeCost = 0;

        //for ability
        public float abCost = 0;
        public float abCooldown = 0;
        public float abAOERadius = 0;

        public PerkLevel()
        {
            stats.damageMin = 0;
            stats.damageMax = 0;
            stats.cooldown = 0;
            stats.range = 0;
            stats.aoeRadius = 0;
        }

        public PerkLevel Clone()
        {
            PerkLevel perk = new PerkLevel();
            perk.cost = cost;
            perk.value = value;
            perk.valueAlt = valueAlt;
            perk.valueRsc = valueRsc;
            perk.stats = stats.Clone();
            perk.effects = effects.Clone();

            perk.HP = HP;
            perk.buildCost = buildCost;
            perk.upgradeCost = upgradeCost;

            perk.abCost = abCost;
            perk.abCooldown = abCooldown;
            perk.abAOERadius = abAOERadius;

            perk.desp = desp;

            return perk;
        }
    }
}
