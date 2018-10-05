using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRBMover : MonoBehaviour {

	[SerializeField] Rigidbody rb;
	[SerializeField] float speed;

	[SerializeField] KeyCode key_x_pos;
	[SerializeField] KeyCode key_x_neg;
	[SerializeField] KeyCode key_y_pos;
	[SerializeField] KeyCode key_y_neg;
	[SerializeField] KeyCode key_z_pos;
	[SerializeField] KeyCode key_z_neg;

	void FixedUpdate () {
		Vector3 velocity = Vector3.zero;
		if(Input.GetKey(key_x_pos)) velocity += new Vector3(1, 0, 0);
		if(Input.GetKey(key_x_neg)) velocity -= new Vector3(1, 0, 0);
		if(Input.GetKey(key_y_pos)) velocity += new Vector3(0, 1, 0);
		if(Input.GetKey(key_y_neg)) velocity -= new Vector3(0, 1, 0);
		if(Input.GetKey(key_z_pos)) velocity += new Vector3(0, 0, 1);
		if(Input.GetKey(key_z_neg)) velocity -= new Vector3(0, 0, 1);
		rb.velocity = velocity.normalized * speed;
	}

}
