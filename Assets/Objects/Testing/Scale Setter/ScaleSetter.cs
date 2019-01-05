using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleSetter : MonoBehaviour, IInteractable {

	[SerializeField] float scale;
	
	public string GetDescription () {
		return string.Format("Set scale to {0:F1}x", scale);
	}

	public void Interact (GameObject other) {
		other.transform.localScale = new Vector3(scale, scale, scale);
	}

}
