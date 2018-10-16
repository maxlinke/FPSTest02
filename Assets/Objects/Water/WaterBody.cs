using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WaterBody : MonoBehaviour {

	private const float waterDragVeclocityBoundary = 12f;
	private const float waterDrag = 256;

	public float waterLevel;
	private List<Rigidbody> rigidbodiesInWater;

	void Start () {
		if(!TagManager.CompareTag("Water", this.gameObject)){
			throw new MissingComponentException("the gameobject <" + this.gameObject.name + "> needs to be tagged \"Water\"");
		}
		rigidbodiesInWater = new List<Rigidbody>();
	}
	
	void Update () {
		
	}

	void FixedUpdate(){
		int rbCount = rigidbodiesInWater.Count;
		for(int i=0; i<rbCount; i++){
			Rigidbody otherRB = rigidbodiesInWater[i];
			AddBuoyancyForceToRigidbody(otherRB);
			DecelerateRigidbody(otherRB);
		}
		rigidbodiesInWater.Clear();
	}

	void OnTriggerEnter(Collider collider){
		
	}

	void OnTriggerStay(Collider collider){
		Rigidbody otherRB = collider.attachedRigidbody;
		if(!rigidbodiesInWater.Contains(otherRB)){
			rigidbodiesInWater.Add(otherRB);
		}
	}

	void OnTriggerExit(Collider collider){

	}

	private void DecelerateRigidbody(Rigidbody otherRB){
		float decelFactor;
		float velMag = otherRB.velocity.magnitude;
		if(velMag <= waterDragVeclocityBoundary){
			decelFactor = Mathf.Lerp(0.0f, 0.2f, Mathf.Clamp01(velMag / waterDragVeclocityBoundary));
		}else{
			decelFactor = Mathf.Lerp(0.2f, 1f, Mathf.Clamp01((velMag - waterDragVeclocityBoundary) / (2f * waterDragVeclocityBoundary)));
		}
		Vector3 decelVector = otherRB.velocity.normalized * decelFactor * -waterDrag * Time.fixedDeltaTime;
		if(decelVector.magnitude > velMag) decelVector = -otherRB.velocity;
		otherRB.velocity += decelVector;
	}

	private void AddBuoyancyForceToRigidbody(Rigidbody otherRB){
		float gravityOnObject = Physics.gravity.magnitude * otherRB.mass;
		float buoyancy = Mathf.Clamp01(waterLevel - otherRB.transform.position.y);
		buoyancy *= 3 * otherRB.drag * gravityOnObject;
		otherRB.AddForceAtPosition(Vector3.up * buoyancy, otherRB.transform.position, ForceMode.Force);
	}

}
