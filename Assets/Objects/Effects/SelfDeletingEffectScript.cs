using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDeletingEffectScript : MonoBehaviour {

	public ParticleSystem mainParticleSystem;
	public float checkFrequency;

	void Start () {
		StartCoroutine(CheckForDeletion());
	}
	
	void Update () {
		
	}


	IEnumerator CheckForDeletion(){
		while(true){
			yield return new WaitForSeconds(checkFrequency);
			if(!mainParticleSystem.IsAlive(true)){
				Destroy(this.gameObject);
			}
		}
	}

}
