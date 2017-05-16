using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TDTK
{
    public class Unit : MonoBehaviour
    {
        //call when unit HP value is changed, for displaying unit overlay	
        public delegate void DamagedHandler(Unit unit);
        public static event DamagedHandler onDamagedE;

        public delegate void DestroyedHandler(Unit unit);
        public static event DestroyedHandler onDestroyedE;

        public int prefabID = -1;
        public int instanceID = -1;

        public string unitName = "unit";
        public Sprite iconSprite;
        public string desp = "";

        #region SubClass
        public enum _UnitSubClass { Creep, Tower };
        public _UnitSubClass subClass = _UnitSubClass.Creep;
        [HideInInspector] public UnitCreep unitC;
        [HideInInspector] public UnitTower unitT;
        //Call by inherited class UnitCreep, caching inherited UnitCreep instance to this instance
        public void SetSubClass(UnitCreep unit)
        {
            unitC = unit;
            subClass = _UnitSubClass.Creep;
            if (!unitC.flying) gameObject.layer = LayerManager.LayerCreep();
            else gameObject.layer = LayerManager.LayerCreepF();
        }
        //Call by inherited class UnitTower, caching inherited UnitTower instance to this instance
        public void SetSubClass(UnitTower unit)
        {
            unitT = unit;
            subClass = _UnitSubClass.Tower;
            gameObject.layer = LayerManager.LayerTower();
        }
        public bool IsTower() { return subClass == _UnitSubClass.Tower ? true : false; }
        public bool IsCreep() { return subClass == _UnitSubClass.Creep ? true : false; }
        public UnitTower GetUnitTower() { return unitT; }
        public UnitCreep GetUnitCreep() { return unitC; }
        #endregion

        public float defaultHP = 10;
        protected float overrideHP = 0;
        public float fullHP = 10;
        public float HP = 10;

        public int damageType = 0;
        public int armorType = 0;

        public int currentActiveStat = 0;
        public List<UnitStat> stats = new List<UnitStat>();


        public bool dead = false;
        public bool stunned = false;
        private float stunDuration = 0;

        public float slowMultiplier = 1;
        public List<Slow> slowEffectList = new List<Slow>();

        public List<Buff> buffEffect = new List<Buff>();

        public List<Transform> shootPoints = new List<Transform>();
        public float delayBetweenShootPoint = 0;
        public Transform targetPoint;
        public float hitThreshold = 0.25f;      //hit distance from the targetPoint for the shootObj

        [HideInInspector] public GameObject thisObj;
        [HideInInspector] public Transform thisT;

        public virtual void Awake()
        {
            thisObj = gameObject;
            thisT = transform;

            if (shootPoints.Count == 0)
                shootPoints.Add(thisT);

            ResetBuff();
            for (int i = 0; i < stats.Count; i++)
            {
                if (stats[i].shootObject != null)
                {
                    stats[i].shootObjectT = stats[i].shootObject.transform;
                }
            }

            if (deadEffectObj != null) ObjectPoolManager.New(deadEffectObj, 3);
        }

        public void Init()
        {
            thisObj = gameObject;
            thisT = transform;
            dead = false;
            stunned = false;
            fullHP = GetFullHP();
            HP = fullHP;
            ResetBuff();
        }
        
        public virtual void Start()
        {
        }

        public virtual void Update()
        {
            TurretUpdate();
        }

        Vector3 _targetPos;
        Quaternion _wantedRot;
        protected void TurretUpdate()
        {
            if (target != null && !IsInConstruction() && !stunned)
            {
                if (turretObject != null)
                {
                    {
                        _targetPos = target.GetTargetT().position;
                        _targetPos.y = turretObject.position.y;
                        _wantedRot = Quaternion.LookRotation(_targetPos - turretObject.position);
                        turretObject.rotation = Quaternion.Slerp(turretObject.rotation, _wantedRot, turretRotateSpeed * Time.deltaTime);
                        targetInLOS = (Quaternion.Angle(turretObject.rotation, _wantedRot) < aimTolerance);
                    }
                }
                else
                    targetInLOS = true;
            }
        }

        public Transform turretObject;

        protected float turretRotateSpeed = 12;
        private float aimTolerance = 5;
        private bool targetInLOS = false;

        protected Unit target;

        public enum _TargetPriority { Nearest, Weakest, Toughest, First, Random };
        public _TargetPriority targetPriority = _TargetPriority.Random;

        protected LayerMask maskTarget = 0;
        public LayerMask GetTargetMask() { return maskTarget; }

        public IEnumerator ScanForTargetRoutine()
        {
            while (true)
            {
                ScanForTarget();
                yield return new WaitForSeconds(0.15f);
            }
        }

        Collider[] _colsOverlap;
        List<Unit> _tgtList = new List<Unit>();
        void ScanForTarget()
        {
            if (dead || IsInConstruction() || stunned) return;

            if (target == null)
            {
                _colsOverlap = Physics.OverlapSphere(thisT.position, GetRange(), maskTarget);
                if (_colsOverlap.Length > 0)
                {
                    _tgtList = new List<Unit>();
                    Unit _unit;
                    for (int i = 0; i < _colsOverlap.Length; i++)
                    {
                        _unit = _colsOverlap[i].gameObject.GetComponent<Unit>();
                        if (!_unit.dead)
                            _tgtList.Add(_unit);
                    }
                    if (_tgtList.Count > 0)
                    {
                        if (targetPriority == _TargetPriority.Random)
                        {
                            target = _tgtList[0];
                        }
                        else if (targetPriority == _TargetPriority.Nearest)
                        {
                            float nearest = Mathf.Infinity;
                            for (int i = 0; i < _tgtList.Count; i++)
                            {
                                float dist = Vector3.Distance(thisT.position, _tgtList[i].thisT.position);
                                if (dist < nearest)
                                {
                                    nearest = dist;
                                    target = _tgtList[i];
                                }
                            }
                        }
                        else if (targetPriority == _TargetPriority.Weakest)
                        {
                            float lowest = Mathf.Infinity;
                            for (int i = 0; i < _tgtList.Count; i++)
                            {
                                if (_tgtList[i].HP < lowest)
                                {
                                    lowest = _tgtList[i].HP;
                                    target = _tgtList[i];
                                }
                            }
                        }
                        else if (targetPriority == _TargetPriority.Toughest)
                        {
                            float highest = 0;
                            for (int i = 0; i < _tgtList.Count; i++)
                            {
                                if (_tgtList[i].HP > highest)
                                {
                                    highest = _tgtList[i].HP;
                                    target = _tgtList[i];
                                }
                            }
                        }
                        else if (targetPriority == _TargetPriority.First)
                        {
                            target = _tgtList[Random.Range(0, _tgtList.Count - 1)];
                            float lowest = Mathf.Infinity;
                            for (int i = 0; i < _tgtList.Count; i++)
                            {
                                if (_tgtList[i].GetDistFromDestination() < lowest)
                                {
                                    lowest = _tgtList[i].GetDistFromDestination();
                                    target = _tgtList[i];
                                }
                            }
                        }
                    }
                }
                targetInLOS = false;
            }
            else
            {
                if (target.dead )
                {
                    target = null;
                }
                else
                {
                    float dist = Vector3.Distance(thisT.position, target.thisT.position);
                    if (dist > GetRange())
                    {
                        target = null;
                    }
                }
            }

        }

        public delegate float PlayShootAnimation();
        public PlayShootAnimation playShootAnimation;

        private bool turretOnCooldown = false;
        Transform _shootTf;
        ShootObject _shootObj;
        public IEnumerator TurretRoutine()
        {
            for (int i = 0; i < shootPoints.Count; i++)
            {
                if (shootPoints[i] == null) { shootPoints.RemoveAt(i); i -= 1; }
            }
            if (shootPoints.Count == 0)
            {
                shootPoints.Add(thisT);
            }
            
            yield return null;

            while (true)
            {
                while (target == null || stunned || IsInConstruction() || !targetInLOS)
                    yield return null;
                turretOnCooldown = true;

                float animationDelay = 0;
                if (playShootAnimation != null) animationDelay = playShootAnimation();
                if (animationDelay > 0) yield return new WaitForSeconds(animationDelay);

                AttackInstance attInstance = new AttackInstance();
                attInstance.srcUnit = this;
                attInstance.tgtUnit = target;
                attInstance.Process();

                for (int i = 0; i < shootPoints.Count; i++)
                {
                    _shootTf = (Transform)Instantiate(GetShootObjectT(), shootPoints[i].position, shootPoints[i].rotation);
                    _shootObj = _shootTf.GetComponent<ShootObject>();
                    _shootObj.Shoot(attInstance, shootPoints[i]);

                    if (delayBetweenShootPoint > 0) yield return new WaitForSeconds(delayBetweenShootPoint);
                }

                yield return new WaitForSeconds(GetCooldown() - animationDelay - shootPoints.Count * delayBetweenShootPoint);
                turretOnCooldown = false;
            }
        }

        public void ApplyEffect(AttackInstance attInstance)
        {
            if (dead) return;

            HP -= attInstance.damage;
            new TextOverlay(thisT.position, attInstance.damage.ToString("f0"), new Color(1f, 1f, 1f, 1f));

            if (onDamagedE != null) onDamagedE(this);
            if (HP <= 0)
            {
                Dead();
                return;
            }

            if (attInstance.stunned) ApplyStun(attInstance.stun.duration);
            if (attInstance.slowed) ApplySlow(attInstance.slow);
            if (attInstance.dotted) ApplyDot(attInstance.dot);
        }

        public void ApplyStun(float duration)
        {
            stunDuration = duration;
            if (!stunned) StartCoroutine(StunRoutine());
        }
        IEnumerator StunRoutine()
        {
            stunned = true;
            while (stunDuration > 0)
            {
                stunDuration -= Time.deltaTime;
                yield return null;
            }
            stunned = false;
        }

        public void ApplySlow(Slow slow) { StartCoroutine(SlowRoutine(slow)); }
        IEnumerator SlowRoutine(Slow slow)
        {
            slowEffectList.Add(slow);
            ResetSlowMultiplier();
            yield return new WaitForSeconds(slow.duration);
            slowEffectList.Remove(slow);
            ResetSlowMultiplier();
        }

        void ResetSlowMultiplier()
        {
            if (slowEffectList.Count == 0)
            {
                slowMultiplier = 1;
                return;
            }

            for (int i = 0; i < slowEffectList.Count; i++)
            {
                if (slowEffectList[i].slowMultiplier < slowMultiplier)
                {
                    slowMultiplier = slowEffectList[i].slowMultiplier;
                }
            }

            slowMultiplier = Mathf.Max(0, slowMultiplier);
        }


        public void ApplyDot(Dot dot) { StartCoroutine(DotRoutine(dot)); }
        IEnumerator DotRoutine(Dot dot)
        {
            int count = (int)Mathf.Floor(dot.duration / dot.interval);
            for (int i = 0; i < count; i++)
            {
                yield return new WaitForSeconds(dot.interval);
                if (dead) break;
                DamageHP(dot.value);
                if (HP <= 0) { Dead(); break; }
            }
        }


        //for ability and what not
        public void ApplyDamage(float dmg)
        {
            DamageHP(dmg);
            if (HP <= 0) Dead();
        }

        //called when unit take damage
        void DamageHP(float dmg)
        {
            HP -= dmg;
            new TextOverlay(thisT.position, dmg.ToString("f0"), new Color(1f, 1f, 1f, 1f));
            if (onDamagedE != null) onDamagedE(this);
        }

        public void RestoreHP(float value)
        {
            new TextOverlay(thisT.position, value.ToString("f0"), new Color(0f, 1f, .4f, 1f));
            HP = Mathf.Clamp(HP + value, 0, fullHP);
        }

        public List<Unit> buffedUnit = new List<Unit>();
        private bool supportRoutineRunning = false;
        public IEnumerator SupportRoutine()
        {
            supportRoutineRunning = true;

            LayerMask maskTarget = 0;
            if (subClass == _UnitSubClass.Tower)
            {
                maskTarget = 1 << LayerManager.LayerTower();
            }
            else if (subClass == _UnitSubClass.Creep)
            {
                LayerMask mask1 = 1 << LayerManager.LayerCreep();
                LayerMask mask2 = 1 << LayerManager.LayerCreepF();
                maskTarget = mask1 | mask2;
            }

            while (true)
            {
                yield return new WaitForSeconds(0.1f);

                if (!dead)
                {
                    List<Unit> tgtList = new List<Unit>();
                    Collider[] cols = Physics.OverlapSphere(thisT.position, GetRange(), maskTarget);
                    if (cols.Length > 0)
                    {
                        for (int i = 0; i < cols.Length; i++)
                        {
                            Unit unit = cols[i].gameObject.GetComponent<Unit>();
                            if (!unit.dead) tgtList.Add(unit);
                        }
                    }

                    for (int i = 0; i < buffedUnit.Count; i++)
                    {
                        Unit unit = buffedUnit[i];
                        if (unit == null || unit.dead)
                        {
                            buffedUnit.RemoveAt(i); i -= 1;
                        }
                        else if (!tgtList.Contains(unit))
                        {
                            unit.UnBuff(GetBuff());
                            buffedUnit.RemoveAt(i); i -= 1;
                        }
                    }

                    for (int i = 0; i < tgtList.Count; i++)
                    {
                        Unit unit = tgtList[i];
                        if (!buffedUnit.Contains(unit))
                        {
                            unit.Buff(GetBuff());
                            buffedUnit.Add(unit);
                        }
                    }
                }
            }
        }

        public void UnbuffAll()
        {
            for (int i = 0; i < buffedUnit.Count; i++)
            {
                buffedUnit[i].UnBuff(GetBuff());
            }
        }

        public List<Buff> activeBuffList = new List<Buff>();
        public void Buff(Buff buff)
        {
            if (activeBuffList.Contains(buff)) return;

            activeBuffList.Add(buff);
            UpdateBuffStat();
        }
        public void UnBuff(Buff buff)
        {
            if (!activeBuffList.Contains(buff)) return;

            activeBuffList.Remove(buff);
            UpdateBuffStat();
        }

        public float damageBuffMul = 0f;
        public float cooldownBuffMul = 0f;
        public float rangeBuffMul = 0f;
        public float criticalBuffMod = 0.1f;

        void UpdateBuffStat()
        {
            for (int i = 0; i < activeBuffList.Count; i++)
            {
                Buff buff = activeBuffList[i];
                if (damageBuffMul < buff.damageBuff) damageBuffMul = buff.damageBuff;
                if (cooldownBuffMul > buff.cooldownBuff) cooldownBuffMul = buff.cooldownBuff;
                if (rangeBuffMul < buff.rangeBuff) rangeBuffMul = buff.rangeBuff;
                if (criticalBuffMod < buff.criticalBuff) criticalBuffMod = buff.criticalBuff;
            }
        }
        void ResetBuff()
        {
            activeBuffList = new List<Buff>();
            damageBuffMul = 0.0f;
            cooldownBuffMul = 0.0f;
            rangeBuffMul = 0.0f;
            criticalBuffMod = 0f;
        }


        public GameObject deadEffectObj;
        public void Dead()
        {
            dead = true;

            float delay = 0;

            if (deadEffectObj != null) ObjectPoolManager.Spawn(deadEffectObj, targetPoint.position, thisT.rotation);

            if (unitC != null) delay = unitC.CreepDestroyed();
            if (unitT != null) unitT.Destroy();

            if (supportRoutineRunning) ResetBuff();

            if (onDestroyedE != null) onDestroyedE(this);

            StartCoroutine(_Dead(delay));
        }

        public IEnumerator _Dead(float delay)
        {
            yield return new WaitForSeconds(delay);
            ObjectPoolManager.Unspawn(thisObj);
        }

        public Transform GetTargetT()
        {
            return targetPoint != null ? targetPoint : thisT;
        }

        private float GetPerkMulHP() { return IsTower() ? PerkManager.GetTowerHP(unitT.prefabID) : 0; }
        private float GetPerkMulDamage() { return IsTower() ? PerkManager.GetTowerDamage(unitT.prefabID) : 0; }
        private float GetPerkMulCooldown() { return IsTower() ? PerkManager.GetTowerCD(unitT.prefabID) : 0; }
        private float GetPerkMulRange() { return IsTower() ? PerkManager.GetTowerRange(unitT.prefabID) : 0; }
        private float GetPerkMulAOERadius() { return IsTower() ? PerkManager.GetTowerAOERadius(unitT.prefabID) : 0; }
        private float GetPerkModCritChance() { return IsTower() ? PerkManager.GetTowerCritChance(unitT.prefabID) : 0; }
        private float GetPerkModCritMul() { return IsTower() ? PerkManager.GetTowerCritMultiplier(unitT.prefabID) : 0; }

        private Stun ModifyStunWithPerkBonus(Stun stun) { return IsTower() ? PerkManager.ModifyStunWithPerkBonus(stun.Clone(), unitT.prefabID) : stun; }
        private Slow ModifySlowWithPerkBonus(Slow slow) { return IsTower() ? PerkManager.ModifySlowWithPerkBonus(slow.Clone(), unitT.prefabID) : slow; }
        private Dot ModifyDotWithPerkBonus(Dot dot) { return IsTower() ? PerkManager.ModifyDotWithPerkBonus(dot.Clone(), unitT.prefabID) : dot; }

        private float GetFullHP()
        {
            if (overrideHP > 0)
            {
                return overrideHP * (1 + GetPerkMulHP());
            }
            else
            {
                return defaultHP * (1 + GetPerkMulHP());
            }
        }

        public float GetDamageMin() { return Mathf.Max(0, stats[currentActiveStat].damageMin * (1 + damageBuffMul + dmgABMul + GetPerkMulDamage())); }
        public float GetDamageMax() { return Mathf.Max(0, stats[currentActiveStat].damageMax * (1 + damageBuffMul + dmgABMul + GetPerkMulDamage())); }
        public float GetCooldown() { return Mathf.Max(0.05f, stats[currentActiveStat].cooldown * (1 - cooldownBuffMul - cdABMul - GetPerkMulCooldown())); }

        public float GetRange() { return Mathf.Max(0, stats[currentActiveStat].range * (1 + rangeBuffMul + rangeABMul + GetPerkMulRange())); }
        public float GetAOERadius() { return stats[currentActiveStat].aoeRadius * (1 + GetPerkMulAOERadius()); }

        public float GetCritChance() { return stats[currentActiveStat].crit.chance + criticalBuffMod + GetPerkModCritChance(); }
        public float GetCritMultiplier() { return stats[currentActiveStat].crit.dmgMultiplier + GetPerkModCritMul(); }

        public Stun GetStun() { return ModifyStunWithPerkBonus(stats[currentActiveStat].stun); }
        public Slow GetSlow() { return ModifySlowWithPerkBonus(stats[currentActiveStat].slow); }
        public Dot GetDot() { return ModifyDotWithPerkBonus(stats[currentActiveStat].dot); }

        public int GetShootPointCount() { return shootPoints.Count; }

        public Transform GetShootObjectT()
        {
            return stats[currentActiveStat].shootObjectT;
        }

        public int GetResourceGain() { return stats[currentActiveStat].rscGain; }

        public Buff GetBuff() { return stats[currentActiveStat].buff; }



        //public string GetDespStats(){ return stats[currentActiveStat].desp; }
        public string GetDespGeneral() { return desp; }

        public string GetDespStats()
        {
            if (!IsTower() || stats[currentActiveStat].useCustomDesp) return stats[currentActiveStat].desp;

            UnitTower tower = unitT;

            string text = "";

            if (tower.type == _TowerType.Turret || tower.type == _TowerType.AOE || tower.type == _TowerType.Mine)
            {
                float currentDmgMin = GetDamageMin();
                float currentDmgMax = GetDamageMax();
                if (currentDmgMax > 0)
                {
                    if (currentDmgMin == currentDmgMax) text += "Damage:		 " + currentDmgMax.ToString("f0");
                    else text += "Damage:		 " + currentDmgMin.ToString("f0") + "-" + currentDmgMax.ToString("f0");
                }

                float currentAOE = GetAOERadius();
                if (currentAOE > 0) text += " (AOE)";

                if (tower.type != _TowerType.Mine)
                {
                    float currentCD = GetCooldown();
                    if (currentCD > 0) text += "\nCooldown:	 " + currentCD.ToString("f1") + "s";
                }

                float critChance = GetCritChance();
                if (critChance > 0) text += "\nCritical:		 " + (critChance * 100).ToString("f0") + "%";

                if (text != "") text += "\n";

                Stun stun = GetStun();
                if (stun.IsValid()) text += "\nChance to stuns target";

                Slow slow = GetSlow();
                if (slow.IsValid()) text += "\nSlows target";

                Dot dot = GetDot();
                float dotDmg = dot.GetTotalDamage();
                if (dotDmg > 0) text += "\nDeal " + dotDmg.ToString("f0") + " over " + dot.duration.ToString("f0") + "s";

            }
            else if (tower.type == _TowerType.Support)
            {
                Buff buff = GetBuff();

                if (buff.damageBuff > 0) text += "Damage Buff: " + ((buff.damageBuff) * 100).ToString("f0") + "%";
                if (buff.cooldownBuff > 0) text += "\nCooldown Buff: " + ((buff.cooldownBuff) * 100).ToString("f0") + "%";
                if (buff.rangeBuff > 0) text += "\nRange Buff: " + ((buff.rangeBuff) * 100).ToString("f0") + "%";
                if (buff.criticalBuff > 0) text += "\nRange Buff: " + ((buff.criticalBuff) * 100).ToString("f0") + "%";
                if (text != "") text += "\n";

                if (buff.regenHP > 0)
                {
                    float regenValue = buff.regenHP;
                    float regenDuration = 1;
                    if (buff.regenHP < 1)
                    {
                        regenValue = 1;
                        regenDuration = 1 / buff.regenHP;
                    }
                    text += "\nRegen " + regenValue.ToString("f0") + "HP every " + regenDuration.ToString("f0") + "s";
                }
            }
            else if (tower.type == _TowerType.Resource)
            {
                text += "Regenerate resource overtime";
            }

            return text;
        }

        public float GetDistFromDestination() { return unitC != null ? unitC._GetDistFromDestination() : 0; }

        public bool IsInConstruction() { return IsTower() ? unitT._IsInConstruction() : false; }


        //used by abilities
        private float dmgABMul = 0;
        public void ABBuffDamage(float value, float duration) { StartCoroutine(ABBuffDamageRoutine(value, duration)); }
        IEnumerator ABBuffDamageRoutine(float value, float duration)
        {
            dmgABMul += value;
            yield return new WaitForSeconds(duration);
            dmgABMul -= value;
        }
        private float rangeABMul = 0;
        public void ABBuffRange(float value, float duration) { StartCoroutine(ABBuffDamageRoutine(value, duration)); }
        IEnumerator ABBuffRangeRoutine(float value, float duration)
        {
            rangeABMul += value;
            yield return new WaitForSeconds(duration);
            rangeABMul -= value;
        }
        private float cdABMul = 0;
        public void ABBuffCooldown(float value, float duration) { StartCoroutine(ABBuffCooldownRoutine(value, duration)); }
        IEnumerator ABBuffCooldownRoutine(float value, float duration)
        {
            cdABMul += value;
            yield return new WaitForSeconds(duration);
            cdABMul -= value;
        }

        void OnDrawGizmos()
        {
            if (target != null)
            {
                if (IsCreep()) Gizmos.DrawLine(transform.position, target.transform.position);
            }
        }

    }
}