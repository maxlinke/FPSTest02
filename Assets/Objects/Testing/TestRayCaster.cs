using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRayCaster : MonoBehaviour {

	[SerializeField] bool disableBeforeCast;
	
	void Update () {
		if(disableBeforeCast){
			gameObject.SetActive(false);
		}
		Vector3 start = transform.position;
		Vector3 dir = transform.forward;
		RaycastHit hit;
		if(Physics.Raycast(start, dir, out hit, Mathf.Infinity)){
			Debug.DrawLine(start, hit.point, Color.red, 0f, false);
		}else{
			Debug.DrawRay(start, dir * 1000f, Color.red, 0f, false);
		}
		gameObject.SetActive(true);
	}
}
