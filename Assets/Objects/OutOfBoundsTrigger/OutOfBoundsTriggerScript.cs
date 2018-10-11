using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfBoundsTriggerScript : MonoBehaviour {

	public GameObject teleportPoint;

	void OnTriggerExit(Collider collider){
//		GameObject obj = collider.gameObject;
//		if(TagManager.CompareTag("Player", obj)){
//			obj.transform.position = teleportPoint.transform.position;
//			obj.GetComponent<Rigidbody>().velocity = Vector3.zero;
//		}else{
//			Debug.LogWarning("OOB trigger destroyed gameObject \"" + obj.name + "\"");
//			Destroy(obj);
//		}

//		Rigidbody otherRB = collider.gameObject.GetComponentInParent<Rigidbody>();
		Rigidbody otherRB = collider.attachedRigidbody;
		if(otherRB != null){
			Debug.LogWarning(otherRB.name + " went out of bounds and was teleported to " + teleportPoint);
			otherRB.velocity = Vector3.zero;
			otherRB.transform.position = teleportPoint.transform.position;
		}else{
			Debug.LogWarning(collider.gameObject.name + " went out of bounds and was destroyed");
			Destroy(collider.gameObject);
		}
	}

}
