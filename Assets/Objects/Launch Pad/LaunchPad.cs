using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchPad : MonoBehaviour, IInteractable {

	public enum LaunchMode {
		SETVELOCITY,
		ADDVELOCITY
	}

	[SerializeField] Vector3 launchVelocity;
	[SerializeField] LaunchMode launchMode;
	[SerializeField] Collider launchTrigger;
	[SerializeField] GameObject[] objectsAffectedByActivation;
	[SerializeField] bool startActivated;

	bool active;

	void Start () {
		if (startActivated) {
			SetActivationState (true);
			active = true;
		} else {
			SetActivationState (false);
			active = false;
		}
	}
	
	void OnTriggerEnter (Collider otherCollider) {
		if (active) {			//TODO possibly redundant?
			Rigidbody otherRB = otherCollider.attachedRigidbody;
			if (otherRB != null) {
				switch (launchMode) {
				case LaunchMode.SETVELOCITY:
					otherRB.velocity = launchVelocity;		//TODO what if the launchpad is moving?
					break;
				case LaunchMode.ADDVELOCITY:
					otherRB.velocity += launchVelocity;
					break;
				default:
					throw new UnityException ("Unsupported LaunchMode \"" + launchMode.ToString () + "\"");
				}
			}
		}
	}

	void SetActivationState (bool newState) {
		for (int i = 0; i < objectsAffectedByActivation.Length; i++) {
			objectsAffectedByActivation [i].SetActive (newState);
		}
		launchTrigger.enabled = newState;
	}

	public void Interact (GameObject other) {
		active = !active;
		SetActivationState (active);
	}

	public string GetDescription () {
		if (active)	return "Deactivate";
		else return "Activate";
	}

}
