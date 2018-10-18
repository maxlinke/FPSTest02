using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinematicRBMover : MonoBehaviour {

	[SerializeField] Rigidbody rb;
	[SerializeField] Vector3 direction;
	[SerializeField] float speed;

	[SerializeField] KeyCode key_positive;
	[SerializeField] KeyCode key_negative;

	void Start () {
		
	}
	
	void Update () {
		
	}

	void FixedUpdate () {
		Vector3 v;
		if(Input.GetKey(key_positive)){
			v = direction.normalized * speed;
		}else if(Input.GetKey(key_negative)){
			v = direction.normalized * speed * -1f;
		}else{
			v = Vector3.zero;
		}
		rb.MovePosition(rb.transform.position + (v * Time.fixedDeltaTime));
	}

}
