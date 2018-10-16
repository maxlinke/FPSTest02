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
		DrawCollisionRays(collision, Color.yellow, 0.25f);
	}

	void OnCollisionStay (Collision collision) {
		collisionStay = true;
		DrawCollisionRays(collision, Color.green, 0.25f);
	}

	void OnCollisionExit (Collision collision) {
		collisionExit = true;
		DrawCollisionRays(collision, Color.red, 0.25f);
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

	void DrawCollisionRays (Collision collision, Color color, float length = 1f) {
		for(int i=0; i<collision.contacts.Length; i++){
			Vector3 start = collision.contacts[i].point;
			Vector3 dir = collision.contacts[i].normal;
			Debug.DrawRay(start, dir * length, color, Time.fixedDeltaTime);
		}
	}

}
