using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScaleSetter : MonoBehaviour {

	[System.Serializable]
	class KeyTimePair {
		public KeyCode key;
		public float timeScale;
	}

	[SerializeField] KeyTimePair[] pairs;
	
	void Update () {
		for(int i=0; i<pairs.Length; i++){
			if(Input.GetKeyDown(pairs[i].key)) Time.timeScale = pairs[i].timeScale;
		}
	}
}
