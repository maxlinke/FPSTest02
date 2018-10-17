using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactLogger : MonoBehaviour {

	const float referenceGravity = 29.43f;

	[SerializeField] float peakForceInKG;
	[SerializeField] bool resetPeakForce;
	
	void Update () {
		if(resetPeakForce){
			peakForceInKG = 0f;
			resetPeakForce = false;
		}
	}

	//F = collision.impulse / Time.fixedDeltaTime;

	void OnCollisionEnter (Collision collision) {
		CalculateAndLogForce(collision);
	}

	void OnCollisionStay (Collision collision) {
		CalculateAndLogForce(collision);
	}

	void CalculateAndLogForce (Collision collision) {
		float f = collision.impulse.magnitude / Time.fixedDeltaTime;
		float m = f / referenceGravity;
		Debug.Log(string.Format("F = {0:F2} (~{1:F2} kg)", f, m));
		if(m > peakForceInKG) peakForceInKG = m;
	}

}
