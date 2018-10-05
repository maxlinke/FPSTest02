using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionAndTriggerLogger : MonoBehaviour {

	int counter;

	bool collisionEnter;
	bool collisionStay;
	bool collisionExit;

	bool triggerEnter;
	bool triggerStay;
	bool triggerExit;

	void FixedUpdate () {
		string output = counter.ToString() + " :\t";
		counter++;

		output += "cEnt " + BoolToSign(collisionEnter) + "\t";
		collisionEnter = false;
		output += "cSty " + BoolToSign(collisionStay) + "\t";
		collisionStay = false;
		output += "cExt " + BoolToSign(collisionExit) + "\t";
		collisionExit = false;

		output += "tEnt " + BoolToSign(triggerEnter) + "\t";
		triggerEnter = false;
		output += "tSty " + BoolToSign(triggerStay) + "\t";
		triggerStay = false;
		output += "tExt " + BoolToSign(triggerExit) + "\t";
		triggerExit = false;

		Debug.Log(output);
	}

	void OnCollisionEnter (Collision collision) {
		collisionEnter = true;
	}

	void OnCollisionStay (Collision collision) {
		collisionStay = true;
	}

	void OnCollisionExit (Collision collision) {
		collisionExit = true;
	}

	void OnTriggerEnter (Collider otherCollider) {
		triggerEnter = true;
	}

	void OnTriggerStay (Collider otherCollider) {
		triggerStay = true;
	}

	void OnTriggerExit (Collider otherCollider) {
		triggerExit = true;
	}

	string BoolToSign (bool value) {
		if(value) return "#";
		else return "_";
	}
}
