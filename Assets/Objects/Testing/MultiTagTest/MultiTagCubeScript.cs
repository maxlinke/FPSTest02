using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiTagCubeScript : MonoBehaviour, IInteractable {

	public Rigidbody rb;

	/*
	int index;

	public void Interact(GameObject other){
		index = (index + 1) % 2;
		if(index == 0) transform.localScale = Vector3.one;
		else transform.localScale = Vector3.one * 0.9f;
	}
	*/

	public void Interact(GameObject other){
		rb.AddTorque(Random.insideUnitSphere * Random.Range(10, 100));
	}

	public string GetDescription(){
		return "Spin";
	}

}
