using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallDropButtonScript : MonoBehaviour, IInteractable {

	public GameObject ballPrefab;
	private GameObject ballInstance;

	public GameObject instantiatePoint;

	public void Interact(GameObject other){
		if(ballInstance != null) DestroyBallInstance();
		InstantiateNewBall();
	}

	public string GetDescription(){
		return "Drop 1 ton ball";
	}

	private void DestroyBallInstance(){
		Destroy(ballInstance);
		ballInstance = null;
	}

	private void InstantiateNewBall(){
		ballInstance = Instantiate(ballPrefab) as GameObject;
		ballInstance.transform.position = instantiatePoint.transform.position;
	}

}
