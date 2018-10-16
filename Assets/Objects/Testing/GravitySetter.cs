using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravitySetter : MonoBehaviour {

	[SerializeField] Vector3 direction;
	[SerializeField] float strength;

	void Start () {
		direction = Physics.gravity.normalized;
		strength = Physics.gravity.magnitude;
	}
	
	void Update () {
		Physics.gravity = direction.normalized * strength;
	}
}
