using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactLogger : MonoBehaviour {

	const float referenceGravity = 29.43f;
	const float impulseScale = 0.01f;

	[SerializeField] float peakForceInKG;
	[SerializeField] bool resetValues;

	List<Vector3> impulses;

	void Start () {
		impulses = new List<Vector3>();
	}

	void Update () {
		if(resetValues){
			peakForceInKG = 0f;
			resetValues = false;
		}
	}

	void FixedUpdate () {
		Vector3 impulseSum = Vector3.zero;
		float impulseMagnitudeSum = 0f;
		for(int i=0; i<impulses.Count; i++){
			impulseSum += impulses[i];
			impulseMagnitudeSum += impulses[i].magnitude;
			Debug.DrawRay(transform.position, impulses[i] * impulseScale, Color.magenta, Time.fixedDeltaTime, false);
		}
		Debug.DrawRay(transform.position, impulseSum * impulseScale, Color.yellow, Time.fixedDeltaTime, false);
		float deltaMag = impulseMagnitudeSum - impulseSum.magnitude;
		Debug.Log("CRUSHING WITH " + deltaMag);
		impulses.Clear();
	}

	//F = collision.impulse / Time.fixedDeltaTime;

	void OnCollisionEnter (Collision collision) {
		CalculateAndLogForce(collision);
		impulses.Add(collision.impulse);
	}

	void OnCollisionStay (Collision collision) {
		CalculateAndLogForce(collision);
		impulses.Add(collision.impulse);
	}

	void CalculateAndLogForce (Collision collision) {
		float f = collision.impulse.magnitude / Time.fixedDeltaTime;
		float m = f / referenceGravity;									//use this for breakable props
//		Debug.Log(string.Format("F = {0:F2} (~{1:F2} kg)", f, m));		//this reaaaaally kills the performance for some reason...
		if(m > peakForceInKG) peakForceInKG = m;
	}

}
