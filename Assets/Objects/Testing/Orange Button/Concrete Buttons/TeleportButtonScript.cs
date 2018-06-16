using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportButtonScript : MonoBehaviour, IInteractable {

	public GameObject teleportTarget;
	public string description;
	public bool includeDistanceToTargetInMessage;
	public bool rotateObject;

	public void Interact(GameObject other){
		other.transform.position = teleportTarget.transform.position;
		if(rotateObject){
			Vector3 otherAngles = other.transform.localEulerAngles;
			Vector3 targetAngles = teleportTarget.transform.localEulerAngles;
			other.transform.localEulerAngles = new Vector3(otherAngles.x, targetAngles.y, otherAngles.z);
		}
	}

	public string GetDescription(){
		string outputMessage = "";
		if(description == null || description.Equals("")){
			outputMessage += "Teleport";
		}else{
			outputMessage += description;
		}
		if(includeDistanceToTargetInMessage){
			float distance = Vector3.Distance(transform.position, teleportTarget.transform.position);
			outputMessage += " (" + (int)(Mathf.Floor(distance + 0.5f)) + "m)";
		}
		return outputMessage;
	}
}
