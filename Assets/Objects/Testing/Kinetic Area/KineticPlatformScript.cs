using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KineticPlatformScript : MonoBehaviour, IStartStopReset {

	public GameObject platform;
	public Rigidbody rb;

	public Transform startPoint;
	public Transform endPoint;

	public float acceleration;

	private Vector3 vector;
	private bool running;
	private float velocity;

	void Start () {
		running = false;
		velocity = 0f;
		vector = endPoint.transform.position - startPoint.transform.position;
	}
	
	void Update () {
		
	}

	void FixedUpdate(){
		if(running){
			velocity += (acceleration * Time.fixedDeltaTime);
			rb.MovePosition(platform.transform.position + (vector.normalized * velocity * Time.fixedDeltaTime));
			if(Vector3.Distance(platform.transform.position, startPoint.transform.position) >= vector.magnitude){
				Stop();
			}
		}
	}

	public void Begin(){
		if(!running) running = true;
	}

	public void Stop(){
		running = false;
		velocity = 0f;
	}

	public void Reset(){
		Stop();
		platform.transform.position = startPoint.transform.position;
		platform.transform.rotation = startPoint.transform.rotation;
	}

}
