using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {
	
	public enum _ShootObjectType{Projectile, Missile, Beam, Effect}
	
	public class ShootObject : MonoBehaviour {
		
		public _ShootObjectType type;
		
		public float speed=5;
		public float beamDuration=.5f;
		
		private Transform shootPoint;
		
		private bool hit=false;
		
		public bool autoSearchLineRenderer=true;
		public List<LineRenderer> lineList=new List<LineRenderer>();
		
		private List<TrailRenderer> trailList=new List<TrailRenderer>();
		
		
		public GameObject shootEffect;
		public GameObject hitEffect;
		
		
		private AttackInstance attInstance;
		
		private GameObject thisObj;
		private Transform thisT;
		
		void Awake(){
			thisObj=gameObject;
			thisT=transform;
			
			thisObj.layer=LayerManager.LayerShootObject();
			
			if(autoSearchLineRenderer){
				LineRenderer[] lines = thisObj.GetComponentsInChildren<LineRenderer>(true);
				for(int i=0; i<lines.Length; i++) lineList.Add(lines[i]);
			}
			
			TrailRenderer[] trails = thisObj.GetComponentsInChildren<TrailRenderer>(true);
			for(int i=0; i<trails.Length; i++) trailList.Add(trails[i]);
			
			if(shootEffect!=null) ObjectPoolManager.New(shootEffect, 3);
			if(hitEffect!=null) ObjectPoolManager.New(hitEffect, 3);
		}
		
		void Start () {
		
		}
		
		void OnEnable(){
			for(int i=0; i<trailList.Count; i++) StartCoroutine(ClearTrail(trailList[i]));
		}
		void OnDisable(){
			
		}
		
		public void Shoot(AttackInstance attInst=null, Transform sp=null){
			if(attInst.tgtUnit==null || attInst.tgtUnit.GetTargetT()==null){
				ObjectPoolManager.Unspawn(thisObj);
				return;
			}
			
			attInstance=attInst;
			target=attInstance.tgtUnit;
			targetPos=target.GetTargetT().position;
			hitThreshold=Mathf.Max(0.1f, target.hitThreshold);
			
			shootPoint=sp;
			if(shootPoint!=null) thisT.rotation=shootPoint.rotation;
			
			if(shootEffect!=null) ObjectPoolManager.Spawn(shootEffect, thisT.position, thisT.rotation);
			
			hit=false;
			
			if(type==_ShootObjectType.Projectile) StartCoroutine(ProjectileRoutine());
			if(type==_ShootObjectType.Beam) StartCoroutine(BeamRoutine());
			if(type==_ShootObjectType.Missile) StartCoroutine(MissileRoutine());
			if(type==_ShootObjectType.Effect) StartCoroutine(EffectRoutine());
		}
		
		private Unit target;
		private Vector3 targetPos;
		public float maxShootAngle=30f;
		public float maxShootRange=0.5f;
		private float hitThreshold=0.15f;		
		
		IEnumerator EffectRoutine(){
			yield return new WaitForSeconds(0.125f);
			Hit();
		}
		
		IEnumerator BeamRoutine(){
			float timeShot=Time.time;
			
			while(!hit){
				if(target!=null) targetPos=target.GetTargetT().position;

                float dist=Vector3.Distance(shootPoint.position, targetPos);
				Ray ray=new Ray(shootPoint.position, (targetPos-shootPoint.position));
				Vector3 targetPosition=ray.GetPoint(dist-hitThreshold);

                Vector3 _temp;
                int _count;
                Vector3 _offset;
                Vector3[] _position;
                for (int i=0; i<lineList.Count; i++){
                    _temp = targetPosition - shootPoint.position;
                    _count = (int)(dist / 0.2) + 2;
                    _offset = _temp / _count;
                    lineList[i].numPositions = _count;
                    _position = new Vector3[_count];
                    _position[0] = shootPoint.position;
                    for (int j = 1; j < (_count - 1); j++)
                    {
                        Vector3 _pos = new Vector3(shootPoint.position.x + j * _offset.x, shootPoint.position.y + j * _offset.y, shootPoint.position.z + j * _offset.z + Random.Range(-0.2f, 0.2f));
                        _position[j] = _pos;
                    }
                    _position[_count - 1] = targetPosition;
                    lineList[i].SetPositions(_position);
				}
				
				if(Time.time-timeShot>beamDuration){
					Hit();
					break;
				}
				
				yield return null;
			}
		}
		
		
		IEnumerator ProjectileRoutine(){
			if(shootEffect!=null) Instantiate(shootEffect, thisT.position, thisT.rotation);
			
			float timeShot=Time.time;
			
			//make sure the shootObject is facing the target and adjust the projectile angle
			thisT.LookAt(targetPos);
			float angle=Mathf.Min(1, Vector3.Distance(thisT.position, targetPos)/maxShootRange)*maxShootAngle;
			//clamp the angle magnitude to be less than 45 or less the dist ratio will be off
			thisT.rotation=thisT.rotation*Quaternion.Euler(-angle, 0, 0);
			
			Vector3 startPos=thisT.position;
			float iniRotX=thisT.rotation.eulerAngles.x;
			
			float y=Mathf.Min(targetPos.y, startPos.y);
			float totalDist=Vector3.Distance(startPos, targetPos);
			
			//while the shootObject havent hit the target
			while(!hit){
				if(target!=null) targetPos=target.GetTargetT().position;
				
				//calculating distance to targetPos
				Vector3 curPos=thisT.position;
				curPos.y=y;
				float currentDist=Vector3.Distance(curPos, targetPos);
				float curDist=Vector3.Distance(thisT.position, targetPos);
				//if the target is close enough, trigger a hit
				if(curDist<hitThreshold && !hit){
					Hit();
					break;
				}
				
				if(Time.time-timeShot<3.5f){
					//calculate ratio of distance covered to total distance
					float invR=1-Mathf.Min(1, currentDist/totalDist);
					
					//use the distance information to set the rotation, 
					//as the projectile approach target, it will aim straight at the target
					Vector3 wantedDir=targetPos-thisT.position;
					if(wantedDir!=Vector3.zero){
						Quaternion wantedRotation=Quaternion.LookRotation(wantedDir);
						float rotX=Mathf.LerpAngle(iniRotX, wantedRotation.eulerAngles.x, invR);
						
						//make y-rotation always face target
						thisT.rotation=Quaternion.Euler(rotX, wantedRotation.eulerAngles.y, wantedRotation.eulerAngles.z);
					}
				}
				else{
					//this shoot time exceed 3.5sec, abort the trajectory and just head to the target
					thisT.LookAt(targetPos);
				}
					
				//move forward
				thisT.Translate(Vector3.forward*Mathf.Min(speed*Time.deltaTime, curDist));
				
				yield return new WaitForSeconds(Time.fixedDeltaTime);
			}
		}
		
		
		public float shootAngleY=20;
		private float missileSpeedModifier=1;
		IEnumerator MissileSpeedRoutine(){
			missileSpeedModifier=.05f;
			float duration=0;
			while(duration<1){
				missileSpeedModifier=Mathf.Sin(Mathf.Sin(duration*Mathf.PI/2)*Mathf.PI/2);
				duration+=Time.deltaTime*1f;
				yield return null;
			}
			missileSpeedModifier=1;
		}
		IEnumerator MissileRoutine() {
			StartCoroutine(MissileSpeedRoutine());
			
			float angleX=Random.Range(maxShootAngle/2, maxShootAngle);
			float angleY=Random.Range(shootAngleY/2, maxShootAngle);
			if(Random.Range(0f, 1f)>0.5f) angleY*=-1;
			thisT.LookAt(targetPos);
			thisT.rotation=thisT.rotation;
			Quaternion wantedRotation=thisT.rotation*Quaternion.Euler(-angleX, angleY, 0);
			float rand=Random.Range(4f, 10f);
			
			float totalDist=Vector3.Distance(thisT.position, targetPos);
			
			float estimateTime=totalDist/speed;
			float shootTime=Time.time;
			
			Vector3 startPos=thisT.position;
			
			while(!hit){
				if(target!=null) targetPos=target.GetTargetT().position;
				float currentDist=Vector3.Distance(thisT.position, targetPos);
				
				float delta=totalDist-Vector3.Distance(startPos, targetPos);
				float eTime=estimateTime-delta/speed;
				
				if(Time.time-shootTime>eTime){
					Vector3 wantedDir=targetPos-thisT.position;
					if(wantedDir!=Vector3.zero){
						wantedRotation=Quaternion.LookRotation(wantedDir);
						float val1=(Time.time-shootTime)-(eTime);
						thisT.rotation=Quaternion.Slerp(thisT.rotation, wantedRotation, val1/(eTime*currentDist));
					}
				}
				else thisT.rotation=Quaternion.Slerp(thisT.rotation, wantedRotation, Time.fixedDeltaTime*rand);
				
				if(currentDist<hitThreshold){
					Hit();
					break;
				}
				
				thisT.Translate(Vector3.forward*Mathf.Min(speed*Time.fixedDeltaTime*missileSpeedModifier, currentDist));
				yield return new WaitForSeconds(Time.fixedDeltaTime);
			}
		}
		
		
		void Hit(){
			hit=true;
			
			if(hitEffect!=null) ObjectPoolManager.Spawn(hitEffect, targetPos, Quaternion.identity);
			
			thisT.position=targetPos;
			
			if(attInstance.srcUnit.GetAOERadius()>0){
				LayerMask mask=attInstance.srcUnit.GetTargetMask();
				
				Collider[] cols=Physics.OverlapSphere(thisT.position, attInstance.srcUnit.GetAOERadius(), mask);
				if(cols.Length>0){
					List<Unit> tgtList=new List<Unit>();
					for(int i=0; i<cols.Length; i++){
						Unit unit=cols[i].gameObject.GetComponent<Unit>();
						if(!unit.dead) tgtList.Add(unit);
					}
					if(tgtList.Count>0){
						for(int i=0; i<tgtList.Count; i++){
							if(tgtList[i]==target) target.ApplyEffect(attInstance);
							else{
								AttackInstance attInst=new AttackInstance();
								attInst.srcUnit=attInstance.srcUnit;
								attInst.tgtUnit=tgtList[i];
								attInst.Process();
								tgtList[i].ApplyEffect(attInst);
							}
						}
					}
				}
			}
			else{
				if(target!=null) target.ApplyEffect(attInstance);
			}
			
			ObjectPoolManager.Unspawn(thisObj);
			//Destroy(thisObj);
		}
		
		IEnumerator ClearTrail(TrailRenderer trail){
			if(trail==null) yield break;
			float trailDuration=trail.time;
			trail.time=-1;
			yield return null;
			trail.time=trailDuration;
		}
		
		
		
	}

}