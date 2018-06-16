using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class littleshipscript : MonoBehaviour {

	public Rigidbody rb;

	void Start () {
		rb.centerOfMass = Vector3.down * 0.1f;
	}

}
