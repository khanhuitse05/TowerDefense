using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TDTK
{
    public enum _CreepType { Default, Offense, Support }
    public class UnitCreep : Unit
    {

        public delegate void DestinationHandler(UnitCreep unit);
        public static event DestinationHandler onDestinationE;

        public _CreepType type = _CreepType.Default;
        public bool flying = false;

        public GameObject spawnUponDestroyed;
        public int spawnUponDestroyedCount = 0;
        public float spawnUnitHPMultiplier = 0.5f;

        public int waveID = 0;
        public int lifeCost = 1;
        public int scoreValue = 1;

        public int lifeValue = 0;
        public int valueRscMin = 0;
        public int valueRscMax = 1;
        public int valueEnergyGain = 0;

        public bool stopToAttack = false;

        private Vector3 pathDynamicOffset; // offset with origin cell

        public override void Awake()
        {
            SetSubClass(this);
            base.Awake();
            maskTarget = 1 << LayerManager.LayerTower();
            if (thisObj.GetComponent<Collider>() == null)
            {
                thisObj.AddComponent<SphereCollider>();
            }
        }

        public override void Start()
        {
            base.Start();
        }
        public void OverrideByHP(float _hp)
        {
            if (_hp > 0) overrideHP = _hp;
        }
        public void OverrideByWave(int _index, float _dev)
        {
            overrideHP = defaultHP + defaultHP * _index * _dev;
        }
        //parent unit is for unit which is spawned from destroyed unit
        public void Init(PathTD p, int ID, int wID, UnitCreep parentUnit = null)
        {
            Init();

            path = p;
            instanceID = ID;
            waveID = wID;

            if (parentUnit == null)
            {
                waypointID = 1;
                subWaypointID = 0;
                subPath = path.GetWPSectionPath(waypointID);
                //
                float dynamicX = Random.Range(-path.dynamicOffset, path.dynamicOffset);
                float dynamicZ = Random.Range(-path.dynamicOffset, path.dynamicOffset);
                pathDynamicOffset = new Vector3(dynamicX, 0, dynamicZ);
                thisT.position += pathDynamicOffset;
            }
            else
            {
                //inherit stats and path from parent unit
                waypointID = parentUnit.waypointID;
                subWaypointID = parentUnit.subWaypointID;
                subPath = parentUnit.subPath;

                fullHP = parentUnit.fullHP * parentUnit.spawnUnitHPMultiplier;
                HP = fullHP;
                pathDynamicOffset = parentUnit.pathDynamicOffset;
            }

            distFromDestination = CalculateDistFromDestination();

            if (type == _CreepType.Offense)
            {
                StartCoroutine(ScanForTargetRoutine());
                StartCoroutine(TurretRoutine());
            }
            if (type == _CreepType.Support)
            {
                StartCoroutine(SupportRoutine());
            }
        }
        void OnEnable()
        {
            SubPath.onPathChangedE += OnSubPathChanged;
        }
        void OnDisable()
        {
            SubPath.onPathChangedE -= OnSubPathChanged;
        }
        public override void Update()
        {
            base.Update();
            MoveUpdate();

            if (target == null && turretObject != null && !stunned)
            {
                turretObject.localRotation = Quaternion.Slerp(turretObject.localRotation, Quaternion.identity, turretRotateSpeed * Time.deltaTime * 0.25f);
            }
        }
        void MoveUpdate()
        {
            if (!stunned && !dead)
            {
                if (MoveToPoint(subPath[subWaypointID]))
                {
                    subWaypointID += 1;
                    if (subWaypointID >= subPath.Count)
                    {
                        subWaypointID = 0;
                        waypointID += 1;
                        if (waypointID >= path.GetPathWPCount())
                        {
                            ReachDestination();
                        }
                        else
                        {
                            subPath = path.GetWPSectionPath(waypointID);
                        }
                    }
                }
            }
        }
        
        public float rotateSpd = 10;
        public float moveSpeed = 3;

        public PathTD path;
        public List<Vector3> subPath = new List<Vector3>();
        public int waypointID = 1;
        public int subWaypointID = 0;

        void OnSubPathChanged(SubPath platformSubPath)
        {
            if (platformSubPath.parentPath == path && platformSubPath.wpIDPlatform == waypointID)
            {
                ResetSubPath(platformSubPath);
            }
        }
        private static Transform dummyT;
        void ResetSubPath(SubPath platformSubPath)
        {
            if (dummyT == null) dummyT = new GameObject().transform;

            Quaternion rot = Quaternion.LookRotation(subPath[subWaypointID] - thisT.position);
            dummyT.rotation = rot;
            dummyT.position = thisT.position;

            Vector3 pos = dummyT.TransformPoint(0, 0, BuildManager.GetGridSize() / 2);
            NodeTD startN = PathFinder.GetNearestNode(pos, platformSubPath.parentPlatform.GetNodeGraph());
            PathFinder.GetPath(startN, platformSubPath.endN, platformSubPath.parentPlatform.GetNodeGraph(), this.SetSubPath);
        }
        void SetSubPath(List<Vector3> pathList)
        {
            subPath = pathList;
            subWaypointID = 0;
            distFromDestination = CalculateDistFromDestination();
        }

        //function call to rotate and move toward a pecific point, return true when the point is reached
        public bool MoveToPoint(Vector3 point)
        {
            if (type == _CreepType.Offense && stopToAttack && target != null) return false;
            //this is for dynamic waypoint, each unit creep have it's own offset pos
            point += pathDynamicOffset;
            float dist = Vector3.Distance(point, thisT.position);
            //if the unit have reached the point specified
            if (dist < 0.005f) return true;
            //rotate towards destination
            if (moveSpeed > 0)
            {
                Quaternion wantedRot = Quaternion.LookRotation(point - thisT.position);
                thisT.rotation = Quaternion.Slerp(thisT.rotation, wantedRot, rotateSpd * Time.deltaTime);
            }
            //move, with speed take distance into accrount so the unit wont over shoot
            Vector3 dir = (point - thisT.position).normalized;
            thisT.Translate(dir * Mathf.Min(dist, moveSpeed * slowMultiplier * Time.deltaTime), Space.World);
            distFromDestination -= (moveSpeed * slowMultiplier * Time.deltaTime);

            return false;
        }

        void ReachDestination()
        {
            if (path.loop)
            {
                if (onDestinationE != null) onDestinationE(this);
                subWaypointID = 0;
                waypointID = path.GetLoopPoint();
                subPath = path.GetWPSectionPath(waypointID);
            }
            else
            {
                dead = true;
                if (onDestinationE != null) onDestinationE(this);
                float delay = 0;
                if (animCreep != null) { delay = animCreep.PlayDestination(); }
                StartCoroutine(_ReachDestination(delay));
            }
        }
        IEnumerator _ReachDestination(float duration)
        {
            yield return new WaitForSeconds(duration);
            ObjectPoolManager.Unspawn(thisObj);
        }

        public float CreepDestroyed()
        {
            int rscGain = Random.Range(valueRscMin, valueRscMax);
            ResourceManager.GainResource(rscGain, PerkManager.GetRscCreepKilled());
            AbilityManager.GainEnergy(valueEnergyGain + (int)PerkManager.GetEnergyWaveClearedModifier());

            if (spawnUponDestroyed != null && spawnUponDestroyedCount > 0)
            {
                for (int i = 0; i < spawnUponDestroyedCount; i++)
                {
                    Vector3 posOffset = Vector3.zero;
                    if (spawnUponDestroyedCount > 1)
                    {
                        posOffset = new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
                    }
                    GameObject obj = ObjectPoolManager.Spawn(spawnUponDestroyed, thisT.position + posOffset, thisT.rotation);
                    UnitCreep unit = obj.GetComponent<UnitCreep>();
                    int ID = SpawnManager.AddDestroyedSpawn(waveID);
                    unit.Init(path, ID, waveID, this);
                }
            }
            if (animCreep != null) { return animCreep.PlayDead(); }

            return 0;
        }

        private UnitCreepAnimation animCreep;
        public void SetAnimationComponent(UnitCreepAnimation ani) { animCreep = ani; }
        public void Hit() { if (animCreep != null) animCreep.PlayHit(); }

        public float GetMoveSpeed() { return moveSpeed * slowMultiplier; }

        public float distFromDestination = 0;
        public float _GetDistFromDestination() { return distFromDestination; }
        public float CalculateDistFromDestination()
        {
            float dist = Vector3.Distance(thisT.position, subPath[subWaypointID]);
            for (int i = subWaypointID + 1; i < subPath.Count; i++)
            {
                dist += Vector3.Distance(subPath[i - 1], subPath[i]);
            }
            dist += path.GetPathDistance(waypointID + 1);
            return dist;
        }
    }

}