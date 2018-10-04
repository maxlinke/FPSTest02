using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnCollisionEnter : MonoBehaviour {

	void OnCollisionEnter (Collision collision) {
		Destroy(this.gameObject);
	}

}
