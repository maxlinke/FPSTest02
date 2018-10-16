using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationLerper : MonoBehaviour {

	[SerializeField] [Range(0f, 1f)] float lerpFactor;

	Quaternion a, b;

	void Start () {
		a = Quaternion.LookRotation(Vector3.forward, Vector3.up);
		b = Quaternion.LookRotation(Vector3.forward, Vector3.down);
	}
	
	void Update () {
		transform.rotation = Quaternion.Lerp(a, b, lerpFactor);
	}
}
