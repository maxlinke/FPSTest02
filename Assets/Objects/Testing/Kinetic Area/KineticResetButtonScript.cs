using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KineticResetButtonScript : MonoBehaviour, IInteractable {

	public string message;
	public GameObject startStopResetObject;
	private IStartStopReset ssr;

	void Start(){
		ssr = startStopResetObject.GetComponent<IStartStopReset>();
	}

	public void Interact(GameObject other){
		ssr.Reset();
	}

	public string GetDescription(){
		return message;
	}

}
