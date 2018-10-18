using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyCodeLogger : MonoBehaviour {

	void Start () {
		
	}
	
	void Update () {
		if(Input.anyKeyDown){
			foreach(KeyCode kcode in System.Enum.GetValues(typeof(KeyCode))){
				if (Input.GetKeyDown(kcode)){
					Debug.Log(kcode);
//					KeyCode parsed = KeyCodeUtils.ParseFromString(kcode.ToString());
//					KeyCode controllerFixed = KeyCodeUtils.RemoveControllerNumber(kcode);
//					Debug.Log(KeyCodeUtils.ToString(controllerFixed));
				}
			}
		}
	}
}
