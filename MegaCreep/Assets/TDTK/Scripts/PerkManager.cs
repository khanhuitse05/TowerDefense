using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK
{

    public class PerkManager : MonoBehaviour
    {

        public static PerkManager instance;
        public List<Perk> perkList = new List<Perk>();          //actual perk list, filled in runtime based on unavailableIDList

        private bool init = false;
        public void Init()
        {
            if (init) return;
            init = true;
            instance = this;
            perkList = GamePreferences.perkList;

            globalTowerModifier = new PerkTowerModifier();
            globalAbilityModifier = new PerkAbilityModifier();

            emptyTowerModifier = new PerkTowerModifier();
            emptyAbilityModifier = new PerkAbilityModifier();

            rscRegen = 0;
            rscGain = 0;
            rscCreepKilledGain = 0;
            rscWaveClearedGain = 0;
            rscRscTowerGain = 0;

            for (int i = 0; i < perkList.Count; i++)
            {
                _PurchasePerk(perkList[i]);
            }

        }

        public static Perk GetPerk(int perkID) { return instance._GetPerk(perkID); }
        public Perk _GetPerk(int perkID)
        {
            for (int i = 0; i < perkList.Count; i++) { if (perkList[i].ID == perkID) return perkList[i]; }
            return null;
        }

        public static bool PurchasePerk(int perkID) { return instance._PurchasePerk(perkID); }
        public bool _PurchasePerk(int perkID)
        {
            for (int i = 0; i < perkList.Count; i++)
            {
                if (perkList[i].ID == perkID)
                    return instance._PurchasePerk(perkList[i]);
            }
            // "PerkID doesn't correspond to any perk in the list"
            return false;
        }

        public static bool PurchasePerk(Perk perk) { return instance._PurchasePerk(perk); }

        public bool _PurchasePerk(Perk perk)
        {
            if (perk.IsAvailable() == false)
            {
                return false;
            }

            if (perk.type == _PerkType.GainLife) { GameControl.GainLife((int)Random.Range(perk.perkLevel[perk.level].value, perk.perkLevel[perk.level].valueAlt)); }
            else if (perk.type == _PerkType.LifeCap) { lifeCap += (int)perk.perkLevel[perk.level].value; GameControl.GainLife(0); }
            else if (perk.type == _PerkType.LifeRegen) { lifeRegen += perk.perkLevel[perk.level].value; }
            else if (perk.type == _PerkType.LifeWaveClearedBonus) { lifeWaveClearedBonus += (int)perk.perkLevel[perk.level].value; }

            else if (perk.type == _PerkType.RscCap)
            {
                int _value = (int)perk.perkLevel[perk.level].valueRsc;
                ResourceManager.GainResource(_value, 0, false); //dont pass multiplier and dont use multiplier
            }
            else if (perk.type == _PerkType.RscRegen)
            {
                rscRegen += perk.perkLevel[perk.level].valueRsc;
            }
            else if (perk.type == _PerkType.RscGain)
            {
                rscGain += perk.perkLevel[perk.level].valueRsc;
            }
            else if (perk.type == _PerkType.RscCreepKilledGain)
            {
                rscCreepKilledGain += perk.perkLevel[perk.level].valueRsc;
            }
            else if (perk.type == _PerkType.RscWaveClearedGain)
            {
                rscWaveClearedGain += perk.perkLevel[perk.level].valueRsc;
            }
            else if (perk.type == _PerkType.RscResourceTowerGain)
            {
                rscRscTowerGain += perk.perkLevel[perk.level].valueRsc;
            }

            else if (perk.type == _PerkType.Tower) { ModifyTowerModifier(globalTowerModifier, perk); }
            else if (perk.type == _PerkType.TowerSpecific)
            {
                for (int i = 0; i < perk.itemIDList.Count; i++)
                {
                    int ID = TowerModifierExist(perk.itemIDList[i]);
                    if (ID == -1)
                    {
                        PerkTowerModifier towerModifier = new PerkTowerModifier();
                        towerModifier.prefabID = perk.itemIDList[i];
                        towerModifierList.Add(towerModifier);
                        ID = towerModifierList.Count - 1;
                    }
                    ModifyTowerModifierInList(ID, perk);
                }
            }
            else if (perk.type == _PerkType.Ability)
            {
                ModifyAbilityModifier(globalAbilityModifier, perk);
            }
            else if (perk.type == _PerkType.AbilitySpecific)
            {
                for (int i = 0; i < perk.itemIDList.Count; i++)
                {
                    int ID = AbilityModifierExist(perk.itemIDList[i]);
                    if (ID == -1)
                    {
                        PerkAbilityModifier abilityModifier = new PerkAbilityModifier();
                        abilityModifier.abilityID = perk.itemIDList[i];
                        abilityModifierList.Add(abilityModifier);
                        ID = abilityModifierList.Count - 1;
                    }
                    ModifyAbilityModifierInList(ID, perk);
                }
            }

            else if (perk.type == _PerkType.EnergyRegen) { energyRegen += perk.perkLevel[perk.level].value; }
            else if (perk.type == _PerkType.EnergyIncreaseCap) { energyCap += perk.perkLevel[perk.level].value; }
            else if (perk.type == _PerkType.EnergyCreepKilledBonus) { energyCreepKilledBonus += perk.perkLevel[perk.level].value; }
            else if (perk.type == _PerkType.EnergyWaveClearedBonus) { energyWaveClearedBonus += perk.perkLevel[perk.level].value; }

            return true;
        }

        private int TowerModifierExist(int prefabID)
        {
            for (int i = 0; i < towerModifierList.Count; i++) { if (towerModifierList[i].prefabID == prefabID) return i; }
            return -1;
        }
        private void ModifyTowerModifierInList(int ID, Perk perk) { ModifyTowerModifier(towerModifierList[ID], perk); }
        private void ModifyTowerModifier(PerkTowerModifier towerModifier, Perk perk)
        {
            towerModifier.HP += perk.perkLevel[perk.level].HP;
            towerModifier.buildCost += perk.perkLevel[perk.level].buildCost;
            towerModifier.upgradeCost += perk.perkLevel[perk.level].upgradeCost;
            ModifyUnitStats(towerModifier.stats, perk.perkLevel[perk.level].stats);
        }



        private int AbilityModifierExist(int abilityID)
        {
            for (int i = 0; i < abilityModifierList.Count; i++) { if (abilityModifierList[i].abilityID == abilityID) return i; }
            return -1;
        }
        private void ModifyAbilityModifierInList(int ID, Perk perk) { ModifyAbilityModifier(abilityModifierList[ID], perk); }
        private void ModifyAbilityModifier(PerkAbilityModifier abilityModifier, Perk perk)
        {
            abilityModifier.cost = perk.perkLevel[perk.level].abCost;
            abilityModifier.cooldown = perk.perkLevel[perk.level].abCooldown;
            abilityModifier.aoeRadius = perk.perkLevel[perk.level].abAOERadius;

            abilityModifier.effects.damageMin += perk.perkLevel[perk.level].effects.damageMin;
            abilityModifier.effects.damageMax += perk.perkLevel[perk.level].effects.damageMax;
            abilityModifier.effects.stunChance += perk.perkLevel[perk.level].effects.stunChance;

            abilityModifier.effects.slow.duration += perk.perkLevel[perk.level].effects.duration;
            abilityModifier.effects.slow.slowMultiplier += perk.perkLevel[perk.level].effects.slow.slowMultiplier;

            abilityModifier.effects.dot.duration += perk.perkLevel[perk.level].effects.duration;
            abilityModifier.effects.dot.interval += perk.perkLevel[perk.level].effects.dot.interval;
            abilityModifier.effects.dot.value += perk.perkLevel[perk.level].effects.dot.value;

            abilityModifier.effects.damageBuff += perk.perkLevel[perk.level].effects.damageBuff;
            abilityModifier.effects.rangeBuff += perk.perkLevel[perk.level].effects.rangeBuff;
            abilityModifier.effects.cooldownBuff += perk.perkLevel[perk.level].effects.cooldownBuff;
            abilityModifier.effects.HPGainMin += perk.perkLevel[perk.level].effects.HPGainMin;
            abilityModifier.effects.HPGainMax += perk.perkLevel[perk.level].effects.HPGainMax;
        }


        private void ModifyUnitStats(UnitStat tgtStats, UnitStat srcStats)
        {
            tgtStats.damageMin += srcStats.damageMin;
            tgtStats.cooldown += srcStats.cooldown;
            tgtStats.range += srcStats.range;
            tgtStats.aoeRadius += srcStats.aoeRadius;

            tgtStats.crit.chance += srcStats.crit.chance;
            tgtStats.crit.dmgMultiplier += srcStats.crit.dmgMultiplier;

            tgtStats.stun.chance += srcStats.stun.chance;
            tgtStats.stun.duration += srcStats.stun.duration;

            tgtStats.slow.duration += srcStats.slow.duration;
            tgtStats.slow.slowMultiplier += srcStats.slow.slowMultiplier;

            tgtStats.dot.duration += srcStats.dot.duration;
            tgtStats.dot.interval += srcStats.dot.interval;
            tgtStats.dot.value += srcStats.dot.value;
        }


        //************************************************************************************************************************************
        //modifiers goes here		

        public int lifeCap = 0;
        public float lifeRegen = 0;
        public int lifeWaveClearedBonus = 0;    //bonus modifier when a wave is cleared

        public static int GetLifeCapModifier() { return instance == null ? 0 : instance.lifeCap; }
        public static float GetLifeRegenModifier() { return instance == null ? 0 : instance.lifeRegen; }
        public static int GetLifeWaveClearedModifier() { return instance == null ? 0 : instance.lifeWaveClearedBonus; }


        public float rscRegen = 0;
        public float rscGain = 0;
        public float rscCreepKilledGain = 0;
        public float rscWaveClearedGain = 0;
        public float rscRscTowerGain = 0;

        public static float GetRscRegen() { return instance == null ? 0 : instance.rscRegen; }
        public static float GetRscGain() { return instance == null ? 0 : instance.rscGain; }
        public static float GetRscCreepKilled() { return instance == null ? 0 : instance.rscCreepKilledGain; }
        public static float GetRscWaveKilled() { return instance == null ? 0 : instance.rscWaveClearedGain; }
        public static float GetRscTowerGain() { return instance == null ? 0 : instance.rscRscTowerGain; }



        public float energyRegen = 0;
        public float energyCap = 0;
        public float energyCreepKilledBonus = 0;        //bonus modifier when a creep is killed
        public float energyWaveClearedBonus = 0;    //bonus modifier when a wave is cleared

        public static float GetEnergyRegenModifier() { return instance == null ? 0 : instance.energyRegen; }
        public static float GetEnergyCapModifier() { return instance == null ? 0 : instance.energyCap; }
        public static float GetEnergyCreepKilledModifier() { return instance == null ? 0 : instance.energyCreepKilledBonus; }
        public static float GetEnergyWaveClearedModifier() { return instance == null ? 0 : instance.energyWaveClearedBonus; }

        public PerkTowerModifier emptyTowerModifier;
        public PerkTowerModifier globalTowerModifier;
        public List<PerkTowerModifier> towerModifierList = new List<PerkTowerModifier>();

        public static PerkTowerModifier GetTowerModifier(int prefabID)
        {
            for (int i = 0; i < instance.towerModifierList.Count; i++)
            {
                if (instance.towerModifierList[i].prefabID == prefabID)
                    return instance.towerModifierList[i];
            }
            return instance.emptyTowerModifier;
        }


        public static float GetTowerHP(int prefabID)
        {
            if (instance == null) return 0;
            return instance.globalTowerModifier.HP + GetTowerModifier(prefabID).HP;
        }
        public static float GetTowerBuildCost(int prefabID)
        {
            if (instance == null) return 0;
            return instance.globalTowerModifier.buildCost + GetTowerModifier(prefabID).buildCost;
        }
        public static float GetTowerUpgradeCost(int prefabID)
        {
            if (instance == null) return 0;
            return instance.globalTowerModifier.upgradeCost + GetTowerModifier(prefabID).upgradeCost;
        }

        public static float GetTowerDamage(int prefabID)
        {
            if (instance == null) return 0;
            return (instance.globalTowerModifier.stats.damageMin + GetTowerModifier(prefabID).stats.damageMin);
        }
        public static float GetTowerCD(int prefabID)
        {
            if (instance == null) return 0;
            return (instance.globalTowerModifier.stats.cooldown + GetTowerModifier(prefabID).stats.cooldown);
        }
        public static float GetTowerRange(int prefabID)
        {
            if (instance == null) return 0;
            return (instance.globalTowerModifier.stats.range + GetTowerModifier(prefabID).stats.range);
        }
        public static float GetTowerAOERadius(int prefabID)
        {
            if (instance == null) return 0;
            return (instance.globalTowerModifier.stats.aoeRadius + GetTowerModifier(prefabID).stats.aoeRadius);
        }
        public static float GetTowerCritChance(int prefabID)
        {
            if (instance == null) return 0;
            return instance.globalTowerModifier.stats.crit.chance + GetTowerModifier(prefabID).stats.crit.chance;
        }
        public static float GetTowerCritMultiplier(int prefabID)
        {
            if (instance == null) return 0;
            return (instance.globalTowerModifier.stats.crit.dmgMultiplier + GetTowerModifier(prefabID).stats.crit.dmgMultiplier);
        }

        public static Stun GetTowerStunMultiplier(int prefabID)
        {
            if (instance == null) return new Stun(0, 0);
            Stun stunG = instance.globalTowerModifier.stats.stun;
            Stun stunT = GetTowerModifier(prefabID).stats.stun;
            return new Stun(stunG.chance + stunT.chance, stunG.duration + stunT.duration);
        }
        public static Slow GetTowerSlowMultiplier(int prefabID)
        {
            if (instance == null) return new Slow(0, 0);
            Slow slowG = instance.globalTowerModifier.stats.slow;
            Slow slowT = GetTowerModifier(prefabID).stats.slow;
            return new Slow(slowG.slowMultiplier + slowT.slowMultiplier, slowG.duration + slowT.duration);
        }
        public static Dot GetTowerDotMultiplier(int prefabID)
        {
            if (instance == null) return new Dot(0, 0, 0);
            Dot dotG = instance.globalTowerModifier.stats.dot;
            Dot dotT = GetTowerModifier(prefabID).stats.dot;
            return new Dot(dotG.duration + dotT.duration, dotG.interval + dotT.interval, dotG.value + dotT.value);
        }
        
        //shared among tower, ability . 0-tower, 2-ability
        public static Stun ModifyStunWithPerkBonus(Stun stun, int prefabID, int type = 0)
        {
            Stun stunMod = new Stun();
            if (type == 0) stunMod = GetTowerStunMultiplier(prefabID);
            stun.chance *= (1 + stunMod.chance);
            stun.duration *= (1 + stunMod.duration);
            return stun;
        }
        public static Slow ModifySlowWithPerkBonus(Slow slow, int prefabID, int type = 0)
        {
            Slow slowMod = new Slow();
            if (type == 0) slowMod = GetTowerSlowMultiplier(prefabID);
            else if (type == 2)
                slowMod = GetAbilitySlowMultiplier(prefabID);
            slowMod.slowMultiplier = slow.slowMultiplier * (1 - slowMod.slowMultiplier);
            slowMod.duration = slow.duration * (1 + slowMod.duration);
            return slowMod;
        }
        public static Dot ModifyDotWithPerkBonus(Dot dot, int prefabID, int type = 0)
        {
            Dot dotMod = new Dot();
            if (type == 0) dotMod = GetTowerDotMultiplier(prefabID);
            else if (type == 2) dotMod = GetAbilityDotMultiplier(prefabID);
            dot.duration *= (1 + dotMod.duration);
            //dot.interval*=(1+dotMod.interval);
            dot.value *= (1 + dotMod.value);
            return dot;
        }
       
        public PerkAbilityModifier emptyAbilityModifier;
        public PerkAbilityModifier globalAbilityModifier;
        public List<PerkAbilityModifier> abilityModifierList = new List<PerkAbilityModifier>();

        public static PerkAbilityModifier GetAbilityModifier(int prefabID)
        {
            for (int i = 0; i < instance.abilityModifierList.Count; i++)
            {
                if (instance.abilityModifierList[i].abilityID == prefabID) return instance.abilityModifierList[i];
            }
            return instance.emptyAbilityModifier;
        }

        public static float GetAbilityCost(int abilityID)
        {
            if (instance == null) return 0;
            return instance.globalAbilityModifier.cost + GetAbilityModifier(abilityID).cost;
        }
        public static float GetAbilityCooldown(int abilityID)
        {
            if (instance == null) return 0;
            return instance.globalAbilityModifier.cooldown + GetAbilityModifier(abilityID).cooldown;
        }
        public static float GetAbilityAOERadius(int abilityID)
        {
            if (instance == null) return 0;
            return instance.globalAbilityModifier.aoeRadius + GetAbilityModifier(abilityID).aoeRadius;
        }

        public static float GetAbilityDuration(int abilityID)
        {
            if (instance == null) return 0;
            return instance.globalAbilityModifier.effects.duration + GetAbilityModifier(abilityID).effects.duration;
        }
        public static float GetAbilityDamage(int abilityID)
        {
            if (instance == null) return 0;
            return instance.globalAbilityModifier.effects.damageMin + GetAbilityModifier(abilityID).effects.damageMin;
        }
        public static float GetAbilityStunChance(int abilityID)
        {
            if (instance == null) return 0;
            return instance.globalAbilityModifier.effects.stunChance + GetAbilityModifier(abilityID).effects.stunChance;
        }

        public static float GetAbilityDamageBuff(int abilityID)
        {
            if (instance == null) return 0;
            return instance.globalAbilityModifier.effects.damageBuff + GetAbilityModifier(abilityID).effects.damageBuff;
        }
        public static float GetAbilityRangeBuff(int abilityID)
        {
            if (instance == null) return 0;
            return instance.globalAbilityModifier.effects.rangeBuff + GetAbilityModifier(abilityID).effects.rangeBuff;
        }
        public static float GetAbilityCooldownBuff(int abilityID)
        {
            if (instance == null) return 0;
            return instance.globalAbilityModifier.effects.cooldownBuff + GetAbilityModifier(abilityID).effects.cooldownBuff;
        }
        public static float GetAbilityHPGain(int abilityID)
        {
            if (instance == null) return 0;
            return instance.globalAbilityModifier.effects.HPGainMin + GetAbilityModifier(abilityID).effects.HPGainMin;
        }

        public static Slow GetAbilitySlowMultiplier(int prefabID)
        {
            if (instance == null) return new Slow(0, 0);
            Slow slowG = instance.globalAbilityModifier.effects.slow;
            Slow slowT = GetAbilityModifier(prefabID).effects.slow;
            return new Slow(slowG.slowMultiplier + slowT.slowMultiplier, slowG.duration + slowT.duration);
        }
        public static Dot GetAbilityDotMultiplier(int prefabID)
        {
            if (instance == null) return new Dot(0, 0, 0);
            Dot dotG = instance.globalAbilityModifier.effects.dot;
            Dot dotT = GetAbilityModifier(prefabID).effects.dot;
            return new Dot(dotG.duration + dotT.duration, dotG.interval + dotT.interval, dotG.value + dotT.value);
        }

    }
}