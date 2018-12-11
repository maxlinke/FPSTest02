using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchPad : MonoBehaviour, IInteractable {

	public enum LaunchMode {
		SETVELOCITYABSOLUTE,
		SETVELOCITYRELATIVE,
		ADDVELOCITY
	}

	[SerializeField] float launchSpeed;
	[SerializeField] LaunchMode launchMode;
	[SerializeField] Collider launchTrigger;
	[SerializeField] GameObject[] objectsAffectedByActivation;
	[SerializeField] bool startActivated;

	Vector3 lastPos;
	Vector3 ownVelocity;
	bool active;

	void Start () {
		if (startActivated) {
			SetActivationState (true);
			active = true;
		} else {
			SetActivationState (false);
			active = false;
		}
		lastPos = transform.position;
	}

	void FixedUpdate () {
		ownVelocity = transform.position - lastPos;
		lastPos = transform.position;
	}
	
	void OnTriggerEnter (Collider otherCollider) {
		if (active) {
			Rigidbody otherRB = otherCollider.attachedRigidbody;
			if (otherRB != null) {
				Vector3 launchVelocity = this.transform.up * launchSpeed;
				switch (launchMode) {
				case LaunchMode.SETVELOCITYABSOLUTE:
					otherRB.velocity = launchVelocity;
					break;
				case LaunchMode.SETVELOCITYRELATIVE:
					otherRB.velocity = launchVelocity + ownVelocity;
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
