using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK
{

    public class AbilityManager : MonoBehaviour
    {
        public delegate void ABActivatedHandler(Ability ab);
        public static event ABActivatedHandler onAbilityActivatedE;     //fire when an ability is used

        public delegate void ABTargetSelectModeHandler(bool flag);
        public static event ABTargetSelectModeHandler onTargetSelectModeE;  //fire when enter/exit target selection for ability

        public List<int> unavailableIDList = new List<int>(); //ID list of perk available for this level, modified in editor

        public List<Ability> abilityList = new List<Ability>(); //actual ability list, filled in runtime based on unavailableIDList
        public static List<Ability> GetAbilityList() { return instance.abilityList; }

        public Transform defaultIndicator;      //generic indicator use for ability without any specific indicator

        private bool inTargetSelectMode = false;
        public static bool InTargetSelectMode() { return instance == null ? false : instance.inTargetSelectMode; }

        private bool validTarget = false;   //used for targetSelectMode, indicate when the cursor is in a valid position or on a valid target

        public int selectedAbilityID = -1;
        public Transform currentIndicator;      //active indicator in used

        public bool startWithFullEnergy = true;
        public float emerguRegen = 0;
        public float energyFull = 0;
        public float energy = 0;


        private Transform thisT;
        private static AbilityManager instance;

        public static bool IsOn() { return instance == null ? false : true; }

        void Awake()
        {
            instance = this;
            thisT = transform;
            List<Ability> dbList = AbilityDB.Load();

            abilityList = new List<Ability>();
            for (int i = 0; i < dbList.Count; i++)
            {
                if (!unavailableIDList.Contains(dbList[i].ID))
                {
                    abilityList.Add(dbList[i].Clone());
                }
            }

            for (int i = 0; i < abilityList.Count; i++) abilityList[i].ID = i;
            if (defaultIndicator)
            {
                defaultIndicator = (Transform)Instantiate(defaultIndicator);
                defaultIndicator.parent = thisT;
                defaultIndicator.gameObject.SetActive(false);
            }
        }
        private void Start()
        {
            if (startWithFullEnergy) energy = GetEnergyFull();
        }
        void OnDestroy() { instance = null; }

        // Update is called once per frame
        void Update()
        {
            RegenEnergy();
            SelectAbilityTarget();
        }

        void RegenEnergy()
        {
            if (GameControl.IsGameStarted())
            {
                if (energy < GetEnergyFull())
                {
                    energy += Time.fixedDeltaTime * GetEnergyRate();
                    energy = Mathf.Min(energy, GetEnergyFull());
                }
            }
        }
        //called in every frame, execute if there's an ability is selected and pending target selection
        //use only mouse input atm.
        void SelectAbilityTarget()
        {
            if (selectedAbilityID < 0) return;
            Ability ability = abilityList[selectedAbilityID];

#if (UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8) && !UNITY_EDITOR
            if (Input.touchCount >= 1)
            {

                Vector3 screenPoint = Input.mousePosition;
                screenPoint.z = 11.5f;
                currentIndicator.position = Camera.main.ScreenToWorldPoint(screenPoint);
            }
            else
            {
                ActivateAbility(ability, currentIndicator.position);
                ClearSelectedAbility();
            }
#else
            Vector3 screenPoint = Input.mousePosition;
            screenPoint.z = 11.5f;
            currentIndicator.position = Camera.main.ScreenToWorldPoint(screenPoint);
            if (Input.GetMouseButtonUp(0))
            {
                ActivateAbility(ability, currentIndicator.position);
                ClearSelectedAbility();
            }
#endif
        }


        //called by ability button from UI, select an ability
        public static string SelectAbility(int ID) { return instance._SelectAbility(ID); }
        public string _SelectAbility(int ID)
        {
            Ability ab = abilityList[ID];

            Debug.Log(ab.name);

            string exception = ab.IsAvailable();
            if (exception != "") return exception;

            if (!ab.requireTargetSelection)
                ActivateAbility(ab);        //no target selection required, fire it away
            else
            {
                if (onTargetSelectModeE != null) onTargetSelectModeE(true); //enter target selection phase

                inTargetSelectMode = true;
                validTarget = false;

                selectedAbilityID = ID;

                if (ab.indicator != null) currentIndicator = ab.indicator;
                else
                {
                    currentIndicator = defaultIndicator;
                    if (ab.autoScaleIndicator)
                    {
                        currentIndicator.localScale = new Vector3(ab.GetAOERadius() * 2, 1, ab.GetAOERadius() * 2);
                    }
                }

                currentIndicator.gameObject.SetActive(true);
            }

            return "";
        }
        public static void ClearSelectedAbility() { instance._ClearSelectedAbility(); }
        public void _ClearSelectedAbility()
        {
            currentIndicator.gameObject.SetActive(false);
            selectedAbilityID = -1;
            currentIndicator = null;

            inTargetSelectMode = false;

            if (onTargetSelectModeE != null) onTargetSelectModeE(false);
        }

        //called when an ability is fired, reduce the energy, start the cooldown and what not
        public void ActivateAbility(Ability ab, Vector3 pos = default(Vector3))
        {
            ab.usedCount += 1;
            energy -= ab.GetCost();
            StartCoroutine(ab.CooldownRoutine());

            CastAbility(ab, pos);

            if (onAbilityActivatedE != null) onAbilityActivatedE(ab);
        }

        //called from ActivateAbility, cast the ability, visual effect and actual effect goes here
        public void CastAbility(Ability ab, Vector3 pos, Unit unit = null)
        {
            if (ab.effectObj != null)
            {
                ObjectPoolManager.Spawn(ab.effectObj, pos, Quaternion.identity);
            }

            if (ab.useDefaultEffect)
            {
                StartCoroutine(ApplyAbilityEffect(ab, pos, unit));
            }
        }


        //apply the ability effect, damage, stun, buff and so on 
        IEnumerator ApplyAbilityEffect(Ability ab, Vector3 pos, Unit tgtUnit = null)
        {
            yield return new WaitForSeconds(ab.effectDelay);

            LayerMask mask1 = 1 << LayerManager.LayerTower();
            LayerMask mask2 = 1 << LayerManager.LayerCreep();
            LayerMask mask3 = 1 << LayerManager.LayerCreepF();
            LayerMask mask = mask1 | mask2 | mask3;

            List<Unit> creepList = new List<Unit>();
            List<Unit> towerList = new List<Unit>();

            if (tgtUnit == null)
            {
                float radius = ab.requireTargetSelection ? ab.GetAOERadius() : Mathf.Infinity;
                Collider[] cols = Physics.OverlapSphere(pos, radius, mask);

                if (cols.Length > 0)
                {
                    for (int i = 0; i < cols.Length; i++)
                    {
                        Unit unit = cols[i].gameObject.GetComponent<Unit>();
                        if (unit.unitC != null) creepList.Add(unit.unitC);
                        if (unit.unitT != null) towerList.Add(unit.unitT);
                    }
                }
            }
            else
            {
                creepList.Add(tgtUnit);
                towerList.Add(tgtUnit);
            }

            AbilityEffect eff = ab.GetActiveEffect();

            for (int n = 0; n < creepList.Count; n++)
            {
                if (eff.damageMax > 0)
                {
                    creepList[n].ApplyDamage(Random.Range(eff.damageMin, eff.damageMax));
                }
                else if (eff.stunChance > 0 && eff.duration > 0)
                {
                    if (Random.Range(0f, 1f) < eff.stunChance) creepList[n].ApplyStun(eff.duration);
                }
                else if (eff.slow.IsValid())
                {
                    creepList[n].ApplySlow(eff.slow);
                }
                else if (eff.dot.GetTotalDamage() > 0)
                {
                    creepList[n].ApplyDot(eff.dot);
                }
            }
            for (int n = 0; n < towerList.Count; n++)
            {
                if (eff.duration > 0)
                {
                    if (eff.damageBuff > 0)
                    {
                        towerList[n].ABBuffDamage(eff.damageBuff, eff.duration);
                    }
                    else if (eff.rangeBuff > 0)
                    {
                        towerList[n].ABBuffRange(eff.rangeBuff, eff.duration);
                    }
                    else if (eff.cooldownBuff > 0)
                    {
                        towerList[n].ABBuffCooldown(eff.cooldownBuff, eff.duration);
                    }
                }
                else if (eff.HPGainMax > 0)
                {
                    towerList[n].RestoreHP(Random.Range(eff.HPGainMin, eff.HPGainMax));
                }
            }

        }

        public static void GainEnergy(int value) { if (instance != null) instance._GainEnergy(value); }
        public void _GainEnergy(int value)
        {
            energy += value;
            energy = Mathf.Min(energy, GetEnergyFull());
        }

        public static float GetAbilityCurrentCD(int ID) { return instance.abilityList[ID].currentCD; }
        public static float GetEnergyFull() { return instance.energyFull + PerkManager.GetEnergyCapModifier(); }
        public static float GetEnergy() { return instance.energy; }
        private float GetEnergyRate() { return emerguRegen + PerkManager.GetEnergyRegenModifier(); }
    }
}