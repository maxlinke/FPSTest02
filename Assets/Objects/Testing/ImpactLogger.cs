using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactLogger : MonoBehaviour {

	const float referenceGravity = 29.43f;
	const float impulseScale = 0.01f;
	const float crushScale = 0.01f;

	[SerializeField] bool gravity;
	[SerializeField] bool resetValues;
	//only visible in debug mode
	float peakForceInKG;
	float peakCrushValue;

	float currentForce;
	float currentCrushValue;
	int numberOfCollisions;

	Rigidbody rb;
	List<Collision> collisions;

	bool jumpInput;

	void Start () {
		rb = GetComponent<Rigidbody>();
		collisions = new List<Collision>();
	}

	void Update () {
		if(resetValues){
			peakForceInKG = 0f;
			peakCrushValue = 0f;
			resetValues = false;
		}
		if(Input.GetKeyDown(KeyCode.Space)){
			jumpInput = true;
		}
	}

	void FixedUpdate () {
		numberOfCollisions = collisions.Count;
		Vector3 impulseSum = Vector3.zero;
		float impulseMagnitudeSum = 0f;
		for(int i=0; i<collisions.Count; i++){
			Vector3 impulse = collisions[i].impulse;		//sometimes both impulses point in the same direction, sometimes not. WWHHHHHYYYYYYYYYYYYYYYYYYYYYYY maybe i have to go with relativevelocity and hope that's consistent?
			impulseSum += impulse;
			impulseMagnitudeSum += impulse.magnitude;
//			Debug.DrawRay(transform.position, impulse * impulseScale, Color.magenta, Time.fixedDeltaTime, false);
			currentForce = CalculateForce(collisions[i]);
			if(currentForce > peakForceInKG){
				peakForceInKG = currentForce;
			}
			DrawCollisionContacts(collisions[i]);
		}
//		Debug.DrawRay(transform.position, impulseSum * impulseScale, Color.yellow, Time.fixedDeltaTime, false);
		float deltaMag = impulseMagnitudeSum - impulseSum.magnitude;	//this is the crushing "force"... 
		//it scales linearly with mass of THIS rigidbody (or the other one)
		//		Debug.Log("CRUSHING WITH " + deltaMag);
		//		if(deltaMag > peakCrushValue) peakCrushValue = deltaMag;

		currentCrushValue = deltaMag / (Time.fixedDeltaTime * rb.mass);		//this is a pretty good approximation.
		DebugDrawHelper.DrawSphere(transform.position, currentCrushValue * crushScale, Color.yellow, Time.fixedDeltaTime, false);
//		Debug.Log("CRUSHING WITH " + crushForce + " | " + deltaMag);
		if(currentCrushValue > peakCrushValue) peakCrushValue = currentCrushValue;

		collisions.Clear();
		if(jumpInput){
			rb.velocity += Vector3.up * 10f;
		}
		if(gravity){
			rb.velocity += Physics.gravity * Time.fixedDeltaTime;
		}
		jumpInput = false;
	}

	//F = collision.impulse / Time.fixedDeltaTime;

	void OnCollisionEnter (Collision collision) {
		collisions.Add(collision);
	}

	void OnCollisionStay (Collision collision) {
		collisions.Add(collision);
	}

	float CalculateForce (Collision collision) {
		float f = collision.impulse.magnitude / Time.fixedDeltaTime;
		float m = f / referenceGravity;									//use this for breakable props
//		Debug.Log(string.Format("F = {0:F2} (~{1:F2} kg)", f, m));		//this reaaaaally kills the performance for some reason...
		return m;
	}

	void DrawCollisionContacts (Collision collision) {
		for(int i=0; i<collision.contacts.Length; i++){
			Vector3 point = collision.contacts[i].point;
			Vector3 normal = collision.contacts[i].normal;
			Debug.DrawRay(point, normal * 0.2f, Color.white, Time.fixedDeltaTime, false);
			Debug.DrawRay(point, collision.impulse * impulseScale, Color.magenta, Time.fixedDeltaTime, false);
		}
	}

}