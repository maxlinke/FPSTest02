using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfBoundsTriggerScript : MonoBehaviour {

	public GameObject teleportPoint;

	void OnTriggerExit (Collider collider) {
		Rigidbody otherRB = collider.attachedRigidbody;
		if(otherRB != null){
			Debug.LogWarning("\"" + otherRB.name + "\" went out of bounds and was teleported to " + teleportPoint + " " + teleportPoint.transform.position);
			Debug.DrawLine(otherRB.transform.position, teleportPoint.transform.position, Color.yellow, 30f, false);
			otherRB.velocity = Vector3.zero;
			otherRB.transform.position = teleportPoint.transform.position;
		}else{
			Debug.LogWarning("\"" + collider.gameObject.name + "\" went out of bounds and was destroyed");
			Destroy(collider.gameObject);
		}
	}

}
