using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerWaterTriggerScript : MonoBehaviour {

	public bool isInWater;
	public float waterLevel;
	private bool triggerStay;


	void FixedUpdate(){
		isInWater = triggerStay;
		triggerStay = false;
	}

	void OnTriggerEnter(Collider collider){
		WaterBodyScript wbs = collider.gameObject.GetComponent<WaterBodyScript>();
		//if(wbs == null) throw new NullReferenceException("watertrigger just entered something tagged as water without a waterbodyscript. object in question : " + collider.name);
		//else waterLevel = wbs.waterLevel;
		if(wbs != null) waterLevel = wbs.waterLevel;
	}

	//i know a simple ontriggerenter and ontriggerexit would probably work just fine, but there is the possibility that it doesn't and that's how you end up swimming in thin air...

	void OnTriggerStay(Collider collider){
		triggerStay = true;
	}
}
