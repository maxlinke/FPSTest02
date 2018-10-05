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

		Rigidbody otherRB = collider.gameObject.GetComponentInParent<Rigidbody>();
		if(otherRB != null){
			otherRB.velocity = Vector3.zero;
			otherRB.transform.position = teleportPoint.transform.position;
		}else{
			Destroy(collider.gameObject);
		}
	}

}
