using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 
 * IMPORTANT 
 * 
 * the trigger's y-level MUST not be higher than the one of the water body
 * otherwise swimming will be a bit buggy (can't go down unless you crouch)
 * 
 * the collider is 0.1f is "thickness"
 * don't forget to position the surface accordingly if using planes
 * otherwise just use a cube i guess
 * 
 * it's also a good idea to shift the center of mass of the box collider (as i think you're using) by -.05f on the y axis to make everything easier
 * 
 */

[RequireComponent(typeof(Collider))]
public class WaterSurfaceScript : MonoBehaviour {

	public Collider col;
	public GameObject smallSplash;
	public GameObject normalSplash;

	void Start () {
		if(smallSplash == null || normalSplash == null || col == null){
			Debug.DrawRay(transform.position, Vector3.up * 100f, Color.red, Mathf.Infinity, false);
			DebugDrawHelper.DrawSphere(transform.position, 0.05f, Color.red, Mathf.Infinity);
			throw new MissingReferenceException("not all references are properly set on \"" + gameObject.name + "\"");
		}
	}
	
	void Update () {
		
	}

	void OnTriggerEnter(Collider collider){
		if(!collider.isTrigger){
			Rigidbody otherRB = collider.attachedRigidbody;
			if(otherRB != null){
				if(otherRB.velocity.y < 0f){
					float kineticEnergy = (otherRB.mass / 2f) * otherRB.velocity.sqrMagnitude;
					InstantiateSplash(col.ClosestPoint(otherRB.worldCenterOfMass), kineticEnergy);
					//TODO sound
				}
			}
		}
	}

	void InstantiateSplash(Vector3 pos, float kineticEnergy){
		GameObject splash;
		if(kineticEnergy <= 20f){
			splash = Instantiate(smallSplash) as GameObject;
		}else{
			splash = Instantiate(normalSplash) as GameObject;
		}
		splash.transform.position = pos;	//TODO connect the water surface to the water body (no watersurface without waterbody)
		//instantiate at waterlevel + a little bit up
		splash.transform.up = transform.up;
		float lerpFactor = Mathf.Clamp01((kineticEnergy - 20f) / (9980f));
		float scale = Mathf.Lerp(0.5f, 2f, lerpFactor);
		ScaleWithAllChildren(splash.transform, scale);
	}

	void ScaleWithAllChildren(Transform otherTransform, float scale){
		otherTransform.localScale = Vector3.one * scale;
		int childCount = otherTransform.childCount;
		for(int i=0; i<childCount; i++){
			ScaleWithAllChildren(otherTransform.GetChild(i), scale);
		}
	}


}
