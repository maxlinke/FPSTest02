using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSphere_Bouncy : MonoBehaviour {

	public Rigidbody rb;
	public KeyCode key_bounce;
	public float velocity;

	public void Update(){
		if(Input.GetKeyDown(key_bounce)){
			bounce();
		}
	}

	public void bounce(){
		rb.velocity += Vector3.up * velocity;
	}

	public float getBounceHeight(){
		return (velocity * velocity) / (2f * Physics.gravity.magnitude);
	}
}
