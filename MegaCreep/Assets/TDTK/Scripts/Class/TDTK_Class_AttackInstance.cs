using UnityEngine;

namespace TDTK
{
    //contains all the information of an attack
    //an instance of the class is generate each time an attack took place
    public class AttackInstance
    {

        public bool processed = false;
        public Unit srcUnit;
        public Unit tgtUnit;

        public bool critical = false;

        public bool stunned = false;
        public bool slowed = false;
        public bool dotted = false;

        public float damage = 0;

        public Stun stun;
        public Slow slow;
        public Dot dot;

        //do the stats processing
        public void Process()
        {
            if (processed) return;
            processed = true;
            if (srcUnit != null) Process_SrcUnit();
        }

        public void Process_SrcUnit()
        {
            damage = Random.Range(srcUnit.GetDamageMin(), srcUnit.GetDamageMax());

            float critChance = srcUnit.GetCritChance();
            if (Random.Range(0f, 1f) < critChance)
            {
                critical = true;
                damage *= srcUnit.GetCritMultiplier();
            }

            float dmgModifier = DamageTable.GetModifier(tgtUnit.armorType, srcUnit.damageType);
            damage *= dmgModifier;

            stunned = srcUnit.GetStun().IsApplicable();
            slowed = srcUnit.GetSlow().IsValid();
            dotted = srcUnit.GetDot().GetTotalDamage() > 0;
            if (stunned) stun = srcUnit.GetStun().Clone();
            if (slowed) slow = srcUnit.GetSlow().Clone();
            if (dotted) dot = srcUnit.GetDot().Clone();
        }

        //clone an instance
        public AttackInstance Clone()
        {
            AttackInstance attInstance = new AttackInstance();

            attInstance.processed = processed;
            attInstance.srcUnit = srcUnit;
            attInstance.tgtUnit = tgtUnit;

            attInstance.critical = critical;

            attInstance.stunned = stunned;
            attInstance.slowed = slowed;
            attInstance.dotted = dotted;

            attInstance.damage = damage;

            attInstance.stun = stun;
            attInstance.slow = slow;
            attInstance.dot = dot;

            return attInstance;
        }
    }
}