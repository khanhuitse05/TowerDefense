using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TDTK {

	//Use in BuildManager, the status return when CheckBuildPoint() is called
	public enum _TileStatus{
		NoPlatform, 	//no platform at detected
		Available, 		//there's a valid build point
		Unavailable, 	//the build point is invalid (occupied)
		Blocked			//building on the spot will block the only available path
	}
	
	//Use in BuildManager, contain all the infomation of the specific select build spot
	[System.Serializable]
	public class BuildInfo{
		public Vector3 position=Vector3.zero;		//the position of the build point
		public PlatformTD platform;					//the platform the build point belongs to
		
		//the prefabIDs of the towers available to be build
		public List<int> availableTowerIDList=new List<int>();	
	}
	
	
	
	
	[System.Serializable]
	public class TDTKItem{
		public int ID=0;
		public string name="";
		public Sprite icon;
	}
	
	[System.Serializable]
	public class DAType : TDTKItem{
		public string desp="";
	}
	[System.Serializable]
	public class DamageType : DAType{
		
	}
	[System.Serializable]
	public class ArmorType : DAType{
		public List<float> modifiers=new List<float>();
	}
	
	
	
}